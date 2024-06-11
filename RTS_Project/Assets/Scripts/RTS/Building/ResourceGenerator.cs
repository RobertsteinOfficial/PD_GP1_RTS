using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private int generationAmount = 10;
    [SerializeField] private float generationRate = 2f;

    private float timer;
    private RTSPlayer player;

    public override void OnStartServer()
    {
        player = connectionToClient.identity.GetComponent<RTSPlayer>();

        health.ServerOnDie += ServerHandleDie;
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update()
    {
        timer += Time.deltaTime;

        if(timer >= generationRate)
        {
            player.SetResources(player.Resources + generationAmount);
            timer = 0;
        }
    }

    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void ServerHandleGameOver()
    {
        enabled = false;
    }

}
