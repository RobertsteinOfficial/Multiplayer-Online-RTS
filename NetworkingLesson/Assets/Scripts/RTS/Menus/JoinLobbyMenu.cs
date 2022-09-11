using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMP_InputField addressInput = null;
    [SerializeField] private Button joinButton = null;

    private void OnEnable()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected -= HandleClientDisconnected;
    }

    public void Join()
    {
        string address = addressInput.text;

        //Cerco di connettermi all'address specificato nell' Input Field
        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient();

        //Disabilito il button per sicurezza
        joinButton.interactable = false;
    }

    /// <summary>
    /// Funzione che chiamo in caso di avvenuta connessione
    /// </summary>
    private void HandleClientConnected()
    {
        //Il button lo rendo di nuovo interagibile in caso io esca e voglia joinare un'altra lobby
        joinButton.interactable = true;

        //Disattivo questa UI
        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
