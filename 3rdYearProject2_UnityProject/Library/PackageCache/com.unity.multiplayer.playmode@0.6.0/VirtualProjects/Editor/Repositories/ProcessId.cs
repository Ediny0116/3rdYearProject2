using JetBrains.Annotations;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class ProcessId
    {
        public ProcessId()
            : this(-1)
        {
        }

        public ProcessId(int value)
        {
            Value = value;
        }

        public int Value
        {
            get; 
            
            // Setter is used by serialization
            [UsedImplicitly] set;
        }
    }
}