using System;
using Unity.Multiplayer.Playmode.InternalBridge.Editor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class ClonedPlayerApplicationEvents
    {
        public event Action PlayerActive;
        public event Action<EditorTitleUpdater> PlayerTitleRename;
        public event Action OpenPlayerWindow;
        public event Action ConsoleLogMessagesChanged;
        public event Action PlayerPaused;
        public event Action FrameAfterPlaymodeMessage;

        public void InvokeClonePlayerPaused()
        {
            PlayerPaused?.Invoke();
        }

        public void InvokeOpenPlayerWindow()
        {
            OpenPlayerWindow?.Invoke();
        }

        public void InvokeEditorStarted(ApplicationTitleDescriptorProxy applicationTitle)
        {
            var updater = new EditorTitleUpdater(applicationTitle);
            PlayerTitleRename?.Invoke(updater);
        }

        public void InvokeConsoleLogMessagesChanged()
        {
            ConsoleLogMessagesChanged?.Invoke();
        }

        public void InvokePlayerActive()
        {
            PlayerActive?.Invoke();
        }

        internal void InvokeFrameAfterPlaymodeMessage()
        {
            FrameAfterPlaymodeMessage?.Invoke();
        }
    }
}