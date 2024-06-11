using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI;

    private void OnEnable()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
    }

    private void OnDisable()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    public void LeaveLobby()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }
    }
}
