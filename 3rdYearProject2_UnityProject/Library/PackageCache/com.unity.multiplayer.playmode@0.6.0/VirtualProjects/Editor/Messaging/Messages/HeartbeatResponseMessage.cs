using System.IO;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class HeartbeatResponseMessage
    {
        public HeartbeatResponseMessage(VirtualProjectIdentifier identifier, bool debuggerAttached)
        {
            Identifier = identifier;
            DebuggerAttached = debuggerAttached;
        }

        public VirtualProjectIdentifier Identifier { get; }
        public bool DebuggerAttached { get; }

            static void Serialize(BinaryWriter writer, object message)
            {
                var value = message as HeartbeatResponseMessage;
                writer.Write(value.Identifier.ToString());
                writer.Write(value.DebuggerAttached);
            }

            static object Deserialize(BinaryReader reader)
            {
                VirtualProjectIdentifier.TryParse(reader.ReadString(), out var identifier);
                var debuggerAttached = reader.ReadBoolean();
                return new HeartbeatResponseMessage(identifier, debuggerAttached);
            }

        [SerializeMessageDelegatesAttribute]
        public static SerializeMessageDelegates SerializeMethods() => new SerializeMessageDelegates { SerializeFunc = Serialize, DeserializeFunc = Deserialize };
    }
}