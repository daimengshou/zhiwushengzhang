using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChartPlane : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public RectTransform canvasRect;
    private bool dragging;
    private Vector3 targetPos;
    private Vector3 offset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //if (dragging)
        //    canvasRect.position = targetPos;
	}

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, 
            out offset);

        offset  = canvasRect.position - offset;
        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pointerPos = eventData.position;
        
        //限定拖动范围
        if (pointerPos.x < 0 || pointerPos.x >= Screen.width ||
            pointerPos.y < 0 || pointerPos.y >= Screen.height)
            return;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, 
            out targetPos);

        targetPos += offset;
        canvasRect.position = targetPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
    }
}
