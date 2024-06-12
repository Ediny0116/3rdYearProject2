using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameFramework.Core.GameFramework.Manager
{
    public class LobbyManager
    {
        private Lobby _lobby;
        public async Task<bool> CreateLobby(int maxPlayers, bool isPrivate, Dictionary<string, string> data)
        {
            Dictionary<string, PlayerDataObject> playerData =SerializePlayerData(data);
            Player player = new Player(AuthenticationService.Instance.PlayerId, connectionInfo: null, playerData);
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                IsPrivate = isPrivate,
                Player = player
            };
            _lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", maxPlayers, options);
            Debug.Log(message: $"Lobby created with lobby id {_lobby.Id}");
            return true;
        }

        private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
        {
            Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();

            ////////
            foreach (KeyValuePair<string, string> kvp in data)
            {
                string key = kvp.Key;
                string value = kvp.Value;
                playerData.Add(key, new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member,
                    value: value));
            }
            ////////

            return playerData;

        }
        public void OnApplicationQuit()
        {
            if (_lobby !=null && _lobby.HostId== AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
            }

        }
    } 
}

    