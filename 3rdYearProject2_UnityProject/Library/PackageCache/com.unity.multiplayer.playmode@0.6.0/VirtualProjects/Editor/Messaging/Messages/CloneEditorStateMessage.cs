using System.IO;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class CloneEditorStateMessage
    {
        public CloneEditorStateMessage(VirtualProjectIdentifier identifier, bool isPlaying)
        {
            Identifier = identifier;
            IsPlaying = isPlaying;
        }

        public VirtualProjectIdentifier Identifier { get; }
        public bool IsPlaying { get; }  // This represents EditorApplication.isPlaying

        static void Serialize(BinaryWriter writer, object message)
        {
            var value = message as CloneEditorStateMessage;
            writer.Write(value.Identifier.ToString());
            writer.Write(value.IsPlaying);
        }

        static object Deserialize(BinaryReader reader)
        {
            VirtualProjectIdentifier.TryParse(reader.ReadString(), out var identifier);
            var isPlaying = reader.ReadBoolean();
            return new CloneEditorStateMessage(identifier, isPlaying);
        }
        
        [SerializeMessageDelegatesAttribute]
        public static SerializeMessageDelegates SerializeMethods() => new SerializeMessageDelegates { SerializeFunc = Serialize, DeserializeFunc = Deserialize };
    }
}