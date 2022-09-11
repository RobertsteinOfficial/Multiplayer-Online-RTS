using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Mirror;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    //uso l'update per pulire il path dell'agent lato server quando la unit è arrivata a destinazione
    //Invece di usare [Server], che mi darebbe warning, uso [ServerCallback]. Di base uso [Server] sui metodi che chiamo 
    //quando dico io, mentre [ServerCallback] sui metodi su cui non ho controllo, come in questo caso.
    [ServerCallback]
    private void Update()
    {
        //se ho un target vado in chase e skippo la parte di movimento
        Targetable target = targeter.GetTarget();
        if (target != null)
        {
            //uso sqrtmagnitude per la distanza perchè Vector3.Distance usa la radice quadrata, che è molto più lenta e noi 
            //stiamo lavorando in Update, e questo Update gira su ogni unit
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            else if (agent.hasPath)
            {
                agent.ResetPath();
            }

            return;
        }

        if (!agent.hasPath) { return; }      //prevengo il fatto che lo script cerchi di pulire il path nello stesso frame in cui lo 
                                             //settiamo
        if (agent.remainingDistance > agent.stoppingDistance) { return; }

        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    [Server]
    public void ServerMove(Vector3 position)
    {
        //prima di muovermi faccio il clear del target
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }

    #endregion


}
