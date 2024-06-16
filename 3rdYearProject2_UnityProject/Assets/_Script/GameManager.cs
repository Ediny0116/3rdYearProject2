using GameFramework.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform RespawnPoint;
    public GameObject Lball, Bball;
    [SerializeField] GameObject endMenu;

    private void Start()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval=true;
        if (RelayManager.Instance.IsHost)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback = connectionApproval;
            (byte[] allocationId, byte[] key, byte[] connectionData, string ip, int port)=RelayManager.Instance.GetHostConnectionInfo();
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(ip,(ushort)port,allocationId,key,connectionData,isSecure:true);
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            (byte[] allocationId, byte[] key, byte[] connectionData, byte[] hostConnectionData, string ip, int port) = RelayManager.Instance.GetClientConnectionInfo();
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(ip, (ushort)port, allocationId, key, connectionData,hostConnectionData, isSecure: true);
            NetworkManager.Singleton.StartClient();
        }
    }

    private void connectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved= true;
        response.CreatePlayerObject= true;
        response.Pending= false;
    }

    public void SpawnLBall()
    {
        Instantiate(Lball, RespawnPoint.position, Quaternion.identity);
    }
    public void SpawnBBall()
    {
        Instantiate(Bball, RespawnPoint.position, Quaternion.identity);
    }

    public void EndGame()
    {
        endMenu = GameObject.Find("EndMenu");
        endMenu.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        Time.timeScale = 0f;
        endMenu.transform.Find("Text (TMP)EndScore").GetComponent<TMPro.TextMeshProUGUI>().text = "Score:" + GameObject.Find("Text (TMP)Score").GetComponent<Score>().GetScore();
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        GameObject.Find("Timer").GetComponent<Timer>().SetRemainingTime(60);
    }
}
