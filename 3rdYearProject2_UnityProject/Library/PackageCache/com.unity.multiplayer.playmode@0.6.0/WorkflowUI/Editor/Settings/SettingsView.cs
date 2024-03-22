using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Playmode.WorkflowUI.Editor
{
    class SettingsView : VisualElement
    {
        static readonly string UXML = $"{UXMLPaths.UXMLWorkflowRoot}/Settings/SettingsView.uxml";

        public Toggle IsMppmActiveToggle => this.Q<Toggle>(nameof(IsMppmActiveToggle));
        public Toggle ShowLaunchScreenToggle => this.Q<Toggle>(nameof(ShowLaunchScreenToggle));
        
        public SettingsView()
        {
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML).CloneTree(this);
        }
    }
}
