using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [Header("Stats")]
    private int resourceCost = 10;
    public int ResourceCost { get { return resourceCost; } }

    [Header("References")]
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private Targeter targeter;
    [SerializeField] private Health healthComponent;

    public UnitMovement UnitMovement { get { return unitMovement; } }
    public Targeter Targeter { get { return targeter; } }

    [Header("Events")]
    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselected;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;

    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    #region Server

    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
        healthComponent.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        ServerOnUnitDespawned?.Invoke(this);
        healthComponent.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawned?.Invoke(this);
    }


    public override void OnStopClient()
    {
        if (!isOwned) return;

        AuthorityOnUnitDespawned?.Invoke(this);
    }


    [ClientCallback]
    public void Select()
    {
        if (!isOwned) return;

        onSelected?.Invoke();
    }

    [ClientCallback]
    public void Deselect()
    {
        if (!isOwned) return;

        onDeselected?.Invoke();
    }

    #endregion
}
