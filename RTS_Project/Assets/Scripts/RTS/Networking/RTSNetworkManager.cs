using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;


public class RTSNetworkManager : NetworkManager
{
    [Space(10)]
    [Header("RTS")]
    [SerializeField] private GameObject unitBasePrefab;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        ClientOnDisconnected?.Invoke();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Color col = new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)
            );

        player.SetTeamColour(col);

    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Map_"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
        }
    }
}
