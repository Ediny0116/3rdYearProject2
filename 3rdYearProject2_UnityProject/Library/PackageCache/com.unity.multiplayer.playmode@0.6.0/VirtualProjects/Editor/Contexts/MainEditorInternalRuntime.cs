using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Multiplayer.Playmode.Common.Runtime;
using UnityEngine;
#if UNITY_MP_TOOLS_DEV
using UnityEditor;
#endif

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class MainEditorInternalRuntime
    {
        const int k_MessageDispatchRateMs = 25;
        const int k_TimeoutFailedToSyncCloneMs = 30_000; // 30 seconds

        internal delegate DateTime CurrentTimeProviderFunc();

        internal delegate void ThreadSleepFunc(int ms);

        internal delegate void ThreadBlockingOperation(
            Func<bool> completionCondition,
            int timeoutMs,
            Action tickFunction,
            int tickRateMs);
        
#if UNITY_MP_TOOLS_DEV
        static readonly Dictionary<VirtualProjectIdentifier, double> ProcessLaunchTimes = new();
#endif
        public void HandleEvents(MainEditorContext vpContext)
        {
            vpContext.MainEditorSystems.ApplicationEvents.CloneLaunching += (identifier, args) =>
            {
#if UNITY_MP_TOOLS_DEV
                ProcessLaunchTimes[identifier] = EditorApplication.timeSinceStartup;
#endif
                vpContext.StateRepository.Update(identifier, state => { state.LaunchArgs = args; }, out _);
            };
            vpContext.MainEditorSystems.ApplicationEvents.CloneHeartbeatReceived += (identifier, debuggerAttached) =>
            {
                if (!vpContext.StateRepository.ContainsKey(identifier))
                {
                    var state = new VirtualProjectState
                    {
                        FirstHeartbeatReceived = DateTime.UtcNow,
                        LastHeartbeatReceived = DateTime.UtcNow,
                        IsDebuggerAttached = debuggerAttached,
                    };
                    vpContext.StateRepository.Create(identifier, state);
                }
                else
                {
                    vpContext.StateRepository.Update(
                        identifier,
                        state =>
                        {
                            state.FirstHeartbeatReceived = DateTime.UtcNow;
                            state.LastHeartbeatReceived = DateTime.UtcNow;
                            state.IsDebuggerAttached = debuggerAttached;
                        },
                        out _);
                }

#if UNITY_MP_TOOLS_DEV
                if (ProcessLaunchTimes.TryGetValue(identifier, out var startTime))
                {
                    var deltaTime = EditorApplication.timeSinceStartup - startTime;
                    MppmLog.Debug($"Clone {identifier} took {deltaTime} seconds to launch");
                    ProcessLaunchTimes.Remove(identifier);
                }
#endif
            };
            vpContext.MainEditorSystems.ApplicationEvents.CloneUnresponsive += identifier =>
            {
                if (vpContext.VirtualProjectsApi.TryGet(identifier, out var project, out _)
                    && vpContext.StateRepository.TryGetValue(identifier, out var state))
                {
                    var choice = CloneSyncModal.DisplayCloneSyncModal($"Clone {identifier} is unresponsive.");
                    CloneSyncModal.DefaultChoiceHandler(choice, project, state);

                    // If the user decided to do nothing, we must ensure that we don't immediately
                    // show the modal again if the clone still fails to send heartbeats. One (hacky)
                    // way to do this is to set the last received time in the future.
                    if (choice == CloneSyncModal.Choices.ContinueAnyways)
                    {
                        vpContext.StateRepository.Update(
                            identifier, s => s.LastHeartbeatReceived = DateTime.UtcNow + TimeSpan.FromHours(1), out _);
                    }
                }
            };
            vpContext.MainEditorSystems.ApplicationEvents.Quitting += () =>
            {
                // The Main Editor fundamentally closes all its child editors
                // since it coordinates Asset Syncing at the API level 
                foreach (var project in vpContext.VirtualProjectsApi.GetProjects(VirtualProjectsApi.k_FilterAll))
                {
                    project.Close(out _);
                }
            };
            vpContext.MainEditorSystems.AssetImportEvents.RequestImport += (didDomainReload, numAssetsChanged) =>
            {
                var runningClones = new List<VirtualProjectIdentifier>();
                foreach (var p in vpContext.VirtualProjectsApi.GetProjects(VirtualProjectsApi.k_FilterAll))
                {
                    if (p.EditorState == EditorState.Launched)
                    {
                        runningClones.Add(p.Identifier);
                    }
                }

                if (runningClones.Count == 0)
                {
                    MppmLog.Debug("No Virtual projects to sync assets with.");
                    return;
                }

                if (numAssetsChanged == 0)
                {
                    MppmLog.Debug("No assets to actually sync with clones.");
                    return;
                }

                if (!TrySynchronousCloneAssetImport(MessagingService.GetImportDelegates(vpContext.MessagingService), DefaultThreadBlockingOperationRun, runningClones, didDomainReload, numAssetsChanged,
                    out var failedClones))
                {
                    foreach (var failedCloneIdentifier in failedClones)
                    {
                        if (!vpContext.VirtualProjectsApi.TryGet(failedCloneIdentifier, out var project, out _)) continue;
                        if (vpContext.MainEditorSystems.AssetImportEvents.InvokeFailedImport(project.Identifier)) continue;
                        // NOTE: This needs to happen on clones even when not running with MPPM!
                            
                        // If MPPM is active then it can handle the UI action. If not then we handle it
                        
                        var hasFound = vpContext.StateRepository.TryGetValue(project.Identifier, out var state);
                        Debug.Assert(hasFound);
                        var choice = CloneSyncModal.DisplayCloneSyncModal($"Clone {project.Identifier} failed to sync.");
                        CloneSyncModal.DefaultChoiceHandler(choice, project, state);
                    }
                }
                else
                {
                    MppmLog.Debug("No Virtual projects to sync assets with.");
                }
            };

            vpContext.MainEditorSystems.SceneEvents.SceneHierarchyChanged += newSceneHierarchy =>
            {
                vpContext.MessagingService.Broadcast(new SceneHierarchyChangedMessage(newSceneHierarchy));
            };
            vpContext.MainEditorSystems.SceneEvents.SceneSaved += sceneSaved =>
            {
                if (string.IsNullOrWhiteSpace(sceneSaved))
                {
                    return;
                }
                vpContext.MessagingService.Broadcast(new SceneSavedMessage(sceneSaved));
            };
        }
        
        internal struct MessagingServiceSynchronousCloneAssetImportDelegates
        {
            internal delegate void DispatchMessages();
            internal delegate void Send(TriggerCloneRefreshMessage message, VirtualProjectIdentifier identifier,
                Action onAck = null, Action<string> onErrorOrTimeout = null);
        
            internal DispatchMessages DispatchMessagesFunc;
            internal Send SendFunc;
        }

        internal static bool TrySynchronousCloneAssetImport(
            MessagingServiceSynchronousCloneAssetImportDelegates messagingService,
            ThreadBlockingOperation threadBlockingOperationRunner,
            IReadOnlyCollection<VirtualProjectIdentifier> clones,
            bool didDomainReload,
            int numAssetsChanged,
            out IReadOnlyCollection<VirtualProjectIdentifier> failedClones)
        {
            var successfulCloneImports = new Dictionary<VirtualProjectIdentifier, bool>();

            // Tell the clones to perform asset import
            foreach (var cloneIdentifier in clones)
            {
                messagingService.SendFunc(
                    new TriggerCloneRefreshMessage(didDomainReload, numAssetsChanged),
                    cloneIdentifier,
                    onAck: () => successfulCloneImports[cloneIdentifier] = true,
                    onErrorOrTimeout: _ => successfulCloneImports[cloneIdentifier] = false);
            }

            // Block main thread until all clones have acknowledge (or time out)
            threadBlockingOperationRunner(
                completionCondition: () => AllClonesAreSuccessfulClones(clones, successfulCloneImports),
                timeoutMs: k_TimeoutFailedToSyncCloneMs,
                tickFunction: ()=>messagingService.DispatchMessagesFunc(),
                tickRateMs: k_MessageDispatchRateMs);

            // Treat any clone which did not respond as a failure and return
            var result = new List<VirtualProjectIdentifier>();
            foreach (var project in clones)
            {
                if (!successfulCloneImports.ContainsKey(project) || successfulCloneImports[project] == false)
                {
                    result.Add(project);
                }
            }

            failedClones = result;
            return result.Count == 0;
        }

        static bool AllClonesAreSuccessfulClones(IReadOnlyCollection<VirtualProjectIdentifier> clones, Dictionary<VirtualProjectIdentifier, bool> successfulCloneImports)
        {
            foreach (var c in clones)
            {
                if (!successfulCloneImports.ContainsKey(c)) return false;
            }
            return true;
        }

        static void DefaultThreadBlockingOperationRun(Func<bool> completionCondition, int timeoutMs, Action tickFunction, int tickRateMs)
        {
            ThreadBlockingOperationRun(() => DateTime.UtcNow, Thread.Sleep, completionCondition, timeoutMs, tickFunction, tickRateMs);
        }

        internal static void ThreadBlockingOperationRun(
            CurrentTimeProviderFunc currentTimeProviderFunc,
            ThreadSleepFunc threadSleepFunc,
            Func<bool> completionCondition,
            int timeoutMs,
            Action tickFunction,
            int tickRateMs)
        {
            var timeout = currentTimeProviderFunc() + TimeSpan.FromMilliseconds(timeoutMs);
            while (!completionCondition())
            {
                if (timeout <= currentTimeProviderFunc())
                {
                    return;
                }

                threadSleepFunc(tickRateMs);
                tickFunction?.Invoke();
            }
        }
    }
}