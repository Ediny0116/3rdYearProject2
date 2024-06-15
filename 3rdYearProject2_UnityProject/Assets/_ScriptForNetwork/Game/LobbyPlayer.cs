using Game.GameFramework.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Game
{
    internal class LobbyPlayer:MonoBehaviour
    {
        [SerializeField] private TextMeshPro _playerName;

        private LobbyPlayerData _data;
        
        public void SetData(LobbyPlayerData data)
        {
            _data = data;
            _playerName.text = _data.GamerTag;
            gameObject.SetActive(true);
        }
    }
}
