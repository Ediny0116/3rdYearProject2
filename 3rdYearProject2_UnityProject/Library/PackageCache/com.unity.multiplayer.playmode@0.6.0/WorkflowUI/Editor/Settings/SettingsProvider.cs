using System.Collections.Generic;
using Unity.Multiplayer.Playmode.Workflow.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Playmode.WorkflowUI.Editor
{
    class MultiplayerPlaymodeSettings : SettingsProvider
    {
        const string k_SectionName = "Preferences/Multiplayer Play Mode";

        [SettingsProvider]
        public static SettingsProvider CreateSettingProvider()
        {
            return new MultiplayerPlaymodeSettings(k_SectionName, SettingsScope.User);
        }

        MultiplayerPlaymodeSettings(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settingsView = new SettingsView();
            rootElement.Add(settingsView);

            settingsView.IsMppmActiveToggle.value = MultiplayerPlayModeSettings.GetIsMppmActive();
            settingsView.IsMppmActiveToggle.RegisterValueChangedCallback(IsMppmActiveToggleCallback);
            settingsView.ShowLaunchScreenToggle.value = MultiplayerPlayModeSettings.ShowLaunchScreenOnPlayers;
            settingsView.ShowLaunchScreenToggle.RegisterValueChangedCallback(evt => MultiplayerPlayModeSettings.ShowLaunchScreenOnPlayers = evt.newValue);
        }

        static void IsMppmActiveToggleCallback(ChangeEvent<bool> evt)
        {
            MultiplayerPlayModeSettings.SetIsMppmActive(evt.newValue);
        }
    }
}