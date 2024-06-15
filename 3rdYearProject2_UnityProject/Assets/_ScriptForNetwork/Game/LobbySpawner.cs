using Game.Events;
using Game.GameFramework.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    internal class LobbySpawner:MonoBehaviour
    {
        [SerializeField] private List<LobbyPlayer> _players;

        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }

        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        }

        private void OnLobbyUpdated()
        {
            List<LobbyPlayerData> playerDatas = GameLobbyManager.Instance.GetPlayers();

            for(int i = 0; i < playerDatas.Count; i++)
            {
                LobbyPlayerData data = playerDatas[i];
                _players[i].SetData(data);
            }
        }
    }
}
