using System;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class CloneApplicationEvents
    {
        public event Action ParentQuitting;
        
        public void InvokeParentQuitting()
        {
            ParentQuitting?.Invoke();
        }
    }
}