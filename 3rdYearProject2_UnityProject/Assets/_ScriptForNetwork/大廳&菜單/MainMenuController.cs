using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _joinButton;

    private void Start()
    {
        _hostButton.onClick.AddListener(OnHostClicked);
        _joinButton.onClick.AddListener(OnJoinClicked);
    }

    private void OnHostClicked()
    {
        GameLobbyManager.Instance.CreateLobby();
    }

    private void OnJoinClicked()
    {
        Debug.Log("Join");
    }
}
