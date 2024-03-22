using Unity.Multiplayer.Playmode.Common.Editor;
using UnityEditor.MPE;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class MainEditorContext
    {
        internal MainEditorContext()
        {
            if (!ChannelService.IsRunning())
            {
                ChannelService.Start();
            }
            
            MainEditorSystems = new MainEditorSystems();
            {
                MessagingService = MessagingService.GetMain(MessagingServiceConstants.DefaultChannelName);
                StateRepository =  SessionStateJsonRepository<VirtualProjectIdentifier, VirtualProjectState>.GetMain(SessionStateRepository.Get, nameof(StateRepository), out _);
                RequestCloneInfo = new RequestCloneInfo();
                {
                    var virtualProjectFileRepository = VirtualProjectFileRepository.GetMain();
                    var virtualProjectProcessRepository = VirtualProjectProcessRepository.GetMain(SessionStateJsonRepository<VirtualProjectIdentifier, ProcessId>.GetMain(SessionStateRepository.Get, nameof(VirtualProjectProcessRepository), out _));
                    VirtualProjectsApi = VirtualProjectsApi.GetMain(virtualProjectFileRepository, virtualProjectProcessRepository, StateRepository, MainEditorSystems);   
                }
                var internalRuntime = new MainEditorInternalRuntime();
                internalRuntime.HandleEvents(this);
            }
            MainEditorSystems.Listen(this);
        }

        internal VirtualProjectsApi VirtualProjectsApi { get; }
        internal RequestCloneInfo RequestCloneInfo { get; }
        public MessagingService MessagingService { get; }
        public SessionStateJsonRepository<VirtualProjectIdentifier, VirtualProjectState> StateRepository { get; }
        
        public MainEditorSystems MainEditorSystems { get; }
    }
}