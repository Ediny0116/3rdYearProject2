using Unity.Multiplayer.Playmode.Common.Editor;
using Unity.Multiplayer.Playmode.Common.Runtime;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;
using UnityEditor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    static class MultiplayerPlaymodeEditorUtility
    {
        public static bool IsPlayerActivateProhibited => EditorUtility.scriptCompilationFailed;

        public static void RevealInFinder(UnityPlayer player)
        {
            var vpContext = EditorContexts.MainEditorContext;
            var directory = vpContext.VirtualProjectsApi.TryGet(player.TypeDependentPlayerInfo.VirtualProjectIdentifier, out var project, out _)
                ? project.Directory
                : Paths.CurrentProjectVirtualProjectsFolder;
            EditorUtility.RevealInFinder(directory);
        }

        public enum ShowConsoleError
        {
            None,
            IsNotReady,
            PlayerTypeHasNoConsole,
        }

        public static ShowConsoleError FocusPlayerView(UnityPlayer player)
        {
            if (player.PlayerState is not (PlayerState.Launched or PlayerState.Launching)) return ShowConsoleError.IsNotReady;
            if (player.Type == PlayerType.Main) return ShowConsoleError.PlayerTypeHasNoConsole;
            if (player.TypeDependentPlayerInfo.VirtualProjectIdentifier == null) return ShowConsoleError.PlayerTypeHasNoConsole;
            
            var vpContext = EditorContexts.MainEditorContext;
            vpContext.MessagingService.Send(new OpenPlayerWindowMessage(), player.TypeDependentPlayerInfo.VirtualProjectIdentifier, null, MppmLog.Warning);
            return ShowConsoleError.None;
        }
    }
}