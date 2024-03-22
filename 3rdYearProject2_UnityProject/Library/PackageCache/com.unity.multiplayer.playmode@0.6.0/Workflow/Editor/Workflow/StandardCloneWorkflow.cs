using Unity.Multiplayer.Playmode.Common.Runtime;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;
using UnityEditor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class StandardCloneWorkflow
    {
        enum ScriptChangesDuringPlayOptions
        {
            RecompileAndContinuePlaying = 0,
            RecompileAfterFinishedPlaying = 1,
            StopPlayingAndRecompile = 2,
        }   // From ScriptChangesDuringPlayOptions in EditorApplication.cs
        
        const string k_ScriptCompilationDuringPlay = "ScriptCompilationDuringPlay";
        const string k_OpenGameViewOnPlay = "OpenGameViewOnEnteringPlayMode";
        
        public void Initialize(WorkflowCloneContext mppmContext, CloneContext vpContext)
        {
            vpContext.CloneSystems.AssetImportEvents.RequestImport += (didDomainReload, numAssetsChanged) =>
            {
                // This prevents a race condition where the clone goes into playmode first and the main editor does a domain reload before it goes into playmode
                if (EditorPrefs.GetInt(k_ScriptCompilationDuringPlay) == (int)ScriptChangesDuringPlayOptions.StopPlayingAndRecompile && didDomainReload && numAssetsChanged > 0)
                {
                    MppmLog.Debug("MPPM Clone Stop Playing And Recompile");
#if UNITY_2023_2_OR_NEWER
                    ModeSwitcher.SaveCurrentView();
                    // Close the existing window if its open since we are about to reopen once we switch to view
                    // NOTE: We close the window AFTER saving the window layout!
                    ModeSwitcher.CloseCurrentWindow();
#endif
                    EditorApplication.ExitPlaymode();
                }
            };
            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.PlayerActive += ()=>
            {
#if UNITY_MP_TOOLS_DEV_LOGMESSAGES
                // Do not update log counts when message logging is enabled,
                // otherwise logs will continually spam every time the message is sent
#else
#if UNITY_2023_2_OR_NEWER
                ConsoleWindowUtility.GetConsoleLogCounts(out var error, out var warning, out var log);
                vpContext.MessagingService.Broadcast(new UpdateCloneLogCountsMessage(VirtualProjectsEditor.CloneIdentifier, new LogCounts{Logs = log, Warnings = warning, Errors = error}));
#endif
#endif
            };
            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.PlayerTitleRename += editorTitleUpdater =>
            {
                const string unknownPlayerWindowTitle = "Unknown Player";
                var hasPlayer = Filters.FindFirstPlayerWithVirtualProjectsIdentifier(mppmContext.SystemDataStore.LoadAllPlayerJson(),VirtualProjectsEditor.CloneIdentifier, out var player);
                if (hasPlayer)
                {
                    var tag = player.Tags.Count <= 0 ? string.Empty : $" [{string.Join('|', player.Tags)}]";
                    editorTitleUpdater.title = $"{player.Name}{tag}";
                }
                else
                {
                    editorTitleUpdater.title = unknownPlayerWindowTitle;
                }
            };
            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.OpenPlayerWindow += () =>
            {
                var viewFlags = EditorApplication.isPlaying
                    ? CloneDataFile.LoadFromFile(mppmContext.CloneDataFile).PlayModeLayoutFlags
                    : CloneDataFile.LoadFromFile(mppmContext.CloneDataFile).EditModeLayoutFlags;
#if UNITY_2023_2_OR_NEWER
                ModeSwitcher.SaveCurrentView();
                // Close the existing window if its open since we are about to reopen once we switch to view
                // NOTE: We close the window AFTER saving the window layout!
                ModeSwitcher.CloseCurrentWindow();
#endif
                ModeSwitcher.SwitchToView(viewFlags);
            };
            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.ConsoleLogMessagesChanged +=  ()=> 
            {
#if UNITY_MP_TOOLS_DEV_LOGMESSAGES
                    // Do not update log counts when message logging is enabled,
                    // otherwise logs will continually spam every time the message is sent
#else
                ConsoleWindowUtility.GetConsoleLogCounts(out var error, out var warning, out var log);
                vpContext.MessagingService.Broadcast(new UpdateCloneLogCountsMessage(VirtualProjectsEditor.CloneIdentifier, new LogCounts{Logs = log, Warnings = warning, Errors = error}));
#endif
            };

            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.PlayerPaused +=
                ()=> vpContext.MessagingService.Broadcast(new PlayerPausedOnCloneMessage());

            
            mppmContext.ClonedPlayerSystems.PlaymodeEvents.Play += () =>
            {
                /*
                 * The main editor keeps its functionality of starting on the scene tab and switching to game view when going into play mode (if OpenGameViewOnPlay is active)
                 * However, the clones always run with which ever view was selected from layout (since OpenGameViewOnPlay is always off)
                 *
                 * Edges cases:
                 * This is currently covering the main editor crashing (it uses the non deleted key in the local data store)
                 * The clone crashing (it just always runs with OpenGameViewOnPlay off)
                 * As well as these crashes happening in either order (main should be able to crash first or last and etc...)
                 *
                 * See MainCachePlayOnGameView for main editor behaviour
                 */
                
                // The following string and property needs to match the functionality
                //  of PlayModeView.openWindowOnEnteringPlayMode currently
        
                // We are a clone so we always go in as false. 
                // The main editor will restore the previous mode
                EditorPrefs.SetBool(k_OpenGameViewOnPlay, false);
                    
                // Save the current view first
#if UNITY_2023_2_OR_NEWER
                ModeSwitcher.SaveCurrentView();
                // Close the existing window if its open since we are about to reopen once we switch to view
                // NOTE: We close the window AFTER saving the window layout!
                ModeSwitcher.CloseCurrentWindow();
#endif
                EditorApplication.EnterPlaymode(); // intentionally after saving the view
            };
            mppmContext.ClonedPlayerSystems.PlaymodeEvents.Pause += () => { EditorApplication.isPaused = true; };
            mppmContext.ClonedPlayerSystems.PlaymodeEvents.Step += () => { EditorApplication.Step(); };
            mppmContext.ClonedPlayerSystems.PlaymodeEvents.Unpause += () => { EditorApplication.isPaused = false; };
            mppmContext.ClonedPlayerSystems.PlaymodeEvents.Stop += () =>
            {
#if UNITY_2023_2_OR_NEWER
                ModeSwitcher.SaveCurrentView();
                // Close the existing window if its open since we are about to reopen once we switch to view
                // NOTE: We close the window AFTER saving the window layout!
                ModeSwitcher.CloseCurrentWindow();
#endif

                EditorApplication.ExitPlaymode(); // intentionally after saving the view
            };

            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.FrameAfterPlaymodeMessage += () =>
            {
                ModeSwitcher.SwitchToView(EditorApplication.isPlaying
                    ? CloneDataFile.LoadFromFile(mppmContext.CloneDataFile).PlayModeLayoutFlags
                    : CloneDataFile.LoadFromFile(mppmContext.CloneDataFile).EditModeLayoutFlags);
            };
        }
    }
}