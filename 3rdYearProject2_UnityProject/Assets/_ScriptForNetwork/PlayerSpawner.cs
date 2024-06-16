using Game;
using Game.GameFramework.Core.Data;
using GameFramework.Core.GameFramework.Manager;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefabA; //add prefab in inspector
    [SerializeField] private GameObject playerPrefabB; //add prefab in inspector
    [SerializeField] private NetworkObject netObj;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            SpawnPlayerServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)] //server owns this object but client can request a spawn
    public void SpawnPlayerServerRpc()
    {
        bool _isHost = GameLobbyManager.Instance.IsHost;

        GameObject newPlayer;
        if (_isHost)
        {
            Debug.Log("Player spawn as Host");
            newPlayer = (GameObject)Instantiate(playerPrefabA);
        }
        else
        {
            Debug.Log("Player spawn as Client");
            newPlayer = (GameObject)Instantiate(playerPrefabB);
        }

        netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
    }
}

