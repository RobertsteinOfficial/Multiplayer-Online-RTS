using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Targeter : NetworkBehaviour       //Questo va su tutte le unit
{
    private Targetable target;

    public Targetable GetTarget()
    {
        return target;
    }

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    /// <summary>
    /// Dobbiamo settare il target sul server, per poi aggiornare i client.
    /// L'idea è di fare come con il movimento sul CommandGiver. La differenza però è che là passiamo un Vector3, che è
    /// facilmente tracciabile da Mirror. Non è il caso di una classe custom come il Targetable. Come workaround possiamo
    /// passargli direttamente il GameObject, che nel caso abbia un component NetworkIdentity non ha problemi a venire tracciato
    /// </summary>
    /// <param name="targetGameObject"></param>
    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }

        target = newTarget;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    [Server]
    private void ServerHandleGameOver()
    {
        ClearTarget();
    }

}
