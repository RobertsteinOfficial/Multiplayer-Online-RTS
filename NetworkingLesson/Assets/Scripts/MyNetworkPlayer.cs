using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class MyNetworkPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleDisplayNameUpdated))]
    [SerializeField]
    private string displayName = "Missing Name";

    [SyncVar(hook = nameof(HandleDisplayColourUpdated))]
    [SerializeField]
    private Color displayColour = Color.black;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Renderer rend;

    #region Server

    [Server]
    public void SetDisplayName(string newName)
    {
        displayName = newName;
    }

    [Server]
    public void SetDisplayColour(Color newColour)
    {
        displayColour = newColour;
    }

    [Command]
    private void CmdSetDisplayName(string newDisplayName)
    {
        if(newDisplayName.Length < 2 || newDisplayName.Length > 20) { return; }


        RpcLogNewName(newDisplayName);

        SetDisplayName(newDisplayName);
    }

    #endregion

    #region Client

    private void HandleDisplayNameUpdated(string oldName, string newName)
    {
        nameText.text = newName;
    }

    private void HandleDisplayColourUpdated(Color oldColour, Color newColour)
    {
        rend.material.SetColor("_BaseColor", newColour);
    }


    [ContextMenu("Set My Name")]
    private void SetMyName()
    {
        CmdSetDisplayName("M");
    }

    [ClientRpc]
    private void RpcLogNewName(string newDisplayName)
    {
        Debug.Log(newDisplayName);
    }
    #endregion




}
