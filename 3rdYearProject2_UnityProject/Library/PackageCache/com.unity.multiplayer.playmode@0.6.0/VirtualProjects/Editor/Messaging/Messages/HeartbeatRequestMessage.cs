using System.IO;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class HeartbeatRequestMessage
    {
        static void Serialize(BinaryWriter writer, object value)
        {
        }

        static object Deserialize(BinaryReader reader)
        {
            return new HeartbeatRequestMessage();
        }
        
        [SerializeMessageDelegatesAttribute]
        public static SerializeMessageDelegates SerializeMethods() => new SerializeMessageDelegates { SerializeFunc = Serialize, DeserializeFunc = Deserialize };
    }
}