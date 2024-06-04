using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Health healthComponent;

    #region Server

    //public override void OnStartServer()
    //{
    //    healthComponent.ServerOnDie += ServerHandleDie;
    //}

    //public override void OnStopServer()
    //{
    //    healthComponent.ServerOnDie -= ServerHandleDie;
    //}

    [Command]
    private void CmdSpawnUnit()
    {
        GameObject unitInstance = Instantiate(unitPrefab, spawnPoint.position, spawnPoint.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!isOwned) return;

        CmdSpawnUnit();
    }

    #endregion
}
