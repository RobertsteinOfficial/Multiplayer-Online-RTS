using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;

    public static event Action<string> ClientOnGameOver;

    private List<UnitBase> bases = new List<UnitBase>();    //Qua ci va la base di ogni player

    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
    }

    /// <summary>
    /// Ogni volta che una base spawna (quindi un player si è unito al game) aggiungo la base alla lista
    /// </summary>
    /// <param name="unitBase"></param>
    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    /// <summary>
    /// Quando la propria base viene distrutta il player è eliminato
    /// Tolgo la sua base dalla lista, e controllo quanti player rimangono
    /// Se rimane un solo player, è game over
    /// /// </summary>
    /// <param name="unitBase"></param>
    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        if (bases.Count != 1) return;

        //Cerco l'unico player rimasto nella lista per passarlo come vincitore all'evento
        int playerID = bases[0].connectionToClient.connectionId;

        RpcGameOver($"Player {playerID}");

        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client

    /// <summary>
    /// Lanciamo l'evento del game over, passando il nome del vincitore come argomento
    /// </summary>
    /// <param name="winner"></param>
    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
