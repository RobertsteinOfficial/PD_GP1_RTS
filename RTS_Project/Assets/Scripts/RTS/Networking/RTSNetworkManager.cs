using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;


public class RTSNetworkManager : NetworkManager
{
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    [Space(10)]
    [Header("RTS")]
    [SerializeField] private GameObject unitBasePrefab;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    private bool isGameInProgress = false;
    public bool IsGameInProgress { get { return isGameInProgress; } }

    #region Server

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Add(player);

        player.SetPlayerName($"Player {Players.Count}");

        Color col = new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)
            );

        player.SetTeamColour(col);


        player.SetIsPartyOwner(Players.Count == 1);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Map_"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach(RTSPlayer player in Players)
            {
                GameObject baseInstance = Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);
                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }

        }
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (!isGameInProgress) return;

        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Remove(player);
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();

        isGameInProgress = false;
    }

    public void StartGame()
    {
        if (Players.Count < 2) return;

        isGameInProgress = true;

        ServerChangeScene("Map_Level0");
    }

    #endregion


    #region Client
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

    public override void OnStopClient()
    {
        Players.Clear();
    }

    #endregion
}
