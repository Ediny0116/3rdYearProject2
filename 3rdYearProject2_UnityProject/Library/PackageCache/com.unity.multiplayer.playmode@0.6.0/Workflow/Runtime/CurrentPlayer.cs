using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;
using Unity.Multiplayer.Playmode.Workflow.Editor;
#endif

namespace Unity.Multiplayer.Playmode
{
    /// <summary>
    /// Utility class to access information about the multiplayer play mode player while in playmode.
    /// </summary>
    public static class CurrentPlayer
    {
        // We only want to load the tag from the store once during play mode
        static bool s_Loaded;
        static IReadOnlyCollection<string> s_Tags;
#if UNITY_EDITOR
        static SystemDataStore s_SystemDataStore;
#endif

        /// <summary>
        /// Returns the tag(s) assigned to the currently running player.
        /// </summary>
        public static IReadOnlyCollection<string> ReadOnlyTags()
        {
            if (!s_Loaded)
            {
                s_Loaded = true;
                LoadTag();
            }

            return s_Tags ?? new List<string>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReloadLatestTagsOnEnterPlaymode()
        {
            s_Loaded = false;
        }

        static void LoadTag()
        {
#if UNITY_EDITOR
            s_SystemDataStore = VirtualProjectsEditor.IsClone
                ? SystemDataStore.GetClone()
                : SystemDataStore.GetMain();

            bool hasPlayer;
            PlayerStateJson player;
            if (VirtualProjectsEditor.IsClone)
            {
                hasPlayer = Filters.FindFirstPlayerWithVirtualProjectsIdentifier(s_SystemDataStore.LoadAllPlayerJson(),
                    VirtualProjectsEditor.CloneIdentifier, out player);
                Debug.Assert(hasPlayer, $"Could not find player using virtual project {VirtualProjectsEditor.CloneIdentifier}");
            }
            else
            {
                hasPlayer = Filters.FindFirstPlayerWithPlayerType(s_SystemDataStore.LoadAllPlayerJson(), PlayerType.Main, out player);
                Debug.Assert(hasPlayer, "Could not find player for the main editor");
            }

            s_Tags = player.Tags;
#endif
        }
    }
}