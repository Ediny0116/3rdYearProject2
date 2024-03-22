using System;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class MainApplicationEvents
    {
        public event Action<VirtualProjectIdentifier, string[]> CloneLaunching;
        public event Action<VirtualProjectIdentifier, bool> CloneHeartbeatReceived;
        public event Action<VirtualProjectIdentifier> CloneUnresponsive;
        public event Action Quitting;

        public void InvokeCloneLaunching(VirtualProjectIdentifier identifier, string[] args)
        {
            CloneLaunching?.Invoke(identifier, args);
        }

        public void InvokeCloneHeartbeatReceived(VirtualProjectIdentifier identifier, bool debuggerAttached)
        {
            CloneHeartbeatReceived?.Invoke(identifier, debuggerAttached);
        }

        public void InvokeCloneUnresponsive(VirtualProjectIdentifier identifier)
        {
            CloneUnresponsive?.Invoke(identifier);
        }

        public void InvokeQuitting()
        {
            Quitting?.Invoke();
        }
    }
}