using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public event Action ServerOnDie;
    public event Action<int, int> ClientOnHealthUpdated;

    [SyncVar(hook = nameof(HandleOnHealthUpdated))]
    private int currentHealth;


    #region Server
    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    public void TakeDamage(int damage)
    {
        if (currentHealth == 0) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        if (currentHealth != 0) return;

        ServerOnDie?.Invoke();
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId)
    {
        if (connectionToClient.connectionId != connectionId) return;

        TakeDamage(currentHealth);
    }

    #endregion

    #region Client

    private void HandleOnHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }

    #endregion
}
