using UnityEngine.UIElements;

namespace Unity.Multiplayer.Playmode.WorkflowUI.Editor
{
    class PlayersListView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<PlayersListView, UxmlTraits> { }
        
        // Player views get added to this view with VisualElement.Add()
    }
}