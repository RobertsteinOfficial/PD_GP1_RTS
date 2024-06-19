using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private LayerMask buildingBlockLayer;
    [SerializeField] private float buildingRangeLimit = 5f;
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private Transform cameraTransform;

    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

    public List<Unit> MyUnits { get { return myUnits; } }
    public List<Building> MyBuildings { get { return myBuildings; } }
    public Transform CameraTransform { get { return cameraTransform; } }

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;
    [SyncVar(hook = nameof(ClientHandlePlayerNameUpdated))]
    private string playerName;

    private Color teamColour = new Color();

    public int Resources { get { return resources; } }
    public Color TeamColour { get { return teamColour; } }
    public bool IsPartyOwner { get { return isPartyOwner; } }
    public string PlayerName { get { return playerName; } }

    public event Action<int> ClientOnResourceUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
    public static event Action ClientOnInfoUpdated;

    #region Server
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }

    private void ServerHandleUnitSpawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myUnits.Remove(unit);
    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myBuildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myBuildings.Remove(building);
    }

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {
        if (Physics.CheckBox
            (point + buildingCollider.center, buildingCollider.size / 2, Quaternion.identity, buildingBlockLayer))
            return false;


        foreach (Building building in myBuildings)
        {
            if ((point - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }

        return false;
    }

    [Command]
    public void CmdStartGame()
    {
        if (!isPartyOwner) return;

        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 point)
    {
        Building buildingToPlace = null;

        foreach (Building building in buildings)
        {
            if (building.Id == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }

        if (buildingToPlace == null) return;
        if (resources < buildingToPlace.Price) return;

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

        if (!CanPlaceBuilding(buildingCollider, point)) return;

        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(buildingInstance, connectionToClient);
        SetResources(resources - buildingToPlace.Price);
    }

    [Server]
    public void SetResources(int amount)
    {
        resources = amount;
    }

    [Server]
    public void SetTeamColour(Color newTeamColour)
    {
        teamColour = newTeamColour;
    }

    [Server]
    public void SetIsPartyOwner(bool value)
    {
        isPartyOwner = value;
    }

    [Server]
    public void SetPlayerName(string newName)
    {
        playerName = newName;
    }

    #endregion

    #region Client


    public override void OnStartClient()
    {
        if (NetworkServer.active) return;

        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();

        if (!isClientOnly) return;
        ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);
    }

    public override void OnStartAuthority()
    {
        if (NetworkServer.active) return;

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;

        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStopAuthority()
    {
        if (!isClientOnly) return;

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if (!isOwned) return;

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void ClientHandleResourcesUpdated(int oldResources, int newResources)
    {
        ClientOnResourceUpdated?.Invoke(newResources);
    }

    private void ClientHandlePlayerNameUpdated(string oldName, string newName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    #endregion
}
