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
        [SerializeField] private GameObject _isReadyTextObj;

        private LobbyPlayerData _data;
        
        public void SetData(LobbyPlayerData data)
        {
            _data = data;
            _playerName.text = _data.GamerTag;

            if (_data.IsReady)
            {
                _isReadyTextObj.SetActive(true);
            }
            else
            {
                _isReadyTextObj.SetActive(false);
            }

            gameObject.SetActive(true);
            Debug.Log("Join player " + _data.Id);
        }
    }
}
