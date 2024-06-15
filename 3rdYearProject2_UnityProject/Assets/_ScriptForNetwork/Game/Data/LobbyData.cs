using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;

namespace GameFramework.Core.Data
{
    public class LobbyData
    {
        public string _relayJoinCode;

        public void Initialize(Dictionary<string, DataObject> lobbyData)
        {
            UpdateState(lobbyData);
        }
        public void UpdateState(Dictionary<string, DataObject> lobbyData)
        {
            if (lobbyData.ContainsKey("RelayJoinCode"))
            {
                _relayJoinCode = lobbyData["RelayJoinCode"].Value;
            }
        }

        public Dictionary<string, string> Serialize()
        {
            return new Dictionary<string, string>()
            {
                {"RelayJoinCode",_relayJoinCode}
            };
        }

        public void SetRelayJoinCode(string code)
        {
            _relayJoinCode = code;
        }
    }
}
