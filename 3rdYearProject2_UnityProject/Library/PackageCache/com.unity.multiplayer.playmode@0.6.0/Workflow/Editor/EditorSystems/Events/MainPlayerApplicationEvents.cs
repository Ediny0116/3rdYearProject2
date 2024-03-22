using System;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class MainPlayerApplicationEvents
    {
        public event Action<PlayerIdentifier> PlayerCommunicative;
        public event Action<string, PlayerIdentifier> SceneChanged;
        public event Action<PlayerIdentifier, LogCounts> LogCountsChanged;
        public event Action UIPollUpdate;
        public event Action<string, string> EditorChangedVersion;
        public event Action PausedOnPlayer;

        internal void InvokeEditorCommunicative(PlayerIdentifier identifier)
        {
            PlayerCommunicative?.Invoke(identifier);
        }

        internal void InvokeSceneChanged(string scenePath, PlayerIdentifier identifier)
        {
            SceneChanged?.Invoke(scenePath, identifier);
        }

        internal void InvokeLogCountsChanged(PlayerIdentifier playerIdentifier, LogCounts logCounts)
        {
            LogCountsChanged?.Invoke(playerIdentifier, logCounts);
        }

        internal void InvokeUIPollUpdate()
        {
            UIPollUpdate?.Invoke();
        }

        internal void InvokeEditorVersionChanged(string from, string to)
        {
            EditorChangedVersion?.Invoke(from, to);
        }

        internal void InvokePausedOnPlayer()
        {
            PausedOnPlayer?.Invoke();
        }
    }
}