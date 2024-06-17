using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _mainScreen;
        [SerializeField] private GameObject _JoinScreen;
        [SerializeField] private GameObject _manual;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _submitCodeButton;
        [SerializeField] private TextMeshProUGUI _codeText;
        [SerializeField] Button buttonPageUp;
        [SerializeField] Button ButtonManual;

        bool isManualOpened = false;
        void OnEnable()
        {
            _hostButton.onClick.AddListener(OnHostClicked);
            _joinButton.onClick.AddListener(OnJoinClicked);
            _submitCodeButton.onClick.AddListener(OnSubmitCodeClicked);
            buttonPageUp.onClick.AddListener(PageUp);
            ButtonManual.onClick.AddListener(Manual);
        }

        private void Manual()
        {  
            if (isManualOpened)
            {
                _manual.SetActive(false);
                _mainScreen.SetActive(true);
                isManualOpened = false;
            }
            else
            {
                _manual.SetActive(true);
                _mainScreen.SetActive(false);
                _JoinScreen.SetActive(false);
                isManualOpened = true;
            }
        }

        void OnDisable()
        {
            _hostButton.onClick.RemoveListener(OnHostClicked);
            _joinButton.onClick.RemoveListener(OnJoinClicked);
            _submitCodeButton.onClick.RemoveListener(OnSubmitCodeClicked);
            buttonPageUp.onClick.RemoveListener(PageUp);
            ButtonManual.onClick.RemoveListener(Manual);
        }

        private void OnJoinClicked()
        {
            _mainScreen.SetActive(false);
            _JoinScreen.SetActive(true);

        }

        private void PageUp()
        {
            _mainScreen.SetActive(true);
            _JoinScreen.SetActive(false);
        }

        private async void OnHostClicked()
        {
            bool succeeded = await GameLobbyManager.Instance.CreateLobby();
            if (succeeded)
            {
                await SceneManager.LoadSceneAsync("Lobby");
            }
        }

        private async void OnSubmitCodeClicked()
        {
            string code = _codeText.text;
            try
            {
                code = code.Substring(0, 6); //TMPro問題會導致出現多餘字元
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            Debug.Log("(" + code + ")");

            bool succeeded = await GameLobbyManager.Instance.JoinLobby(code);
            if (succeeded)
            {
                Debug.Log("Joined lobby");
                await SceneManager.LoadSceneAsync("Lobby");
            }
            else
            {
                Debug.Log("Failed to join lobby");
            }
        }
    }
}
