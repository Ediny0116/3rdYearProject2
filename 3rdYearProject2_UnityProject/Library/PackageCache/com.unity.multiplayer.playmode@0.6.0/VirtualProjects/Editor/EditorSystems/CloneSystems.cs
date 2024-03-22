using System;
using System.Diagnostics;
using Unity.Multiplayer.Playmode.Common.Runtime;
using UnityEditor;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class CloneSystems
    {
        const string k_InitializeMessageSent = "vp_InitializeMessageSent";
        string m_IsCloneRequestMessageReceived = nameof(m_IsCloneRequestMessageReceived);

        internal CloneApplicationEvents CloneApplicationEvents { get; }
        internal AssetImportEvents AssetImportEvents { get; }
        internal SceneEvents SceneEvents { get; }

        internal CloneSystems()
        {
            CloneApplicationEvents = new CloneApplicationEvents();
            AssetImportEvents = new AssetImportEvents();
            SceneEvents = new SceneEvents();
        }

        internal void Listen(CloneContext vpContext)
        {
            /*
             * These system classes are simply an aggregation of logic and other events
             *
             * Its only purpose is to forward events to the Internal Runtimes, Workflows, and MultiplayerPlaymode (UI)
             */
            if (!SessionState.GetBool(k_InitializeMessageSent, false))
            {
                SessionState.SetBool(k_InitializeMessageSent, true);
                vpContext.MessagingService.Broadcast(
                    new CloneInitializedMessage(VirtualProjectsEditor.CloneIdentifier));
            }


            const int durationUntilKillCloneOneSecond = 1;
            const int durationUntilPlaymodeThreeSeconds = 3;
            var playmodeStartTime = DateTime.UtcNow;
            var killCloneStartTime = DateTime.UtcNow;
            var hasParsedMainEditorId = int.TryParse(VirtualProjectsEditor.MainEditorProcessId, out var mainProcessId);
            if (!hasParsedMainEditorId)
            {
                MppmLog.Warning("Unable to parse the main editors process id.");
            }

            EditorApplication.update += () =>
            {
                vpContext.MessagingService.HandleUpdate();
                
                // Watch the main editor and close ourselves if it is closed
                // The Main Editor fundamentally closes all its child editors
                // since it coordinates Asset Syncing at the API level
                var hasExceededKillTime = (DateTime.UtcNow - killCloneStartTime).TotalSeconds >= (float)durationUntilKillCloneOneSecond;
                if (hasParsedMainEditorId && hasExceededKillTime)
                {
                    killCloneStartTime = DateTime.UtcNow;
                    if (!InternalProcessRunner.IsRunning(mainProcessId))
                    {
                        CloneApplicationEvents.InvokeParentQuitting();
                    }
                }
                
                // This is specifically for the Request Clone Info system
                // Unlike MPPM, there are no other consumers of this 'event' in the system... yet
                var hasExceeded = (DateTime.UtcNow - playmodeStartTime).TotalSeconds >= (float)durationUntilPlaymodeThreeSeconds;
                if(CommandLineParameters.ReadRequestedClonePlaymode() && hasExceeded)
                {
                    EditorApplication.isPlaying = true;
                }

                if (SessionState.GetBool(m_IsCloneRequestMessageReceived, false))
                {
                    SessionState.SetBool(m_IsCloneRequestMessageReceived, false);
                    vpContext.MessagingService.Broadcast(new CloneEditorStateMessage(VirtualProjectsEditor.CloneIdentifier, EditorApplication.isPlaying));
                }
            };

            vpContext.MessagingService.Receive<TriggerCloneRefreshMessage>(message =>
            {
                AssetImportEvents.InvokeRequestImport(message.DidDomainReload, message.NumAssetsChanged);
            });
            vpContext.MessagingService.Receive<RequestCloneInfoMessage>(_ =>
            {
                SessionState.SetBool(m_IsCloneRequestMessageReceived, true);
            });
            vpContext.MessagingService.Receive<HeartbeatRequestMessage>(_ =>
            {
                var heartbeat = new HeartbeatResponseMessage(VirtualProjectsEditor.CloneIdentifier, Debugger.IsAttached);
                vpContext.MessagingService.Broadcast(heartbeat);
            });
            vpContext.MessagingService.Receive<SceneHierarchyChangedMessage>(message => SceneEvents.InvokeSceneHierarchyChanged(message.SceneHierarchy));
            vpContext.MessagingService.Receive<SceneSavedMessage>(message => SceneEvents.InvokeSceneSaved(message.SceneSaved));
        }
    }
}