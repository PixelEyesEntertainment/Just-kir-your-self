using UnityEngine;
using System.Collections.Generic;

public class UnitCommander : MonoBehaviour
{
    [Header("Keys")]
    public KeyCode attackMoveKey = KeyCode.A;
    public KeyCode stopKey = KeyCode.S;

    public enum CommandType { None, Move, AttackMove, Stop }

    [Header("Command State")]
    public CommandType pendingCommand = CommandType.None;

    public static bool IsCommandPending = false;
    public static bool BlockSelectionUntilMouseUp = false; // NEW: block selection until left mouse button is released

    [Header("Settings")]
    public LayerMask groundMask = -1;
    public GameObject moveMarkerPrefab;

    [Header("References")]
    public SelectSystem selectSystem;
    public Camera commandCamera;

    void Start()
    {
        if (selectSystem == null)
            selectSystem = FindObjectOfType<SelectSystem>();

        if (commandCamera == null)
        {
            commandCamera = Camera.main;
            if (commandCamera == null)
                Debug.LogError("[UnitCommander] No camera assigned!");
        }
    }

    void Update()
    {
        if (selectSystem == null || commandCamera == null) return;

        // ─── Detect command keys ─────────────────────────────────────────
        if (Input.GetKeyDown(attackMoveKey))
        {
            // Only allow if there are selected units
            List<Selectable> selected = selectSystem.GetSelected();
            if (selected != null && selected.Count > 0)
            {
                pendingCommand = CommandType.AttackMove;
                IsCommandPending = true;
                Debug.Log("[UnitCommander] Attack‑Move pending. Click left‑mouse to execute.");
            }
            else
            {
                // No units selected – do nothing
                pendingCommand = CommandType.None;
                IsCommandPending = false;
            }
        }
        else if (Input.GetKeyDown(stopKey))
        {
            ExecuteStop();
            pendingCommand = CommandType.None;
            IsCommandPending = false;
            BlockSelectionUntilMouseUp = false;
        }

        // ─── Left‑click: execute pending command ────────────────────────
        if (Input.GetMouseButtonDown(0) && pendingCommand != CommandType.None)
        {
            Ray ray = commandCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
            {
                List<Selectable> selected = selectSystem.GetSelected();
                if (selected == null || selected.Count == 0)
                {
                    pendingCommand = CommandType.None;
                    IsCommandPending = false;
                    BlockSelectionUntilMouseUp = false;
                    return;
                }

                switch (pendingCommand)
                {
                    case CommandType.AttackMove:
                        ExecuteAttackMove(selected, hit.point);
                        break;
                    default:
                        ExecuteMove(selected, hit.point);
                        break;
                }

                if (moveMarkerPrefab != null)
                    Instantiate(moveMarkerPrefab, hit.point, Quaternion.identity);

                // ─── Block selection until mouse button is released ──────
                BlockSelectionUntilMouseUp = true;
                IsCommandPending = false;
                pendingCommand = CommandType.None;
            }
            else
            {
                pendingCommand = CommandType.None;
                IsCommandPending = false;
                BlockSelectionUntilMouseUp = false;
            }
        }

        // ─── Right‑click: normal move ────────────────────────────────────
        if (Input.GetMouseButtonDown(1))
        {
            if (pendingCommand != CommandType.None)
            {
                pendingCommand = CommandType.None;
                IsCommandPending = false;
                BlockSelectionUntilMouseUp = false;
                Debug.Log("[UnitCommander] Command cancelled.");
                return;
            }

            Ray ray = commandCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
            {
                List<Selectable> selected = selectSystem.GetSelected();
                if (selected == null || selected.Count == 0) return;

                if (moveMarkerPrefab != null)
                    Instantiate(moveMarkerPrefab, hit.point, Quaternion.identity);

                ExecuteMove(selected, hit.point);
            }
        }
    }

    // ─── Command Execution ───────────────────────────────────────────────

    private void ExecuteMove(List<Selectable> units, Vector3 destination)
    {
        foreach (Selectable sel in units)
        {
            if (sel == null) continue;
            MoveToTarget mover = sel.GetComponent<MoveToTarget>();
            if (mover != null)
                mover.SetDestination(destination);
        }
    }

    private void ExecuteAttackMove(List<Selectable> units, Vector3 destination)
    {
        foreach (Selectable sel in units)
        {
            if (sel == null) continue;
            MoveToTarget mover = sel.GetComponent<MoveToTarget>();
            if (mover != null)
                mover.SetAttackMove(destination);
        }
    }

    private void ExecuteStop()
    {
        List<Selectable> selected = selectSystem.GetSelected();
        if (selected == null || selected.Count == 0) return;

        foreach (Selectable sel in selected)
        {
            if (sel == null) continue;
            MoveToTarget mover = sel.GetComponent<MoveToTarget>();
            if (mover != null)
                mover.StopMoving();
        }
    }

    // ─── Public methods ──────────────────────────────────────────────────

    public void ClearCommand()
    {
        pendingCommand = CommandType.None;
        IsCommandPending = false;
        BlockSelectionUntilMouseUp = false;
    }

    public void SetCommand(CommandType command)
    {
        pendingCommand = command;
        IsCommandPending = true;
        BlockSelectionUntilMouseUp = false;
    }

    public void StopSelected() => ExecuteStop();
}