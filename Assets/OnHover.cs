using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool Hovered;
    public Action OnHoverBegin;
    public Action OnHoverEnd;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hovered = true;
        OnHoverBegin?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hovered = false;
        OnHoverEnd?.Invoke();
    }
}