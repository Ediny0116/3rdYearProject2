using Unity.Multiplayer.Playmode.VirtualProjects.Editor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    static class MultiplayerPlaymode
    {
        static InternalMultiplayerPlaymode s_InternalMultiplayerPlaymode;

        static MultiplayerPlaymode()
        {
            VirtualProjectWorkflow.OnInitialized += isMainEditor =>
            {
                IsVirtualProjectWorkflowInitialized = true;

                if (!isMainEditor)
                {
                    return;
                }

                InitializeInternal();
            };
            VirtualProjectWorkflow.OnDisabled += isMainEditor =>
            {
                if (!IsVirtualProjectWorkflowInitialized)
                {
                    return;
                }

                IsVirtualProjectWorkflowInitialized = false;

                if (!isMainEditor)
                {
                    return;
                }

                DisableInternal();
            };
        }

        static void InitializeInternal()
        {
            var vpContext = EditorContexts.MainEditorContext;
            var mppmContext = VirtualProjectWorkflow.WorkflowMainEditorContext;
            s_InternalMultiplayerPlaymode = new InternalMultiplayerPlaymode(mppmContext.SystemDataStore, vpContext.VirtualProjectsApi, mppmContext.ProjectDataStore);

            mppmContext.MainPlayerSystems.ApplicationEvents.PlayerCommunicative +=
                OnApplicationEventsOnPlayerCommunicative;
            vpContext.MainEditorSystems.ApplicationEvents.CloneLaunching +=
                OnApplicationEventsOnCloneLaunching;
        }

        static void DisableInternal()
        {
            var vpContext = EditorContexts.MainEditorContext;
            var mppmContext = VirtualProjectWorkflow.WorkflowMainEditorContext;
            foreach (var player in Players)
            {
                player.Deactivate(out _);
            }

            mppmContext.MainPlayerSystems.ApplicationEvents.PlayerCommunicative -=
                OnApplicationEventsOnPlayerCommunicative;
            vpContext.MainEditorSystems.ApplicationEvents.CloneLaunching -=
                OnApplicationEventsOnCloneLaunching;
            s_InternalMultiplayerPlaymode = null;
        }

        static void OnApplicationEventsOnPlayerCommunicative(PlayerIdentifier identifier)
        {
            foreach (var player in Players)
            {
                if (player.PlayerIdentifier == identifier)
                {
                    player.InvokeOnPlayerCommunicative();
                }
            }
        }

        static void OnApplicationEventsOnCloneLaunching(VirtualProjectIdentifier identifier, string[] args)
        {
            foreach (var player in Players)
            { 
                if (player.TypeDependentPlayerInfo.VirtualProjectIdentifier == identifier)
                {
                    player.InvokeOnUpdated();
                }
            }
        }

        public static bool IsVirtualProjectWorkflowInitialized { get; set; }

        public static UnityPlayer PlayerOne => s_InternalMultiplayerPlaymode.PlayerOne;
        public static UnityPlayer PlayerTwo => s_InternalMultiplayerPlaymode.PlayerTwo;
        public static UnityPlayer PlayerThree => s_InternalMultiplayerPlaymode.PlayerThree;
        public static UnityPlayer PlayerFour => s_InternalMultiplayerPlaymode.PlayerFour;

        public static UnityPlayer[] Players => s_InternalMultiplayerPlaymode.Players;
        public static UnityPlayerTags PlayerTags => s_InternalMultiplayerPlaymode.PlayerTags;
    }
}