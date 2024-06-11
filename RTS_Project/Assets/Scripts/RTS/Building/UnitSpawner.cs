using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Health healthComponent;
    [SerializeField] private TMP_Text remainingUnitsText;
    [SerializeField] private Image unitProgressImage;

    [Header("Settings")]
    [SerializeField] private int maxUnitInQueue;
    [SerializeField] private float spawnMoveRange = 5f;
    [SerializeField] private float unitSpawnDuration = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    float progressImageVelocty;

    private void Update()
    {
        if(isServer)
        {
            ProduceUnits();
        }

        if(isClient)
        {
            UpdateTimerDisplay();
        }
    }

    #region Server

    public override void OnStartServer()
    {
        healthComponent.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        healthComponent.ServerOnDie -= ServerHandleDie;
    }

    [Command]
    private void CmdSpawnUnit()
    {
        if (queuedUnits == maxUnitInQueue) return;

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if (player.Resources < unitPrefab.ResourceCost) return;

        queuedUnits++;
        player.SetResources(player.Resources - unitPrefab.ResourceCost);

    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void ProduceUnits()
    {
        if (queuedUnits == 0) return;

        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnDuration) return;

        GameObject unitInstance = Instantiate(unitPrefab.gameObject, spawnPoint.position, spawnPoint.rotation);
        NetworkServer.Spawn(unitInstance, connectionToClient);

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = spawnPoint.position.y;

        queuedUnits--;
        unitTimer = 0f;
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!isOwned) return;

        CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitsText.text = newUnits.ToString();
    }

    private void UpdateTimerDisplay()
    {
        float progress = unitTimer / unitSpawnDuration;

        if(progress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = progress;
        }
        else
        {
            unitProgressImage.fillAmount = 
                Mathf.SmoothDamp(unitProgressImage.fillAmount, progress, ref progressImageVelocty, 0.1f);
        }
    }

    #endregion
}
