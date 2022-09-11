using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

        if(hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            if(target.hasAuthority)
            {
                TryMove(hit.point);
                return;
            }

            TryTarget(target);
            return;
        }

        TryMove(hit.point);
    }

    /// <summary>
    /// Assegno  la unit su cui ho cliccato come target di tutte le unit che ho selezionato
    /// </summary>
    /// <param name="target"></param>
    private void TryTarget(Targetable target)
    {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter().CmdSetTarget(target.gameObject);
        }
    }

    private void TryMove(Vector3 point)
    {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().CmdMove(point);
        }
    }

    /// <summary>
    /// Questo serve semplicemente per far sì che al gameover lo script smette di far girare l'Update loop.
    /// Dobbiamo prendere come argomento una stringa perchè l'action a cui ci agganciamo prende come argomento una stringa, ma 
    /// non ci serve in realtà
    /// </summary>
    /// <param name="winnerName"></param>
    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}
