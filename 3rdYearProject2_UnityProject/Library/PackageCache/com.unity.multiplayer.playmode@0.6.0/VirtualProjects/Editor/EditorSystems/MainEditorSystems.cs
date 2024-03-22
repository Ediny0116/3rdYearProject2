using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class MainEditorSystems
    {
        public const int k_UnresponsiveTimeoutSeconds = 10;

        internal MainApplicationEvents ApplicationEvents { get; }
        internal AssetImportEvents AssetImportEvents { get; }
        internal SceneEvents SceneEvents { get; }

        internal MainEditorSystems()
        {
            ApplicationEvents = new MainApplicationEvents();
            AssetImportEvents = new AssetImportEvents();
            SceneEvents = new SceneEvents();
        }

        internal void Listen(MainEditorContext vpContext)
        {
            /*
             * These system classes are simply an aggregation of logic and other events
             *
             * Its only purpose is to forward events to the Internal Runtimes, Workflows, and MultiplayerPlaymode (UI)
             */

            const int cloneResponsivenessCheckPeriodOneSecond = 1;
            const int cloneHeartbeatRequestPeriodTwoSeconds = 2;
            var cloneResponsiveCheckStartTime = DateTime.UtcNow;
            var cloneHeartbeatRequestStartTime = DateTime.UtcNow;
            var timeSinceStartup = DateTime.UtcNow;

            KeepAlive(Process.GetCurrentProcess().Id);  // need to keep the main editor alive so it can send heartbeats properly

            EditorApplication.update += () =>
            {
                // Send heartbeat requests to the clones. This will prompt the clones to send back a
                // heartbeat response which is what we use to determine liveness.
                var hasExceededLiveness = (DateTime.UtcNow - cloneHeartbeatRequestStartTime).TotalSeconds >= (float)cloneHeartbeatRequestPeriodTwoSeconds;
                if (hasExceededLiveness)
                {
                    cloneHeartbeatRequestStartTime = DateTime.UtcNow;
                    /* * * * */
                    
                    var hasProcess = false;
                    foreach (var project in vpContext.VirtualProjectsApi.GetProjects(VirtualProjectsApi.k_FilterAll))
                    {
                        if (project.ProcessId != -1)
                        {
                            KeepAlive(project.ProcessId);   // keep clones alive with syscall so that it can receive heart beats
                            hasProcess = true;
                        }
                    }

                    if (hasProcess)
                    {
                        vpContext.MessagingService.Broadcast(new HeartbeatRequestMessage(), () =>
                        {
                            // empty to ensure replies
                        });   
                    }
                }

                // Check if any of the clones is unresponsive. Note that this check is disabled if
                // the main editor or the clone has a debugger attached. The former because we could
                // have been stopped for a while without processing any heartbeats, and the latter
                // because if a clone is being debugged it's expected that we won't receive anything
                // for it. Note that this approach is not foolproof (it's still possible to get an
                // unresponsive clone with a debugger if the timing is right), but it's a good proxy
                // and worst case we just get an annoying popup about the unresponsive clone.
                var hasExceeded = (DateTime.UtcNow - cloneResponsiveCheckStartTime).TotalSeconds >= (float)cloneResponsivenessCheckPeriodOneSecond;
                if (hasExceeded && !Debugger.IsAttached)
                {
                    cloneResponsiveCheckStartTime = DateTime.UtcNow;
                    /* * * * */
                    
                    foreach (var project in vpContext.VirtualProjectsApi.GetProjects(VirtualProjectsApi.k_FilterAll))
                    {
                        var hasState = vpContext.StateRepository.TryGetValue(project.Identifier, out var state);
                        
                        if (!hasState) continue;
                        if (state.LastHeartbeatReceived == default) continue;
                        if (state.IsDebuggerAttached) continue;
                        if (project.EditorState != EditorState.Launched) continue;
                        if ((timeSinceStartup - state.LastHeartbeatReceived).Seconds <= k_UnresponsiveTimeoutSeconds) continue;
                        
                        ApplicationEvents.InvokeCloneUnresponsive(project.Identifier);
                    }
                }

                vpContext.MessagingService.HandleUpdate();
            };
            EditorApplication.quitting += () => ApplicationEvents.InvokeQuitting();

            AssetDatabaseCallbacks.OnPostprocessAllAssetsCallback += (didDomainReload, numAssetsChanged) =>
            {
                // If the main editor is still compiling, there will be another refresh/domain reload when it’s complete.
                // So we won’t bother getting the clone to do an intermediate refresh that is likely to have out of date assets that will trigger the assert message.
                if (!EditorApplication.isCompiling)
                    AssetImportEvents.InvokeRequestImport(didDomainReload, numAssetsChanged);
            };

            vpContext.MessagingService.Receive<CloneInitializedMessage>(identifier =>
            {
                // Clone being initialized can be seen as receiving our first heartbeat.
                ApplicationEvents.InvokeCloneHeartbeatReceived(identifier.Identifier, false);
                
                if (!vpContext.RequestCloneInfo.ActiveClones.ContainsKey(identifier.Identifier))
                {
                    var state = new RequestCloneInfo.CloneInfo
                    {
                        IsCommunicative = true,
                    };
                    vpContext.RequestCloneInfo.ActiveClones.Create(identifier.Identifier, state);
                }
                else
                {
                    vpContext.RequestCloneInfo.ActiveClones.Update(identifier.Identifier, state=>state.IsCommunicative = true, out _);
                }
            });
            vpContext.MessagingService.Receive<HeartbeatResponseMessage>(message =>
            {
                ApplicationEvents.InvokeCloneHeartbeatReceived(message.Identifier, message.DebuggerAttached);
            });
            vpContext.MessagingService.Receive<CloneEditorStateMessage>(message =>
            {
                if (!vpContext.RequestCloneInfo.ActiveClones.ContainsKey(message.Identifier))
                {
                    var state = new RequestCloneInfo.CloneInfo
                    {
                        IsPlaying = message.IsPlaying,
                    };
                    vpContext.RequestCloneInfo.ActiveClones.Create(message.Identifier, state);
                }
                else
                {
                    vpContext.RequestCloneInfo.ActiveClones.Update(message.Identifier, state=>state.IsPlaying = message.IsPlaying, out _);
                }
            });
            

            EditorSceneManager.activeSceneChangedInEditMode += (_, _) =>
            {
                EditorApplication.delayCall += () =>
                {
                    SceneEvents.InvokeSceneHierarchyChanged(SceneHierarchy.FromCurrentEditorSceneManager());
                };
            };

            EditorSceneManager.sceneClosed += _ =>
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.delayCall += () =>
                    {
                        SceneEvents.InvokeSceneHierarchyChanged(SceneHierarchy.FromCurrentEditorSceneManager());
                    };
                }
            };

            EditorSceneManager.sceneOpened += (_, _) =>
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.delayCall += () =>
                    {
                        SceneEvents.InvokeSceneHierarchyChanged(SceneHierarchy.FromCurrentEditorSceneManager());
                    };
                }
            };

            EditorSceneManager.sceneSaved += scene =>
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.delayCall += () => { SceneEvents.InvokeSceneSaved(scene.path); };
                }
            };
        }
        
        static void KeepAlive(int processID)
        {
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                var psi = new ProcessStartInfo("kill", $"-CONT {processID}")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };
                Process.Start(psi);
            }
        }
    }
}