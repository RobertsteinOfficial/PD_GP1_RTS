using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Building : NetworkBehaviour
{
    [SerializeField] private Sprite icon;
    [SerializeField] private int id = -1;
    [SerializeField] private int price = 100;

    public Sprite Icon { get { return icon; } }
    public int Id { get { return id; } }
    public int Price { get { return price; } }

    public static event Action<Building> ServerOnBuildingSpawned;
    public static event Action<Building> ServerOnBuildingDespawned;

    #region Server

    public override void OnStartServer()
    {
        ServerOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawned?.Invoke(this);
    }

    #endregion
}
