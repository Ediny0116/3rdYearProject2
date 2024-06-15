using GameFramework.Core;
using GameFramework.Core.GameFramework.Manager;
using GameFramework.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Game.GameFramework.Core.Data; //?

namespace Game
{
    public class GameLobbyManager : Singleton<GameLobbyManager>
    {
        private List<LobbyPlayerData> _lobbyPlayerDatas = new List<LobbyPlayerData>();

        private LobbyPlayerData _localLobbyPlayerData;

        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }


        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        }

        public string GetLobbyCode()
        {
            return LobbyManager.Instance.GetLobbyCode();
        }

        public async Task<bool> CreateLobby()
        {
            LobbyPlayerData playerData = new LobbyPlayerData();
            playerData.Initialize(AuthenticationService.Instance.PlayerId, "HostPlayer");
            bool succeeded = await LobbyManager.Instance.CreateLobby(maxPlayers: 2, isPrivate: false, playerData.Serialize());
            return succeeded;
        }

        public async Task<bool> JoinLobby(string code)
        {
            LobbyPlayerData playerData = new LobbyPlayerData();
            playerData.Initialize(AuthenticationService.Instance.PlayerId, "JoinPlayer");
            bool succeeded = await LobbyManager.Instance.JoinLobby(code, playerData.Serialize());
            return succeeded;
        }

        private void OnLobbyUpdated(Lobby lobby)
        {
            List<Dictionary<string, PlayerDataObject>> playerData = LobbyManager.Instance.GetPlayersData();
            _lobbyPlayerDatas.Clear();

            foreach (Dictionary<string, PlayerDataObject> data in playerData)
            {
                LobbyPlayerData lobbyPlayerData = new LobbyPlayerData();
                lobbyPlayerData.Initialize(data);

                if (lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
                {
                    _localLobbyPlayerData = lobbyPlayerData;
                }

                _lobbyPlayerDatas.Add(lobbyPlayerData);
            }
            Events.LobbyEvents.OnLobbyUpdated?.Invoke();
        }

        internal List<LobbyPlayerData> GetPlayers()
        {
           return _lobbyPlayerDatas;
        }
    }
}