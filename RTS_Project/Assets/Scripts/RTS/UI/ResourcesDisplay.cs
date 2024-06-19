using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText;

    private RTSPlayer player;

    private void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        ClientHandleResourcesUpdated(player.Resources);
        player.ClientOnResourceUpdated += ClientHandleResourcesUpdated;
    }

    private void OnDisable()
    {
        player.ClientOnResourceUpdated -= ClientHandleResourcesUpdated;
    }

   

    private void ClientHandleResourcesUpdated(int resources)
    {
        resourcesText.text = $"Resources : {resources}";
    }

}
