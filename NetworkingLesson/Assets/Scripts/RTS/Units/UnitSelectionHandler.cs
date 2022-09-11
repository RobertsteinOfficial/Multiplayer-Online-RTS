using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask = new LayerMask();
    [SerializeField] private RectTransform unitSelectionArea = null;

    private Vector2 startPosition;

    private RTSPlayer player;
    private Camera mainCamera;
    public List<Unit> SelectedUnits { get; } = new List<Unit>();

    private void Start()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    /// <summary>
    /// Faccio il clear delle unit selezionate, mi preparo a un'altra selezione
    /// </summary>
    private void StartSelectionArea()
    {
        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.DeSelect();
            }

            SelectedUnits.Clear();
        }

        unitSelectionArea.gameObject.SetActive(true);
        startPosition = Mouse.current.position.ReadValue();
        //chiamo anche qua manualmente, perchè voglio sia chiamato anche al primo frame
        UpdateSelectionArea();
    }

    /// <summary>
    /// Aggiorno l'area di selezione in base al movimento del mouse
    /// </summary>
    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    /// <summary>
    /// una volta rilasciato il tasto del mouse disattivo la selection area e aggiungo le unit selezionate alla lista
    /// </summary>
    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        //se l'area ha magnitudine 0 vuol dire che ho fatto solo un click. Faccio raycast, se becco la unit la seleziono e
        //faccio return
        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }
            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) { return; }
            if (!unit.hasAuthority) { return; }

            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }

            return;
        }


        //Se no converto in scren position la posizione delle unit, e controllo che queste posizioni siano all'interno dei 
        //confini dell'area di selezione. Le unit che soddisfano questa condizione le seleziono
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (Unit unit in player.GetMyUnits())
        {
            if (SelectedUnits.Contains(unit)) { continue; }

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            if (screenPosition.x > min.x && screenPosition.x < max.x &&
                screenPosition.y > min.y && screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    /// <summary>
    /// Se una delle unit selezionate viene distrutta dobbiamo toglierla dalla lista
    /// </summary>
    /// <param name="unit"></param>
    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit); //Anche se la unit non è nella lista non avremo errori, perchè Remove fa il controllo per noi
                                    //Se ci fate caso infatti restituisce un bool
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
