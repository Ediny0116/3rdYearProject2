using System.Collections.Generic;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class InternalMultiplayerPlaymode
    {
        const string k_EditorModeName = "com.unity.mppm.clone";
        const string k_VPPrefix = "mppm";

        internal UnityPlayer PlayerOne { get; }
        internal UnityPlayer PlayerTwo { get; }
        internal UnityPlayer PlayerThree { get; }
        internal UnityPlayer PlayerFour { get; }

        internal UnityPlayer[] Players { get; }

        internal UnityPlayerTags PlayerTags { get; }

        internal InternalMultiplayerPlaymode(SystemDataStore systemDataStore, VirtualProjectsApi virtualProjectsApi, ProjectDataStore projectDataStore)
        {
            var has1 = systemDataStore.TryLoadPlayerJson(1, out var one);
            var has2 = systemDataStore.TryLoadPlayerJson(2, out var two);
            var has3 = systemDataStore.TryLoadPlayerJson(3, out var three);
            var has4 = systemDataStore.TryLoadPlayerJson(4, out var four);
            
            Debug.Assert(has1);
            Debug.Assert(has2);
            Debug.Assert(has3);
            Debug.Assert(has4);

            PlayerOne = new UnityPlayer(one, systemDataStore, virtualProjectsApi, k_VPPrefix, null);
            PlayerTwo = new UnityPlayer(two, systemDataStore, virtualProjectsApi, k_VPPrefix, DefaultVirtualClonePlayerLaunchArgs(two.Name));
            PlayerThree = new UnityPlayer(three, systemDataStore, virtualProjectsApi, k_VPPrefix, DefaultVirtualClonePlayerLaunchArgs(three.Name));
            PlayerFour = new UnityPlayer(four, systemDataStore, virtualProjectsApi, k_VPPrefix, DefaultVirtualClonePlayerLaunchArgs(four.Name));
            Players = new[]
            {
                PlayerOne, PlayerTwo, PlayerThree, PlayerFour,
            };
            PlayerTags = new UnityPlayerTags(projectDataStore, systemDataStore);
        }
        
        static string[] DefaultVirtualClonePlayerLaunchArgs(string projectName)
        {
            var args = new List<string>();
#if !UNITY_MPPM_FORCE_MAIN_WINDOW
            args.Add(CommandLineParameters.BuildEditorModeArgument(k_EditorModeName));
#endif
            if (!MultiplayerPlayModeSettings.ShowLaunchScreenOnPlayers)
            {
                args.Add(CommandLineParameters.k_NoLaunchScreen);
            }

            args.Add(CommandLineParameters.k_VirtualLibraryFolder);
            args.Add(CommandLineParameters.k_AssetDatabaseReadOnly);
            args.Add(CommandLineParameters.BuildEditorDebuggingName(projectName));
            return args.ToArray();
        }
    }
}