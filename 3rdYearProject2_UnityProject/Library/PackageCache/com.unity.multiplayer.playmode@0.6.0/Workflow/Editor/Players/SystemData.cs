using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    [Serializable]
    class SystemData
    {
        [JsonProperty] 
        public bool IsMppmActive { get; internal set; }
        [JsonProperty] 
        public string EditorVersion { get; internal set; }
        
        [JsonProperty(Required = Required.Always)]
        public readonly Dictionary<int, PlayerStateJson> Data = new Dictionary<int, PlayerStateJson>();

        internal static string Serialize(SystemData systemData)
        {
            return JsonConvert.SerializeObject(systemData, Formatting.Indented);
        }
        
        internal static bool TryDeserialize(string data, out SystemData systemData)
        {
            try
            {
                systemData = JsonConvert.DeserializeObject<SystemData>(data);
            }
            catch (JsonException e) when (e is JsonSerializationException or JsonReaderException)
            {
                systemData = null;
            }

            return systemData != null;
        }
    }
}