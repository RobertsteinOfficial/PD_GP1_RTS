using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TeamColourSetter : NetworkBehaviour
{
    [SerializeField] private Renderer[] renderers = new Renderer[0];

    [SyncVar(hook = nameof(HandleTeamColourUpdated))]
    private Color teamColour = new Color();

    #region Server

    public override void OnStartServer()
    {
        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
        teamColour = player.TeamColour;
    }

    #endregion

    #region Client

    private void HandleTeamColourUpdated(Color oldColour, Color newColour)
    {
        foreach(Renderer renderer in renderers)
        {
            renderer.material.SetColor("_Color", newColour);
        }
    }

    #endregion
}
