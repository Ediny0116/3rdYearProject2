using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Playmode.WorkflowUI.Editor
{
    class PlayerView : VisualElement
    {
        public static class ClassNames
        {
            public const string mainEditorPlayer = "main-editor-player";

            public const string inactive = nameof(inactive);
            public const string activating = nameof(activating);
            public const string active = nameof(active);
            public const string running = nameof(running);
            public const string dotgray = nameof(dotgray);
            public const string dotgreen = nameof(dotgreen);

            public static readonly string[] All = { inactive, activating, active, running, dotgray, dotgreen };
            public static readonly string[] Inactive = { inactive, dotgray };
            public static readonly string[] Active = { active, running, dotgreen };
            public static readonly string[] Activating = { activating, dotgray };
        }


#if UNITY_EDITOR_WIN
        const string k_OpenInExplorerContextualMenuLabel = "Open in Explorer";
#elif UNITY_EDITOR_OSX
        const string k_OpenInExplorerContextualMenuLabel = "Show in Finder";
#else
        const string k_OpenInExplorerContextualMenuLabel = "Open Directory";
#endif
        internal const string TagLineBreak = "---";
        internal const string TagCreateTag = "+ Create Tag";
        internal const string TagDefault = "Add Tags for Player";
        
#if UNITY_USE_MULTIPLAYER_ROLES
        internal const string RoleServerClient = "Server & Client";
        internal const string RoleServer = "Server";
        internal const string RoleClient = "Client";
#endif

        internal Label ActiveText => this.Q<Label>(nameof(ActiveText));
        internal Toggle NameToggle => this.Q<Toggle>(nameof(NameToggle));
        internal VisualElement PlayerViewContent => this.Q<VisualElement>(nameof(PlayerViewContent));
        internal Label LogInfoText => this.Q<Label>(nameof(LogInfoText));
        internal Label LogWarningText => this.Q<Label>(nameof(LogWarningText));
        internal Label LogErrorText => this.Q<Label>(nameof(LogErrorText));
        internal VisualElement LogFilters => this.Q<VisualElement>(nameof(LogFilters));
        internal DropdownField MultiplayerRolesDropdown => this.Q<DropdownField>(nameof(MultiplayerRolesDropdown));
        internal DropdownField PlayerTagDropdown => this.Q<DropdownField>(nameof(PlayerTagDropdown));
        internal LoadingSpinner ActivatingIndicator => this.Q<LoadingSpinner>(nameof(ActivatingIndicator));
        internal VisualElement PlayerTagPills => this.Q<VisualElement>(nameof(PlayerTagPills));
        internal VisualElement EllipsesContainer => this.Q<VisualElement>(nameof(EllipsesContainer));

        static readonly string UXML = $"{UXMLPaths.UXMLWorkflowRoot}/Views/Players/PlayerView.uxml";
        static readonly string s_DarkUssPath = $"{UXMLPaths.UXMLWorkflowRoot}/Views/Players/PlayerViewDark.uss";
        static readonly string s_LightUssPath = $"{UXMLPaths.UXMLWorkflowRoot}/Views/Players/PlayerViewLight.uss";

        public event Action<bool> ActiveUpdatedEvent;
        public event Action<string> PillCloseEvent;

        [NotNull]
        internal readonly VisualElement EditorIcon = new();
        [NotNull]
        internal readonly VisualElement EllipseIcon = new();

        public PlayerView()
        {
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML).CloneTree(this);
            this.AddEventLifecycle(OnAttach, OnDetach);
            
            styleSheets.Add(EditorGUIUtility.isProSkin
                ? AssetDatabase.LoadAssetAtPath<StyleSheet>(s_DarkUssPath)
                : AssetDatabase.LoadAssetAtPath<StyleSheet>(s_LightUssPath));
        }

        void OnAttach(AttachToPanelEvent _)
        {
            NameToggle.RegisterValueChangedCallback(OnActiveToggled);
        }

        void OnDetach(DetachFromPanelEvent _)
        {
            NameToggle.UnregisterValueChangedCallback(OnActiveToggled);
        }

        void OnActiveToggled(ChangeEvent<bool> evt)
        {
            ActiveUpdatedEvent?.Invoke(evt.newValue);
        }

        public Pill AddPill(string newValue)
        {
            var p = new Pill
            {
                Text = newValue,
            };
            PlayerTagPills.Add(p);
            return p;
        }

        public void PopulateMultiplayerRoles(bool isMainPlayer)
        {
#if UNITY_USE_MULTIPLAYER_ROLES
            MultiplayerRolesDropdown.SetValueWithoutNotify(isMainPlayer
                ? RoleServer
                : RoleClient);
            MultiplayerRolesDropdown.choices = new List<string>
            {
                RoleServerClient,
                RoleServer,
                RoleClient,
            };
#else
            MultiplayerRolesDropdown?.RemoveFromHierarchy();
#endif
        }

        public void RepopulateTagsAndPills(IEnumerable<string> allSystemTags, IReadOnlyCollection<string> unityPlayerTags, bool canModifyTag)
        {
            var choices = new List<string>();
            foreach (var tag in allSystemTags)
            {
                var contains = false;
                foreach (var playerTag in unityPlayerTags)
                {
                    if (Equals(playerTag, tag))
                    {
                        contains = true;
                        break;
                    }
                }

                if (!contains)
                {
                    choices.Add(tag.Length >= 24
                        ? $"{tag[..22]}..."
                        : tag);
                }
            }
            choices.Add(TagLineBreak);
            choices.Add(TagCreateTag);
            // populate dropdown with all available tags
            PlayerTagDropdown.SetEnabled(canModifyTag);
            PlayerTagDropdown.SetValueWithoutNotify(TagDefault);
            PlayerTagDropdown.choices = choices;

            var pills = this.Query<Pill>().ToList();
            foreach (var pill in pills)
            {
                pill.RemoveFromHierarchy();
            }
            foreach (var tagEntry in unityPlayerTags)
            {
                var p = AddPill(tagEntry);
                p.CloseEvent += sender =>
                {
                    PillCloseEvent?.Invoke(tagEntry);
                };
            }
        }

        public void RefreshEditorIconBackgroundImage(bool isPlayerMain)
        {
            if (EditorIcon.style.backgroundImage.value != null) return;
            
            // Under some circumstances, like loading a new scene or during an assembly reload,
            // this visual element's background image can be reset to null.
            // Refresh it if this has occurred.
            EditorIcon.style.backgroundImage = isPlayerMain
                ? Images.GetImage(Images.ImageName.MainEditorIcon)
                : Images.GetImage(Images.ImageName.CloneEditorIcon);
            EllipseIcon.style.backgroundImage = Images.GetImage(Images.ImageName.Settings);
        }

        public enum PlayerState
        {
            NotLaunched,
            Launching,
            Launched,
            UnexpectedlyStopped,
        }

        public void UIActivateState(bool isMainEditor, PlayerState unityPlayerState, bool isPlayerActivateProhibited)
        {
            this.RemoveFromClassList(ClassNames.All);

            if (isMainEditor)
            {
                NameToggle.value = true;
                NameToggle.SetEnabled(false);
                this.AddToClassList(ClassNames.Active);
                ActiveText.text = "Active";
            }
            else
            {
                switch (unityPlayerState)
                {
                    case PlayerState.NotLaunched:
                    {
                        // Off
                        this.AddToClassList(ClassNames.Inactive);
                        ActivatingIndicator.Stop();
                        ActiveText.text = "Inactive";
                        NameToggle.SetEnabled(!isPlayerActivateProhibited);
                        NameToggle.value = false;
                        LogFilters.SetEnabled(false);
                        LogInfoText.text = 0.ToString();
                        LogWarningText.text = 0.ToString();
                        LogErrorText.text = 0.ToString();
                    }
                        break;
                    case PlayerState.UnexpectedlyStopped:
                    {
                        // Active but not communicative?
                        ActivatingIndicator.Stop();
                        NameToggle.SetEnabled(!isPlayerActivateProhibited);
                    }
                        break;
                    case PlayerState.Launched:
                    {
                        // Active
                        this.AddToClassList(ClassNames.Active);
                        ActivatingIndicator.Stop();
                        ActiveText.text = "Active";
                        LogFilters.SetEnabled(true);
                    }
                        break;
                    case PlayerState.Launching:
                    {
                        // Activating
                        this.AddToClassList(ClassNames.Activating);
                        ActivatingIndicator.Start();
                        ActiveText.text = "Activating";
                    }
                        break;
                }
            }
        }
        
        public void UILogCounts(bool isMainEditor, string info, string warning, string error)
        {
            if (isMainEditor) return;

            LogInfoText.text = string.IsNullOrWhiteSpace(info) ? 0.ToString() : info;
            LogWarningText.text = string.IsNullOrWhiteSpace(warning) ? 0.ToString() : warning;
            LogErrorText.text = string.IsNullOrWhiteSpace(error) ? 0.ToString() : error;
        }
    }
}