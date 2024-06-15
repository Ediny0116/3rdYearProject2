using GameFramework.Core;
using GameFramework.Core.GameFramework.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using UnityEngine;


public class GameLobbyManager : Singleton<GameLobbyManager>
{
    public string GetLobbyCode()
    {
        return LobbyManager.Instance.GetLobbyCode();
    }

    public async Task<bool> CreateLobby()
    {
        Dictionary<string, string> playerData = new Dictionary<string, string>()
        {
            {"GamerTag","HostPlayer" }
        };
        bool succeeded =await LobbyManager.Instance.CreateLobby(maxPlayers: 2,isPrivate:false ,playerData);
        return succeeded;
    }

    public async Task<bool> JoinLobby(string code)
    {
        Dictionary<string, string> playerData = new Dictionary<string, string>()
        {
            {"GamerTag","JoinPlayer" }
        };
        bool succeeded = await LobbyManager.Instance.JoinLobby(code, playerData);
        return succeeded;
    }
}
