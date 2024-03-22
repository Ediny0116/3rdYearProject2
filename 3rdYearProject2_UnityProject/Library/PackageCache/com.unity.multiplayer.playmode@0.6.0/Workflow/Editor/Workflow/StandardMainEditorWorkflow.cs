using System;
using System.Collections.Generic;
using System.IO;
using Unity.Multiplayer.Playmode.Common.Editor;
using Unity.Multiplayer.Playmode.Common.Runtime;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_USE_MULTIPLAYER_ROLES
using Unity.Multiplayer.Editor;
#endif

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class StandardMainEditorWorkflow
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
         * See CloneDisablePlayOnGameView for clone behaviour
         */
        static bool OpenWindowOnEnteringPlayMode
        {
            get => EditorPrefs.GetBool(k_OpenGameViewOnPlay, true);
            set
            {
                if (EditorPrefs.GetBool(k_OpenGameViewOnPlay) != value)
                {
                    EditorPrefs.SetBool(k_OpenGameViewOnPlay, value);
                }
            }
        }
        
        // The following string and property needs to match the functionality
        //  of PlayModeView.openWindowOnEnteringPlayMode currently
        const string k_OpenGameViewOnPlay = "OpenGameViewOnEnteringPlayMode";
        const string k_OpenGameViewOnPlayCache = "OpenGameViewOnEnteringPlayModeCache";
        
        public void Initialize(WorkflowMainEditorContext mppmContext, MainEditorContext vpContext)
        {
            // If the editor closed unexpectedly we pull the value 
            // that was in memory before we went into playmode
            // in case the clones have altered it
            if (EditorPrefs.HasKey(k_OpenGameViewOnPlayCache)) 
            {
                OpenWindowOnEnteringPlayMode = EditorPrefs.GetBool(k_OpenGameViewOnPlayCache);
                EditorPrefs.DeleteKey(k_OpenGameViewOnPlayCache);
            }

            vpContext.MainEditorSystems.AssetImportEvents.FailedImport += identifier =>
            {
                // Override the default VP behaviour and show a more relevant modal
                var hasProject = vpContext.VirtualProjectsApi.TryGet(identifier, out var project, out _);
                var hasState = vpContext.StateRepository.TryGetValue(identifier, out var state);
                var hasPlayer = Filters.FindFirstPlayerWithVirtualProjectsIdentifier(mppmContext.SystemDataStore.LoadAllPlayerJson(), identifier, out var player);
                Debug.Assert(hasProject);
                Debug.Assert(hasState);
                Debug.Assert(hasPlayer);
                
                var choice = PlayerSyncModal.DisplayPlayerSyncModal($"{player.Name} failed to sync.");
                CloneSyncModal.DefaultChoiceHandler(choice, project, state);
            };

            mppmContext.MainPlayerSystems.ApplicationEvents.SceneChanged += (_, _) =>
            {
                if (EditorApplication.isPaused)
                {
                    vpContext.MessagingService.Broadcast(new PauseMessage());
                }
            };
            mppmContext.MainPlayerSystems.ApplicationEvents.LogCountsChanged += (identifier, logCounts) =>
            {
                if (!mppmContext.LogsRepository.ContainsKey(identifier))
                {
                    mppmContext.LogsRepository.Create(identifier, new BoxedLogCounts { LogCounts = logCounts, });
                }
                else
                {
                    mppmContext.LogsRepository.Update(identifier, playerLogs=> { playerLogs.LogCounts = logCounts; }, out _);
                }
            };
            mppmContext.MainPlayerSystems.ApplicationEvents.UIPollUpdate += () =>
            {
                /*****
                 * Keep the MPPM local cache of the state of the clones up to date
                 * (for the UI and other systems)
                 */
#if UNITY_USE_MULTIPLAYER_ROLES
                if(!EditorApplication.isPlaying) 
                {
                    foreach (var player in UnityPlayer.GetPlayers(mppmContext.SystemDataStore))
                    {
                        if(player.Type == PlayerType.Main)
                        {
                            EditorMultiplayerRolesManager.ActiveMultiplayerRoleMask = (MultiplayerRoleFlags)player.MultiplayerRole;       
                            break;
                        }
                    }
                }
#endif
                foreach (var player in UnityPlayer.GetPlayers(mppmContext.SystemDataStore))
                {
                    // If a player's editor clone has unexpectedly stopped, prompt for a restart.
                    if (player.Active
                     && player.Type == PlayerType.Clone
                     && player.TypeDependentPlayerInfo.VirtualProjectIdentifier != null 
                     && vpContext.VirtualProjectsApi.TryGet(player.TypeDependentPlayerInfo.VirtualProjectIdentifier, out var project, out _))
                    {
                        if (project.EditorState == EditorState.UnexpectedlyStopped)
                        {
                            project.Close(out _);

                            var choice = PlayerSyncModal.DisplayPlayerSyncModal($"{player.Name} unexpectedly stopped.");
                            if (choice == CloneSyncModal.Choices.Restart)
                            {
                                var hasProjectState = vpContext.StateRepository.TryGetValue(project.Identifier, out var projectState);
                                Debug.Assert(hasProjectState);
                                project.Launch(out _, out _, projectState.LaunchArgs);
                            }
                            else
                            {
                                player.Active = false;
                                mppmContext.SystemDataStore.SavePlayerJson(player.Index, player);
                            }
                        }
                        else if (project.EditorState == EditorState.NotLaunched)
                        {
                            player.Active = false;
                            mppmContext.SystemDataStore.SavePlayerJson(player.Index, player);
                        }
                    }
                }
            };
            mppmContext.MainPlayerSystems.ApplicationEvents.EditorChangedVersion += (from, to) =>
            {
                /*****
                 * After changing unity versions, clones that were created in the
                 * old version prompt "Are you sure" when first opening.
                 * We delete the library folder to prevent that.
                 * A forced refresh is not enough.
                 */
                MppmLog.Debug($"The editor version has changed from {from} to {to}");
                var directories = Directory.EnumerateDirectories(Paths.CurrentProjectVirtualProjectsFolder);
                foreach (var cloneDirectory in directories)
                {
                    var library = Path.Combine(cloneDirectory, "Library");
                    if (Directory.Exists(library))
                    {
                        DeleteDirectory(library);
                    }
                }
            };
            mppmContext.MainPlayerSystems.ApplicationEvents.PausedOnPlayer += () =>
            {
                if (!EditorApplication.isPaused)
                {
                    EditorApplication.isPaused = true;
                    vpContext.MessagingService.Broadcast(new PauseMessage());
                }
            };

            mppmContext.MainPlayerSystems.PlaymodeEvents.Play += () =>
            {
                if (IsAnyClonesOpen(mppmContext))
                {
                    if (IsAnyDirtySceneOpen(out var openSceneNames))
                    {
                        MppmLog.Warning( "Unsaved scene changes in the main editor will not be loaded on other players. " +
                            $"Currently open scenes with unsaved changes include: {openSceneNames}");
                    }
                }
                
                EditorPrefs.SetBool(k_OpenGameViewOnPlayCache, OpenWindowOnEnteringPlayMode);
                
                vpContext.MessagingService.Broadcast(new PlayMessage());
            };
            mppmContext.MainPlayerSystems.PlaymodeEvents.Pause += () =>
            {
                vpContext.MessagingService.Broadcast(new PauseMessage());
            };
            mppmContext.MainPlayerSystems.PlaymodeEvents.Step += () =>
            {
                vpContext.MessagingService.Broadcast(new StepMessage());
            };
            mppmContext.MainPlayerSystems.PlaymodeEvents.Unpause += () =>
            {
                vpContext.MessagingService.Broadcast(new UnpauseMessage());
            };
            mppmContext.MainPlayerSystems.PlaymodeEvents.Stop += () =>
            {
                vpContext.MessagingService.Broadcast(new StopMessage());
                OpenWindowOnEnteringPlayMode = EditorPrefs.GetBool(k_OpenGameViewOnPlayCache);
                EditorPrefs.DeleteKey(k_OpenGameViewOnPlayCache);
            };
        }

        static void DeleteDirectory(string path)
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch(Exception e)
            {
                // Catch all exceptions.
                // For the most part it is fine if the folder remains
                MppmLog.Debug($"An exception occured while deleting the directory. [{e}]");
            }
        }

        static bool IsAnyClonesOpen(WorkflowMainEditorContext context)
        {
            foreach (var player in UnityPlayer.GetPlayers(context.SystemDataStore))
            {
                if (player.Type == PlayerType.Clone && player.Active)
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsAnyDirtySceneOpen(out string openSceneNames)
        {
            openSceneNames = string.Empty;
            
            var openDirtyScenes = new List<Scene>();
            var sceneCount = SceneManager.sceneCount;
            for (var i = 0; i < sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene.isDirty)
                {
                    openDirtyScenes.Add(scene);
                }
            }
            if (openDirtyScenes.Count <= 0)
            {
                return false;
            }

            var sceneNames = new List<string>();
            foreach (var scene in openDirtyScenes)
            {
                sceneNames.Add(scene.name);
            }
            openSceneNames = string.Join(", ", sceneNames);
            return true;
        }
    }
}