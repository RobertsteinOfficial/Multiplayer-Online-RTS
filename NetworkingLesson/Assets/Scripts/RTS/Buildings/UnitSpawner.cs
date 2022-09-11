using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private Unit unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private int macUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7;
    [SerializeField] private float unitSpawnDuration = 5f;

    [SyncVar (hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    private void Update()
    {
        //Ogni frame, se sono il server, cerco di produrre Unit
        if(isServer)
        {
            ProduceUnits();
        }

        //Se sono client, continuo ad aggiornare il timer
        if(isClient)
        {
            UpdateTimerDisplay();
        }
    }

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ProduceUnits()
    {
        if(queuedUnits == 0) { return; }

        unitTimer += Time.deltaTime;

        if(unitTimer < unitSpawnDuration) { return; }

        GameObject unitInstance = Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);
        NetworkServer.Spawn(unitInstance, connectionToClient);

        //Muovo la unit spawnata in un posto random per non farle sovrapporre
        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnPoint.position.y;
        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        queuedUnits--;
        unitTimer = 0f;
    }

    /// <summary>
    /// Logica per quando lo spawner muore, la aggancio all'evento lanciato dal component Health quando gli hp scendono a 0
    /// </summary>
    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnit()
    {
        if(queuedUnits == macUnitQueue) { return; }

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if(player.GetResources() < unitPrefab.GetResourceCost()) { return; }

        queuedUnits++;
        player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());
    }


    #endregion

    #region Client

    /// <summary>
    /// Aggiorno la UI del timer
    /// </summary>
    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        if(newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = 
                Mathf.SmoothDamp(unitProgressImage.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
        }


    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }
        if(!hasAuthority) { return; }

        CmdSpawnUnit();
    }

    /// <summary>
    /// Ogni volta che aggiorno le unit in queue chiamo questa funzione per aggiornare anche la UI
    /// </summary>
    /// <param name="oldUnits"></param>
    /// <param name="newUnits"></param>
    private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitsText.text = newUnits.ToString();
    }

    #endregion
}
