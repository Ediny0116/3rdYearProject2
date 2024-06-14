using System;
using System.Collections;
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
    public class LobbyManager:Singleton<LobbyManager>
    {
        private Lobby _lobby;
        private Coroutine _heartbeatCorotine;
        private Coroutine _refreshLobbyCoroutine;

        public string GetLobbyCode()
        {
            return _lobby?.LobbyCode;
        }

        public async Task<bool> CreateLobby(int maxPlayers, bool isPrivate, Dictionary<string, string> data)
        {
            Dictionary<string, PlayerDataObject> playerData =SerializePlayerData(data);
            Player player = new Player(AuthenticationService.Instance.PlayerId, connectionInfo: null, playerData);
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                IsPrivate = isPrivate,
                Player = player
            };

            try
            { 
            _lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", maxPlayers, options);
            }
            catch (System.Exception)
            {
                return false;
            }
            Debug.Log(message: $"Lobby created with lobby id {_lobby.Id}");

            _heartbeatCorotine=StartCoroutine(HerathbeatLobbyCoroutine(_lobby.Id, 6f));
            _refreshLobbyCoroutine=StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));

            return true;
        }

        private IEnumerator HerathbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            while (true)
            {
                //Debug.Log("Heartbeat");
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return new WaitForSeconds(waitTimeSeconds);
            }
        }

        private IEnumerator RefreshLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            while (true)
            {
               Task<Lobby> task= LobbyService.Instance.GetLobbyAsync(lobbyId);
                yield return new WaitUntil(()=>task.IsCompleted);
                Lobby newLobby= task.Result;
                if(newLobby.LastUpdated>_lobby.LastUpdated)
                {
                    _lobby = newLobby;
;                }
                yield return new WaitForSeconds(waitTimeSeconds);
            }
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

        public async Task<bool> JoinLobby(string code, Dictionary<string, string> playerData)
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions();
            Player player = new Player(AuthenticationService.Instance.PlayerId, connectionInfo: null, SerializePlayerData(playerData));

            options.Player = player;

            try
            {
                _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
            }
            catch (System.Exception)
            {
                return false;
            }
           _refreshLobbyCoroutine= StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
            return true;
        }
    } 
}

    