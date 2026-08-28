using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Selectable : MonoBehaviour
{
    [Header("Selection Events")]
    public UnityEvent onSelect;
    public UnityEvent onDeselect;

    public bool IsSelected { get; private set; }

    private static List<Selectable> allSelectables = new List<Selectable>();

    void Awake() => allSelectables.Add(this);
    void OnDestroy() => allSelectables.Remove(this);

    public void Select()
    {
        if (IsSelected) return;
        IsSelected = true;
        onSelect.Invoke();
    }

    public void Deselect()
    {
        if (!IsSelected) return;
        IsSelected = false;
        onDeselect.Invoke();
    }

    public static List<Selectable> GetAll() => allSelectables;
}