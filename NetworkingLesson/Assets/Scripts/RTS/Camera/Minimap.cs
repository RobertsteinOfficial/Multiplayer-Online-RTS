using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Mirror;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect = null;
    [SerializeField] private float mapScale = 20f;      //Assumiamo che la mappa sia un quadrato, perciò diamo una 
                                                        //sola dimensione per la scala
    [SerializeField] private float offset = -6;         //Offset della camera

    private Transform playerCameraTransform;

    private void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        
        playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();
    }

 
    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        //Prendo la posizione del mouse a schermo
        Vector2 mousePos = Mouse.current.position.ReadValue();

        //Se il mouse non è all'interno della zona della minimappa faccio return. 
        //RectTransformUtility come dice stesso il nome è una classe piena di utilities per lavorare con i RectTransform
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, mousePos, null, out Vector2 localPoint))
        { return; }

        //Ora abbiamo bisogno di trasporre la posizione sulla minimappa a una posizione nella mappa di gioco,
        //a prescindere dalle dimensioni della minimappa. In pratica dobbiamo convertire la posizione in valori 
        //percentuali
        Vector2 lerp = new Vector2(
            (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width,
            (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        //Mi calcolo la nuova posizione della camera
        Vector3 newCameraPos = new Vector3(
            Mathf.Lerp(-mapScale, mapScale, lerp.x),
            playerCameraTransform.position.y,
            Mathf.Lerp(-mapScale, mapScale, lerp.y));

        playerCameraTransform.position = newCameraPos + new Vector3(0f, 0f, offset);
    }

}
