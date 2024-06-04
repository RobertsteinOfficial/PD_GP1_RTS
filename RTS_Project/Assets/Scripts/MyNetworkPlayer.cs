using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class MyNetworkPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleDisplayNameUpdate))]
    [SerializeField]
    string displayName = "Unassigned";

    [SyncVar(hook = nameof(HandleDisplayColourUpdate))]
    [SerializeField]
    Color displayColour = Color.white;

    [SerializeField] TextMeshProUGUI displayNameText;
    [SerializeField] MeshRenderer meshRenderer;


    #region Server

    [Server]
    public void SetDisplayName(string newName)
    {
        displayName = newName;
    }

    [Server]
    public void SetDisplayColor(Color newColor)
    {
        displayColour = newColor;
    }

    [Command]
    private void CmdSetDisplayName(string newName)
    {
        if (newName.Length < 4 || newName.Length > 10) return;

        SetDisplayName(newName);

        RpcLognewName(newName);
    }

    #endregion

    #region Client

    private void HandleDisplayColourUpdate(Color oldColour, Color newColour)
    {
        meshRenderer.material.SetColor("_Color", newColour);
    }

    private void HandleDisplayNameUpdate(string oldName, string newName)
    {
        displayNameText.text = newName;
    }

    [ContextMenu("Set New Name")]
    private void SetMyName()
    {
        CmdSetDisplayName("Name1");
    }

    [ClientRpc]
    private void RpcLognewName(string newName)
    {
        Debug.Log(newName);
    }



    #endregion
}
