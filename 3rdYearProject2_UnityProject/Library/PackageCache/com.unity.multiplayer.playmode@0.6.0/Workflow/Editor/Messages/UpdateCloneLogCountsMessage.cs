using System.IO;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class UpdateCloneLogCountsMessage
    {
        public UpdateCloneLogCountsMessage(VirtualProjectIdentifier identifier, LogCounts logCounts)
        {
            Identifier = identifier;
            LogCounts = logCounts;
        }

        public VirtualProjectIdentifier Identifier { get; }
        public LogCounts LogCounts { get; }

        static void Serialize(BinaryWriter writer, object message)
        {
            var value = message as UpdateCloneLogCountsMessage;
            writer.Write(value.Identifier.ToString());
            LogCounts.Serialize(writer, value.LogCounts);
        }

        static object Deserialize(BinaryReader reader)
        {
            VirtualProjectIdentifier.TryParse(reader.ReadString(), out var identifier);
            var logs = LogCounts.Deserialize(reader);
            return new UpdateCloneLogCountsMessage(identifier, logs);
        }

        [SerializeMessageDelegatesAttribute]
        public static SerializeMessageDelegates SerializeMethods() => new SerializeMessageDelegates { SerializeFunc = Serialize, DeserializeFunc = Deserialize };
    }
}