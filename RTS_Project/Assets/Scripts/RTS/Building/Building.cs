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
    [SerializeField] private GameObject buildingPreview;

    public Sprite Icon { get { return icon; } }
    public int Id { get { return id; } }
    public int Price { get { return price; } }
    public GameObject BuildingPreview { get { return buildingPreview; } }

    public static event Action<Building> ServerOnBuildingSpawned;
    public static event Action<Building> ServerOnBuildingDespawned;

    public static event Action<Building> AuthorityOnBuildingSpawned;
    public static event Action<Building> AuthorityOnBuildingDespawned;

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

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isOwned) return;

        AuthorityOnBuildingDespawned?.Invoke(this);
    }

    #endregion
}
