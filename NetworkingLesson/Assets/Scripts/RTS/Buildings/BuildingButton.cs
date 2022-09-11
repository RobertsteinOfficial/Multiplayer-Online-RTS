using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.InputSystem;

//Questa classe è un button custom che servirà a selezionare i building da spawnare e trascinarli in scena.
//Dato che voglio la funzione di drag e drop, il funzionamento del button di Unity non mi va bene
public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building = null;
    [SerializeField] private Image iconImage = null;
    //Costo in risorse (gold, minerali, whatever) del building
    [SerializeField] private TMP_Text priceText = null;
    //Ci serve una layermask per controllare se possiamo piazzare il building nella zona selezionata
    [SerializeField] private LayerMask floorMask = new LayerMask();

    private Camera mainCamera;
    private BoxCollider buildingCollider;
    private RTSPlayer player;
    private GameObject buildingPreviewInstance;
    private Renderer buildingRendererInstance;

    private void Start()
    {
        mainCamera = Camera.main;

        iconImage.sprite = building.GetIcon();
        priceText.text = building.GetPrice().ToString();

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        buildingCollider = building.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (buildingPreviewInstance == null) { return; }

        UpdateBuildingPreview();
    }

    //Queste due funzioni sono implementazioni delle interfacce IPointerDownHandler e IPointerUpHandler
    public void OnPointerDown(PointerEventData eventData)
    {
        //Se non sto cliccando col sinistro faccio return
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        //Se non ho abbastanza risorse per spawnare il building idem
        if (player.GetResources() < building.GetPrice()) { return; }

        //Istanzio la preview del building e mi salvo il suo renderer
        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();
        //Lo setto "invisibile" di default, poi lo farò tornare visibile non appena si troverà in una posizione valida
        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buildingPreviewInstance == null) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            //Qua piazziamo il building vero e proprio. O meglio chiamo una funzione del player, dato che questo
            //è uno script Monobehaviour e quindi non posso fare roba di networking qua dentro
            player.CmdTryPlaceBuilding(building.GetId(), hit.point);

        }

        //Distruggo la preview
        Destroy(buildingPreviewInstance);
    }

    /// <summary>
    /// Aggiorno la posizione della preview del building
    /// </summary>
    private void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }

        buildingPreviewInstance.transform.position = hit.point;

        if (!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }

        //Aggiorno il colore del renderer della preview del building a seconda se posso piazzarlo o meno in quel punto
        Color color = player.CanPlaceBuilding(buildingCollider, hit.point) ? Color.green : Color.red;
        buildingRendererInstance.material.SetColor("_BaseColor", color);
    }
}
