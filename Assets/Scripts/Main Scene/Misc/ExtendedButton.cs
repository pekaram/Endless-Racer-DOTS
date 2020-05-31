using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

/// <summary>
/// The purpose is exposing more than the button's OnClick event.
/// </summary>
public class ExtendedButton : Button
{
    public event Action OnButtonDown;

    public event Action OnButtonUp;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        this.OnButtonDown?.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        this.OnButtonUp?.Invoke();
    }
}