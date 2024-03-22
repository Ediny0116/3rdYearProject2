using System;
using Unity.Multiplayer.Playmode.InternalBridge.Editor;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;
using UnityEditor;
#if UNITY_USE_MULTIPLAYER_ROLES
using Unity.Multiplayer.Editor;
#endif

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class ClonedPlayerSystems
    {
        const string k_InitializeMessageSent = "mppm_InitializeMessageSent";
        
        // This is done because something like a EditorApplication.delayCall does not survive a domain reload
        // that could occur when going in or out of playmode
        const string k_FrameAfterPlaymodeMessage = "frameAfterPlaymodeMessage";
        
        readonly PlaymodeMessageQueue m_PlaymodeMessageQueue;
        
        internal PlaymodeEvents PlaymodeEvents { get; }

        internal ClonedPlayerApplicationEvents ClonedPlayerApplicationEvents { get; }

        internal ClonedPlayerSystems()
        {
            ClonedPlayerApplicationEvents = new ClonedPlayerApplicationEvents();
            PlaymodeEvents = new PlaymodeEvents();
            m_PlaymodeMessageQueue = new PlaymodeMessageQueue();
        }

        internal void Listen(WorkflowCloneContext mppmContext, CloneContext vpContext)
        {
            /*
             * These system classes are simply an aggregation of logic and other events
             *
             * Its only purpose is to forward events to the Internal Runtimes, Workflows, and MultiplayerPlaymode (UI)
             */
            var messagingService = vpContext.MessagingService;
            if (!SessionState.GetBool(k_InitializeMessageSent, false))
            {
                SessionState.SetBool(k_InitializeMessageSent, true);
                messagingService.Broadcast(
                    new PlayerInitializedMessage(VirtualProjectsEditor.CloneIdentifier));
            }

            EditorApplicationProxy.RegisterUpdateMainWindowTitle(applicationTitleDescriptor =>
            {
                ClonedPlayerApplicationEvents.InvokeEditorStarted(applicationTitleDescriptor);
            });
            EditorApplication.delayCall += () => { ClonedPlayerApplicationEvents.InvokePlayerActive(); };
            EditorApplication.pauseStateChanged += pauseState =>
            {
                if (pauseState == PauseState.Paused && EditorApplication.isPlaying)
                {
                    // Note: Just handle any case (including pause on error)
                    // Where the player ends up paused.
                    // Pause all the other players as well
                    ClonedPlayerApplicationEvents.InvokeClonePlayerPaused();
                }
            };

            messagingService.Receive<PauseMessage>(_ => m_PlaymodeMessageQueue.AddEvent(PlayModeMessageTypes.Pause));
            messagingService.Receive<UnpauseMessage>(_ => m_PlaymodeMessageQueue.AddEvent(PlayModeMessageTypes.Unpause));
            messagingService.Receive<StepMessage>(_ => m_PlaymodeMessageQueue.AddEvent(PlayModeMessageTypes.Step));
            messagingService.Receive<PlayMessage>(_ => m_PlaymodeMessageQueue.AddEvent(PlayModeMessageTypes.Play));
            messagingService.Receive<StopMessage>(_ => m_PlaymodeMessageQueue.AddEvent(PlayModeMessageTypes.Stop));
            messagingService.Receive<OpenPlayerWindowMessage>(_ => ClonedPlayerApplicationEvents.InvokeOpenPlayerWindow());

            ConsoleWindowUtility.consoleLogsChanged += ClonedPlayerApplicationEvents.InvokeConsoleLogMessagesChanged;

            // Because we don't want to enter and exit playmode on the same frame (or immediately right after each other)
            // We instead wait on an interval to find a moment when the editor is not busy
            // At that moment we then process events in the order that they were added (with duplicates removed)
            const float delayHalfSecond = 0.5f;
            var startTime = DateTime.UtcNow;
            EditorApplication.update += () =>
            {
                if (SessionState.GetBool(k_FrameAfterPlaymodeMessage, false))
                {
                    SessionState.SetBool(k_FrameAfterPlaymodeMessage, false);
                    ClonedPlayerApplicationEvents.InvokeFrameAfterPlaymodeMessage();
                }

                var hasExceeded = (DateTime.UtcNow - startTime).TotalSeconds >= delayHalfSecond;
                if (!hasExceeded)
                {
                    return;
                }
                
                if (ModeSwitcher.CurrentWindow != null)
                {   
                    // Just save the view every second while the window is open just in case
                    // the process exits OR they click the close button (which currently just does a minimize)
                    //
                    // Now when that happens we will be able to load up the last positions of their view just fine
                    ModeSwitcher.SaveCurrentView();
                }
                
#if UNITY_USE_MULTIPLAYER_ROLES
                if(!EditorApplication.isPlaying) 
                {
                    foreach (var player in UnityPlayer.GetPlayers(mppmContext.SystemDataStore))
                    {
                        if(player.TypeDependentPlayerInfo.VirtualProjectIdentifier == VirtualProjectsEditor.CloneIdentifier)
                        {
                            EditorMultiplayerRolesManager.ActiveMultiplayerRoleMask = (MultiplayerRoleFlags)player.MultiplayerRole;       
                            break;
                        }
                    }
                }
#endif

                startTime = DateTime.UtcNow;
                {
                    var isEditorBusy = EditorApplication.isCompiling 
                                       || EditorApplication.isUpdating 
                                       || EditorApplication.isPlaying != EditorApplication.isPlayingOrWillChangePlaymode;   // See https://docs.unity3d.com/ScriptReference/PlayModeStateChange.ExitingEditMode.html and https://docs.unity3d.com/ScriptReference/PlayModeStateChange.ExitingPlayMode.html
                    if (!isEditorBusy 
                        && m_PlaymodeMessageQueue.ReadEvent(out var pm))
                    {
                        switch (pm)
                        {
                            case PlayModeMessageTypes.Pause:
                                PlaymodeEvents.InvokePause();
                                break;
                            case PlayModeMessageTypes.Unpause:
                                PlaymodeEvents.InvokeUnpause();
                                break;
                            case PlayModeMessageTypes.Step:
                                PlaymodeEvents.InvokeStep();
                                break;
                            case PlayModeMessageTypes.Play:
                                PlaymodeEvents.InvokePlay();
                                SessionState.SetBool(k_FrameAfterPlaymodeMessage, true);
                                break;
                            case PlayModeMessageTypes.Stop:
                                PlaymodeEvents.InvokeStop();
                                SessionState.SetBool(k_FrameAfterPlaymodeMessage, true);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException($"{nameof(pm)}:{pm}");
                        }
                    }
                }
            };
        }
    }
}