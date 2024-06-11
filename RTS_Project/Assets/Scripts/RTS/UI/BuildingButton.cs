using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Mirror;
using UnityEngine.InputSystem;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private LayerMask floorMask;

    private Camera mainCamera;
    private RTSPlayer player;
    private GameObject buildingPreviewInstance;
    private Renderer buildingRendererInstance;
    private BoxCollider buildingCollider;


    private void Start()
    {
        mainCamera = Camera.main;

        iconImage.sprite = building.Icon;
        priceText.text = building.Price.ToString();

        buildingCollider = building.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        if (buildingPreviewInstance == null) return;

        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (player.Resources < building.Price) return;

        buildingPreviewInstance = Instantiate(building.BuildingPreview);
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();

        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buildingPreviewInstance == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            player.CmdTryPlaceBuilding(building.Id, hit.point);
        }

        Destroy(buildingPreviewInstance);
    }

    private void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) return;

        buildingPreviewInstance.transform.position = hit.point;

        if (!buildingPreviewInstance.activeSelf)
            buildingPreviewInstance.SetActive(true);

        Color color = player.CanPlaceBuilding(buildingCollider, hit.point) ? Color.green : Color.red;
        buildingRendererInstance.material.SetColor("_Color", color);
    }

}
