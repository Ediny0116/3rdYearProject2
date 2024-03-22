using System;
using System.Collections.Generic;
using Unity.Multiplayer.Playmode.Common.Runtime;
using Unity.Multiplayer.Playmode.Workflow.Editor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Playmode.WorkflowUI.Editor
{
    class MultiplayerWindow : EditorWindow
    {
        public MainView MainView { get; private set; }

        void OnFocus()
        {
            MultiplayerWindowController.ShouldUpdateUI = true;
        }

        public void CreateGUI()
        {
            if (!MultiplayerWindowController.IsVirtualProjectWorkflowInitialized)
            {
                DestroyImmediate(this);
                return;
            }

            MainView = new MainView();
            rootVisualElement.Add(MainView);
            MultiplayerWindowController.ShouldStartWindow = true;
        }
    }

    static class MultiplayerWindowController
    {
        const string k_Title = "Multiplayer Play Mode";
        
        const string k_GiveFocusToPlayerContextualMenuLabel = "Focus on Player";
#if UNITY_EDITOR_WIN
        const string k_OpenInExplorerContextualMenuLabel = "Open in Explorer";
#elif UNITY_EDITOR_OSX
        const string k_OpenInExplorerContextualMenuLabel = "Show in Finder";
#else
        const string k_OpenInExplorerContextualMenuLabel = "Open Directory";
#endif

        static MultiplayerWindow s_MultiplayerWindow;
        static readonly Dictionary<PlayerView, UnityPlayer> APIModelToViewMapping = new Dictionary<PlayerView, UnityPlayer>();

        public static bool ShouldUpdateUI;
        public static bool ShouldStartWindow;
        public static bool IsVirtualProjectWorkflowInitialized;

        [MenuItem("Window/Multiplayer Play Mode")]
        static void ShowConfiguration()
        {
            if (IsVirtualProjectWorkflowInitialized)
            {
                ShouldStartWindow = true;
            }
            else
            {
                Debug.LogWarning("MPPM is not enabled. [Preferences->Multiplayer Play Mode]");
            }
        }

        static MultiplayerWindowController()
        {
            VirtualProjectWorkflow.OnInitialized += isMainEditor =>
            {
                if (!isMainEditor)
                {
                    MppmLog.Debug("We are a clone. No need to open the projects window.");
                    return;
                }
                IsVirtualProjectWorkflowInitialized = true;
            };
            VirtualProjectWorkflow.OnDisabled += isMainEditor =>
            {
                if (!isMainEditor)
                {
                    MppmLog.Debug("We are a clone. No need to open the projects window.");
                    return;
                }

                IsVirtualProjectWorkflowInitialized = false;
            };

            // Events that update the UI
            CompilationPipeline.assemblyCompilationFinished += (_, _) => { ShouldUpdateUI = true; };
            EditorApplication.playModeStateChanged += _ =>
            {
                ShouldUpdateUI = true;
            };
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;

            // Main window update loop
            EditorApplication.update += () =>
            {
                if (IsVirtualProjectWorkflowInitialized && ShouldStartWindow)
                {
                    ShouldStartWindow = false;
                    Start();
                }

                if (IsVirtualProjectWorkflowInitialized && ShouldUpdateUI && s_MultiplayerWindow != null)
                {
                    ShouldUpdateUI = false;
                    Update();
                }
            };
        }
        
        static void Start()
        {
            if (s_MultiplayerWindow == null)
            {
                s_MultiplayerWindow = EditorWindow.GetWindow<MultiplayerWindow>();
                s_MultiplayerWindow.titleContent = new GUIContent(k_Title);

                APIModelToViewMapping.Clear();

                foreach (var player in MultiplayerPlaymode.Players)
                {
                    var view = new PlayerView();
                    view.ActiveUpdatedEvent += newValue =>
                    {
                        if (newValue)
                        {
                            if (!player.Activate(out var error))
                            {
                                if (error == ActivationError.CompileErrors)
                                {                    
                                    MppmLog.Warning("Cannot activate a player while there are compile errors");
                                }
                            }
                        }
                        else
                        {
                            player.Deactivate(out _);
                        }

                        ShouldUpdateUI = true;
                    };
                    view.PlayerTagDropdown.RegisterValueChangedCallback(evt =>
                    {
                        Debug.Assert(player.Tags != null, "Tags should never be null");
                        
                        var newValue = evt.newValue;
                        if (newValue == PlayerView.TagDefault) return;
                        if (newValue == PlayerView.TagLineBreak) return;
                        
                        if (newValue != PlayerView.TagCreateTag)
                        {
                            if (!player.AddTag(newValue, out var tagError))
                            {
                                switch (tagError)
                                {
                                    case TagError.InPlayMode:
                                        MppmLog.Warning("Cannot modify tag while player is active in play mode");
                                        break;
                                    case TagError.Duplicate:
                                        MppmLog.Warning($"Attempting to add tag \"{newValue}\" to a player that already have \"{newValue}\" assigned to the player.");
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }

                            view.AddPill(newValue);
                        }
                        else
                        {
                            var result = ModeSwitcher.TryOpenProjectSettingsWindow("Project/Multiplayer/Playmode");
                            Debug.Assert(result, "Could not open project settings window.");
                        }

                        view.PlayerTagDropdown.SetValueWithoutNotify(PlayerView.TagDefault);

                        ShouldUpdateUI = true;
                    });
#if UNITY_USE_MULTIPLAYER_ROLES
                    view.MultiplayerRolesDropdown.RegisterValueChangedCallback(evt =>
                    {
                        var player = APIModelToViewMapping[view];
                        player.Role = evt.newValue switch
                        {
                            PlayerView.RoleServerClient => MultiplayerRoleFlags.ClientAndServer,
                            PlayerView.RoleServer => MultiplayerRoleFlags.Server,
                            PlayerView.RoleClient => MultiplayerRoleFlags.Client,
                            _ => throw new ArgumentOutOfRangeException(),
                        };
                    });
#endif
                    view.PillCloseEvent += tagEntry =>
                    {
                        if (!player.RemoveTag(tagEntry, out var tagError))
                        {
                            switch (tagError)
                            {
                                case TagError.DoesNotExist:
                                    MppmLog.Warning( $"Attempting to remove tag \"{tagEntry}\" from a player without \"{tagEntry}\" assigned to the player.");
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

                        view.PopulateMultiplayerRoles(player.Type == PlayerType.Main);
                        var canModifyTag1 = player.PlayerState is not (PlayerState.Launched or PlayerState.Launching) || !EditorApplication.isPlaying;
                        view.RepopulateTagsAndPills(MultiplayerPlaymode.PlayerTags.Tags, player.Tags, canModifyTag1);
                        ShouldUpdateUI = true;
                    };

                    player.OnPlayerCommunicative += () =>
                    {
                        _ = MultiplayerPlaymodeEditorUtility.FocusPlayerView(player);
                        ShouldUpdateUI = true;
                    };
                    player.OnUpdated += () =>
                    {
                        ShouldUpdateUI = true;
                    };
                    MultiplayerPlaymode.PlayerTags.OnUpdated += () =>
                    {
                        ShouldUpdateUI = true;
                    };

                    var isPlayerActive = player.PlayerState is PlayerState.Launched or PlayerState.Launching;
                    var isPlayerMainEditor = player.Type == PlayerType.Main;

                    if (isPlayerMainEditor)
                    {
                        view.AddToClassList(PlayerView.ClassNames.mainEditorPlayer);
                    }
                    else
                    {
                        view.PlayerViewContent.AddManipulator(new ContextualMenuManipulator(populateEvent =>
                        {
                            PlayerContextMenuOptions(populateEvent, player);
                        }));
                        var t = new ContextualMenuManipulator(populateEvent =>
                        {
                            PlayerContextMenuOptions(populateEvent, player);
                        });
                        t.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
                        view.EllipsesContainer.AddManipulator(t);
                        view.EllipseIcon.AddToClassList("ellipse-icon");
                        view.EllipsesContainer.Add(view.EllipseIcon);
                    }

                    // Format Name Foldout
                    view.NameToggle.text = player.Name;
                    view.NameToggle.value = isPlayerActive;

                    var checkMark = view.NameToggle.Q<VisualElement>("unity-checkmark");
                    var spacing = new VisualElement
                    {
                        style =
                        {
                            minWidth = 24,
                        },
                    };
                    view.RefreshEditorIconBackgroundImage(isPlayerMainEditor);
                    view.EditorIcon.AddToClassList("editorIcon");
                    var foldoutComponents = new VisualElement
                    {
                        name = "foldout-components",
                        style =
                        {
                            flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                        },
                    };
                    foldoutComponents.Add(spacing);
                    foldoutComponents.Add(view.EditorIcon);
                    checkMark.parent.Insert(1, foldoutComponents);
                    view.NameToggle.SetEnabled(!MultiplayerPlaymodeEditorUtility.IsPlayerActivateProhibited || isPlayerActive);
                    var canModifyTag = player.PlayerState is not (PlayerState.Launched or PlayerState.Launching) ||
                                       !EditorApplication.isPlaying;
                    var logs = MultiplayerPlaymodeLogUtility.PlayerLogs(player.PlayerIdentifier).LogCounts;
                    view.RefreshEditorIconBackgroundImage(isPlayerMainEditor);
                    view.PopulateMultiplayerRoles(isPlayerMainEditor);
                    view.RepopulateTagsAndPills(MultiplayerPlaymode.PlayerTags.Tags, player.Tags, canModifyTag);
                    view.UILogCounts(isPlayerMainEditor, logs.Logs.ToString(), logs.Warnings.ToString(), logs.Errors.ToString());
                    view.UIActivateState(isPlayerMainEditor, (PlayerView.PlayerState)player.PlayerState, MultiplayerPlaymodeEditorUtility.IsPlayerActivateProhibited);
                    if (isPlayerMainEditor)
                    {
                        s_MultiplayerWindow.MainView.MainListView.Add(view);
                    }else if (player.Type == PlayerType.Clone)
                    {
                        s_MultiplayerWindow.MainView.VirtualListView.Add(view);
                    }
                    APIModelToViewMapping.Add(view, player); // bind our view to the api model
                }
            }
        }

        static void PlayerContextMenuOptions(ContextualMenuPopulateEvent populateEvent, UnityPlayer player)
        {
            populateEvent.menu.AppendAction(
                k_OpenInExplorerContextualMenuLabel,
                _ => MultiplayerPlaymodeEditorUtility.RevealInFinder(player),
                DropdownMenuAction.AlwaysEnabled);
            populateEvent.menu.AppendAction(
                k_GiveFocusToPlayerContextualMenuLabel,
                _ => MultiplayerPlaymodeEditorUtility.FocusPlayerView(player),
                DropdownMenuAction.AlwaysEnabled);
        }

        static void Update()
        {
            foreach (var (view, player) in APIModelToViewMapping)
            {
                // double check log counts. this was broken and this probably fixes it
                var canModifyTag = player.PlayerState is not (PlayerState.Launched or PlayerState.Launching) ||
                                   !EditorApplication.isPlaying;
                
                var logs = MultiplayerPlaymodeLogUtility.PlayerLogs(player.PlayerIdentifier).LogCounts;
                var isPlayerMain = player.Type == PlayerType.Main;
                view.RefreshEditorIconBackgroundImage(isPlayerMain);
                view.PopulateMultiplayerRoles(isPlayerMain);
                view.RepopulateTagsAndPills(MultiplayerPlaymode.PlayerTags.Tags, player.Tags, canModifyTag);
                view.UILogCounts(isPlayerMain, logs.Logs.ToString(), logs.Warnings.ToString(), logs.Errors.ToString());
                view.UIActivateState(isPlayerMain, (PlayerView.PlayerState)player.PlayerState, MultiplayerPlaymodeEditorUtility.IsPlayerActivateProhibited);
            }

            if (MultiplayerPlaymodeEditorUtility.IsPlayerActivateProhibited)
            {
                s_MultiplayerWindow.MainView.AddToClassList(MainView.k_HasCompileErrorsClassName);
            }
            else
            {
                s_MultiplayerWindow.MainView.RemoveFromClassList(MainView.k_HasCompileErrorsClassName);
            }
            
            if (!MultiplayerPlayModeSettings.GetIsMppmActive())
            {
                s_MultiplayerWindow.MainView.AddToClassList(MainView.k_HasMPPMDisabled);
            }
            else
            {
                s_MultiplayerWindow.MainView.RemoveFromClassList(MainView.k_HasMPPMDisabled);
            }
        }

        static void OnSceneChanged(Scene _, Scene __)
        {
            foreach (var (view, player) in APIModelToViewMapping)
            {
                view.RefreshEditorIconBackgroundImage(player.Type == PlayerType.Main);
            }
		}
        [Shortcut("FocusPlayerOne", KeyCode.F9, ShortcutModifiers.Control)]
        public static void FocusPlayerOne()
        {
            _ = MultiplayerPlaymodeEditorUtility.FocusPlayerView(MultiplayerPlaymode.PlayerOne);
        }
        [Shortcut("FocusPlayerTwo", KeyCode.F10, ShortcutModifiers.Control)]
        public static void FocusPlayerTwo()
        {
            _ = MultiplayerPlaymodeEditorUtility.FocusPlayerView(MultiplayerPlaymode.PlayerTwo);
        }
        [Shortcut("FocusPlayerThree", KeyCode.F11, ShortcutModifiers.Control)]
        public static void FocusPlayerThree()
        {
            _ = MultiplayerPlaymodeEditorUtility.FocusPlayerView(MultiplayerPlaymode.PlayerThree);
        }
        [Shortcut("FocusPlayerFour", KeyCode.F12, ShortcutModifiers.Control)]
        public static void FocusPlayerFour()
        {
            _ = MultiplayerPlaymodeEditorUtility.FocusPlayerView(MultiplayerPlaymode.PlayerFour);
        }
    }
}