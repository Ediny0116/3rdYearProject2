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
using Game.GameFramework.Core.Data;
using GameFramework.Manager;
using GameFramework.Core.Data;
using UnityEngine.SceneManagement; //?

namespace Game
{
    public class GameLobbyManager : Singleton<GameLobbyManager>
    {
        private List<LobbyPlayerData> _lobbyPlayerData = new List<LobbyPlayerData>();

        private LobbyPlayerData _localLobbyPlayerData;

        private int _maxNumberOfPlayers = 2;
        private LobbyData _lobbyData;

        public bool IsHost => _localLobbyPlayerData.Id==LobbyManager.Instance.GetHostId();

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
            _localLobbyPlayerData = new LobbyPlayerData();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, "HostPlayer");
            _lobbyData = new LobbyData();
            _lobbyData.Initialize(new Dictionary<string, DataObject>());
            bool succeeded = await LobbyManager.Instance.CreateLobby(_maxNumberOfPlayers, isPrivate: false, _localLobbyPlayerData.Serialize(),_lobbyData.Serialize());
            return succeeded;
        }

        public async Task<bool> JoinLobby(string code)
        {
            _localLobbyPlayerData = new LobbyPlayerData();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, "JoinPlayer");
            bool succeeded = await LobbyManager.Instance.JoinLobby(code, _localLobbyPlayerData.Serialize());
            return succeeded;
        }

        private void OnLobbyUpdated(Lobby lobby)
        {
            List<Dictionary<string, PlayerDataObject>> playerData = LobbyManager.Instance.GetPlayersData();
            _lobbyPlayerData.Clear();

            int numberOfPlayerReady = 0;

            foreach (Dictionary<string, PlayerDataObject> data in playerData)
            {
                LobbyPlayerData lobbyPlayerData = new LobbyPlayerData();
                lobbyPlayerData.Initialize(data);

                if (lobbyPlayerData.IsReady)
                {
                    numberOfPlayerReady++;
                }

                if (lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
                {
                    _localLobbyPlayerData = lobbyPlayerData;
                }

                _lobbyPlayerData.Add(lobbyPlayerData);
            }

            _lobbyData=new LobbyData();
            _lobbyData.Initialize(lobby.Data);

            Events.LobbyEvents.OnLobbyUpdated?.Invoke();

            if(numberOfPlayerReady==lobby.Players.Count)
            {
                Events.LobbyEvents.OnLobbyReady?.Invoke();
            }
        }

        public List<LobbyPlayerData> GetPlayers()
        {
           return _lobbyPlayerData;
        }

        public async Task<bool> SetPlayerReady()
        {
            _localLobbyPlayerData.IsReady = true;
            return await LobbyManager.Instance.UpdatePlayerData(_localLobbyPlayerData.Id,_localLobbyPlayerData.Serialize());
        }

        public async Task StartGame(string sceneName)
        {
            string JoinRelayCode= await RelayManager.Instance.CreateRelay(_maxNumberOfPlayers);

            _lobbyData.SetRelayJoinCode(JoinRelayCode);
            await LobbyManager.Instance.UpdateLobbyData(_lobbyData.Serialize());

            string allocationId =  RelayManager.Instance.GetAllocationId();
            string connectionData =  RelayManager.Instance.GetConnectionData();
            await LobbyManager.Instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize(),allocationId,connectionData);

            SceneManager.LoadScene(sceneName);
        }
    }
}