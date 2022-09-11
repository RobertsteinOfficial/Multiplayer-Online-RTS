using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//Ci sono un sacco di cose che possono subire danno in un RTS: unità, edifici, ostacoli ambientali.
//Sarebbe abbastanza stupido gestire la cosa con script diversi per ogni oggetto. Questo script sarà comune a tutte le entità
//che possono subire danno
public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;      //Gli hp attuali li gestiamo solo lato server, per cui usiamo una SyncVar
                                    //a cui agganceremo un hook

    public event Action ServerOnDie;
    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId)
    {
        //Se il player morto non siamo noi facciamo return
        if(connectionToClient.connectionId != connectionId) { return; }

        //Se no uccidiamo tutte le nostre unità rimaste in vita
        DealDamage(currentHealth);
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currentHealth == 0) { return; }

        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if (currentHealth != 0) { return; }

        ServerOnDie?.Invoke();      //la morte non la faccio qua, perchè è l'unica cosa che differisce tra i vari oggetti
                                    //lancio però una Action
    }

    #endregion

    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }

    #endregion
}
