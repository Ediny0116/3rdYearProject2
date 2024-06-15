using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _lobbyCodeText;
        [SerializeField] Button buttonCopy;

        // Start is called before the first frame update
        void Start()
        {
            _lobbyCodeText.text = $"Lobby Code: {GameLobbyManager.Instance.GetLobbyCode()}";
        }
        private void OnEnable()
        {
            buttonCopy.onClick.AddListener(CopyText);
        }

        private void CopyText()
        {
            TextEditor te = new TextEditor();
            te.text = GameLobbyManager.Instance.GetLobbyCode();
            te.SelectAll();
            te.Copy();
        }
    }
}
