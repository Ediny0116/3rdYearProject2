using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class Init : MonoBehaviour
    {
        // Start is called before the first frame update
        async void Start()
        {
            await UnityServices.InitializeAsync();

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                AuthenticationService.Instance.SignedIn += OnSignedIn;

                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn)
                {
                    string username = PlayerPrefs.GetString(key: "Username");
                    if (username == "")
                    {
                        username = "Player";
                        PlayerPrefs.SetString(key: "Username", username);
                    }
                    SceneManager.LoadScene("MainMenu");
                }
            }
            else
            {
                Debug.Log("Unity Services Failed to Initialize");
            }

        }

        private void OnSignedIn()
        {
            Debug.Log(message: $"Player Id:{AuthenticationService.Instance.PlayerId}");
            Debug.Log(message: $"Token: {AuthenticationService.Instance.AccessToken}");
        }
    }
}
