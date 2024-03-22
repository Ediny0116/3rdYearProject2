using UnityEditor.MPE;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class CloneContext
    {
        internal CloneContext()
        {
            if (!ChannelService.IsRunning())
            {
                ChannelService.Start();
            }
            
            CloneSystems = new CloneSystems();
            {
                MessagingService = MessagingService.GetClone(CommandLineParameters.ReadCurrentChannelName());
                var internalRuntime = new CloneInternalRuntime();
                internalRuntime.HandleEvents(this);
            }
            CloneSystems.Listen(vpContext:this);
        }

        public MessagingService MessagingService { get; }
        public CloneSystems CloneSystems { get; }
    }
}
