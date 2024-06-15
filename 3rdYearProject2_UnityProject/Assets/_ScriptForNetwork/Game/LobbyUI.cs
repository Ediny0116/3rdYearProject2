using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _lobbyCodeText;
        [SerializeField] Button buttonCopy;
        [SerializeField] Button buttonQuit;
        [SerializeField] Button _readyButton;
        [SerializeField] Button _startButton;

        // Start is called before the first frame update
        void Start()
        {
            _lobbyCodeText.text = $"Lobby Code: {GameLobbyManager.Instance.GetLobbyCode()}";
        }
        private void OnEnable()
        {
            buttonCopy.onClick.AddListener(CopyText);
            buttonQuit.onClick.AddListener(QuitLobby);
            _readyButton.onClick.AddListener(OnReadyPressed);

            if (GameLobbyManager.Instance.IsHost)
            {
                Events.LobbyEvents.OnLobbyReady+=OnLobbyReady;
                _startButton.onClick.AddListener(OnStartButtonClicked);
            }


        }

        private void OnDisable()
        {
            buttonCopy.onClick.RemoveAllListeners();
            buttonQuit.onClick.RemoveAllListeners();
            _readyButton.onClick.RemoveAllListeners();
            _startButton.onClick.RemoveAllListeners();

            Events.LobbyEvents.OnLobbyReady -= OnLobbyReady;
        }

        private void OnLobbyReady()
        {
            _startButton.gameObject.SetActive(true);
        }

        private async void OnReadyPressed()
        {
            bool succeedeed = await GameLobbyManager.Instance.SetPlayerReady();
            if (succeedeed)
            {
                _readyButton.interactable = false;
            }
        }

        private void QuitLobby()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void CopyText()
        {
            TextEditor te = new TextEditor();
            te.text = GameLobbyManager.Instance.GetLobbyCode();
            te.SelectAll();
            te.Copy();
        }

        private async void OnStartButtonClicked()
        {
            await GameLobbyManager.Instance.StartGame("Game");
        }
    }
}
