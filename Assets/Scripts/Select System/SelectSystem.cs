using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SelectSystem : MonoBehaviour
{
    [Header("Camera")]
    public Camera selectCamera;

    [Header("Selection Settings")]
    public KeyCode addModifier = KeyCode.LeftShift;

    [Header("Box Visuals")]
    public Color boxFillColor = new Color(0, 1, 0, 0.2f);
    public Color boxBorderColor = Color.green;

    [Header("Events")]
    public UnityEvent<List<Selectable>> onSelectionChanged;

    [Header("Debug (Read‑only)")]
    public List<Selectable> selected = new List<Selectable>();

    private bool isDragging;
    private Vector2 dragStart;
    private Vector2 dragEnd;
    private bool hasValidDragStart;

    void Start()
    {
        if (selectCamera == null)
        {
            selectCamera = Camera.main;
            if (selectCamera == null)
                Debug.LogError("[SelectSystem] No camera assigned and no MainCamera found!");
        }
        hasValidDragStart = false;
        dragStart = Vector2.zero;
    }

    void Update()
    {
        if (selectCamera == null) return;

        // ─── Block selection if a command is pending OR mouse is blocked ──
        if (UnitCommander.IsCommandPending || UnitCommander.BlockSelectionUntilMouseUp)
        {
            // Reset drag state
            isDragging = false;
            hasValidDragStart = false;
            dragStart = Vector2.zero;

            // If the mouse button is up, we can clear the block flag
            if (!Input.GetMouseButton(0) && UnitCommander.BlockSelectionUntilMouseUp)
            {
                UnitCommander.BlockSelectionUntilMouseUp = false;
            }
            return;
        }

        // ─── Left‑click handling ──────────────────────────────────────────
        if (Input.GetMouseButtonDown(0))
        {
            hasValidDragStart = true;
            dragStart = Input.mousePosition;
            dragEnd = dragStart;
            isDragging = false;
        }

        if (Input.GetMouseButton(0) && hasValidDragStart)
        {
            dragEnd = Input.mousePosition;
            if (Vector2.Distance(dragStart, dragEnd) > 10f)
            {
                if (!isDragging)
                {
                    isDragging = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                Rect screenRect = GetScreenRect(dragStart, dragEnd);
                List<Selectable> hits = GetSelectablesInRect(screenRect);
                ApplySelection(hits);
                isDragging = false;
            }
            else
            {
                Selectable hit = RaycastSelectable();
                if (hit != null)
                {
                    bool shift = Input.GetKey(addModifier);
                    if (shift)
                    {
                        if (selected.Contains(hit))
                            RemoveFromSelection(hit);
                        else
                            AddToSelection(hit);
                    }
                    else
                    {
                        SelectSingle(hit);
                    }
                }
                else if (!Input.GetKey(addModifier))
                {
                    ClearSelection();
                }
            }
            hasValidDragStart = false;
        }

        if (!Input.GetMouseButton(0))
            hasValidDragStart = false;
    }

    // ─── Rest of the class (unchanged) ────────────────────────────────────

    private Selectable RaycastSelectable()
    {
        Ray ray = selectCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            return hit.collider.GetComponent<Selectable>();
        return null;
    }

    private List<Selectable> GetSelectablesInRect(Rect screenRect)
    {
        List<Selectable> results = new List<Selectable>();
        foreach (Selectable sel in Selectable.GetAll())
        {
            if (sel == null || sel.gameObject == null) continue;

            Renderer rend = sel.GetComponent<Renderer>();
            if (rend != null)
            {
                if (RendererBoundsIntersectRect(rend, screenRect, selectCamera))
                    results.Add(sel);
            }
            else
            {
                Vector3 screenPos = selectCamera.WorldToScreenPoint(sel.transform.position);
                if (screenPos.z > 0 && screenRect.Contains(screenPos))
                    results.Add(sel);
            }
        }
        return results;
    }

    private bool RendererBoundsIntersectRect(Renderer rend, Rect screenRect, Camera cam)
    {
        Bounds bounds = rend.bounds;
        Vector3[] corners = new Vector3[8];
        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;

        corners[0] = c + new Vector3(-e.x, -e.y, -e.z);
        corners[1] = c + new Vector3(e.x, -e.y, -e.z);
        corners[2] = c + new Vector3(-e.x, e.y, -e.z);
        corners[3] = c + new Vector3(e.x, e.y, -e.z);
        corners[4] = c + new Vector3(-e.x, -e.y, e.z);
        corners[5] = c + new Vector3(e.x, -e.y, e.z);
        corners[6] = c + new Vector3(-e.x, e.y, e.z);
        corners[7] = c + new Vector3(e.x, e.y, e.z);

        foreach (Vector3 worldPos in corners)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
            if (screenPos.z > 0 && screenRect.Contains(screenPos))
                return true;
        }
        return false;
    }

    private Rect GetScreenRect(Vector2 start, Vector2 end)
    {
        float xMin = Mathf.Min(start.x, end.x);
        float xMax = Mathf.Max(start.x, end.x);
        float yMin = Mathf.Min(start.y, end.y);
        float yMax = Mathf.Max(start.y, end.y);
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private Rect GetGUIRect(Rect screenRect)
    {
        return new Rect(
            screenRect.x,
            Screen.height - screenRect.y - screenRect.height,
            screenRect.width,
            screenRect.height
        );
    }

    // ─── Selection Logic ───────────────────────────────────────────────

    private void ApplySelection(List<Selectable> newSelection)
    {
        List<Selectable> toDeselect = new List<Selectable>(selected);
        foreach (Selectable s in newSelection)
            toDeselect.Remove(s);
        foreach (Selectable s in toDeselect)
            s.Deselect();

        foreach (Selectable s in newSelection)
            if (!selected.Contains(s))
                s.Select();

        selected = new List<Selectable>(newSelection);
        onSelectionChanged?.Invoke(selected);
    }

    private void SelectSingle(Selectable target)
    {
        ClearSelection();
        AddToSelection(target);
    }

    private void AddToSelection(Selectable target)
    {
        if (!selected.Contains(target))
        {
            selected.Add(target);
            target.Select();
            onSelectionChanged?.Invoke(selected);
        }
    }

    private void RemoveFromSelection(Selectable target)
    {
        if (selected.Contains(target))
        {
            selected.Remove(target);
            target.Deselect();
            onSelectionChanged?.Invoke(selected);
        }
    }

    private void ClearSelection()
    {
        foreach (Selectable s in selected)
            s.Deselect();
        selected.Clear();
        onSelectionChanged?.Invoke(selected);
    }

    // ─── Public Getters ────────────────────────────────────────────────

    public List<Selectable> GetSelected() => selected;
    public int SelectedCount => selected.Count;

    // ─── GUI Box Drawing ──────────────────────────────────────────────

    void OnGUI()
    {
        if (!isDragging) return;

        Rect screenRect = GetScreenRect(dragStart, dragEnd);
        Rect guiRect = GetGUIRect(screenRect);

        GUI.color = boxFillColor;
        GUI.DrawTexture(guiRect, Texture2D.whiteTexture);

        GUI.color = boxBorderColor;
        Rect top = new Rect(guiRect.x, guiRect.y, guiRect.width, 2);
        Rect bottom = new Rect(guiRect.x, guiRect.y + guiRect.height - 2, guiRect.width, 2);
        Rect left = new Rect(guiRect.x, guiRect.y, 2, guiRect.height);
        Rect right = new Rect(guiRect.x + guiRect.width - 2, guiRect.y, 2, guiRect.height);
        GUI.DrawTexture(top, Texture2D.whiteTexture);
        GUI.DrawTexture(bottom, Texture2D.whiteTexture);
        GUI.DrawTexture(left, Texture2D.whiteTexture);
        GUI.DrawTexture(right, Texture2D.whiteTexture);

        GUI.color = Color.white;
    }
}