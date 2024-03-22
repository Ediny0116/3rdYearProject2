using System.IO;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class PlayerInitializedMessage
    {
        public PlayerInitializedMessage(VirtualProjectIdentifier identifier)
        {
            Identifier = identifier;
        }

        public VirtualProjectIdentifier Identifier { get; }

        static void Serialize(BinaryWriter writer, object message)
        {
            var value = message as PlayerInitializedMessage;
            writer.Write(value.Identifier.ToString());
        }

        static object Deserialize(BinaryReader reader)
        {
            VirtualProjectIdentifier.TryParse(reader.ReadString(), out var identifier);
            return new PlayerInitializedMessage(identifier);
        }

        [SerializeMessageDelegatesAttribute]
        public static SerializeMessageDelegates SerializeMethods() => new SerializeMessageDelegates { SerializeFunc = Serialize, DeserializeFunc = Deserialize };
    }
}