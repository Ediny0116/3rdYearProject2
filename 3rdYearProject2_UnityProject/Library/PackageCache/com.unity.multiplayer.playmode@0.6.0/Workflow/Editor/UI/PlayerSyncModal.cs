using System;
using UnityEditor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    static class PlayerSyncModal
    {
        public static Unity.Multiplayer.Playmode.VirtualProjects.Editor.CloneSyncModal.Choices DisplayPlayerSyncModal(string whatHappened)
        {
            var option = EditorUtility.DisplayDialogComplex(
                whatHappened,
                "Do you want to restart the player?",
                "Restart",
                "Close player",
                "Continue anyways");

            return option switch
            {
                0 => VirtualProjects.Editor.CloneSyncModal.Choices.Restart,
                1 => VirtualProjects.Editor.CloneSyncModal.Choices.CloseEditor,
                2 => VirtualProjects.Editor.CloneSyncModal.Choices.ContinueAnyways,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}