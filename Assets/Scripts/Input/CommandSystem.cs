using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using MyGame.Core.Units;
using MyGame.Presentation;
using MyGame.Core;
using MyGame.Input;
using MyGame.RuntimeSystems.Movement;
using MyGame.Core.Events;
using MyGame.Core.Services;

namespace MyGame.Input
{
    /// <summary>
    /// Handles unit commands and integrates with the new NavMesh movement system
    /// </summary>
    public class CommandSystem : MonoBehaviour
    {
        [Header("Command Settings")]
        [SerializeField] private float formationSpacing = 2f;
        [SerializeField] private bool useNavMeshValidation = true;
        [SerializeField] private float maxCommandDistance = 100f;
        
        [Header("Formation Settings")]
        [SerializeField] private bool useFormationMovement = true;
        [SerializeField] private FormationType defaultFormation = FormationType.Grid;
        
        private SelectionManager selectionManager;
        private INavigationMeshValidation pathfindingSystem;
        private SelectionRectangle selectionRectangle;

        public enum FormationType
        {
            Grid,
            Circle,
            Line,
            Wedge
        }

        private void Awake()
        {
            // Register with DependencyContainer
            DependencyContainer.Instance.Register(this);
            
            // Subscribe to input events
            InputEvents.OnUnitSelection += HandleUnitSelection;
            InputEvents.OnUnitCommand += HandleUnitCommand;
            
            // Subscribe to game events
            GameEvents.OnUnitMoveCommand += HandleMoveCommand;
        }

        private void Start()
        {
            // Get required systems
            selectionManager = DependencyContainer.Instance.TryResolve<MyGame.Presentation.SelectionManager>();
            pathfindingSystem = DependencyContainer.Instance.TryResolve<INavigationMeshValidation>();
            selectionRectangle = DependencyContainer.Instance.TryResolve<MyGame.Presentation.SelectionRectangle>();
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            InputEvents.OnUnitSelection -= HandleUnitSelection;
            InputEvents.OnUnitCommand -= HandleUnitCommand;
            GameEvents.OnUnitMoveCommand -= HandleMoveCommand;
        }

        private void HandleMoveCommand(IUnit unit, Vector3 destination)
        {
            if (unit is Unit concreteUnit)
            {
                // Validate target position on NavMesh
                Vector3 targetPosition = destination;
                if (useNavMeshValidation && pathfindingSystem != null)
                {
                    targetPosition = pathfindingSystem.GetNearestValidPosition(targetPosition);
                }
                
                // Check if target is too far
                float distance = Vector3.Distance(concreteUnit.transform.position, targetPosition);
                if (distance > maxCommandDistance)
                {
                    Debug.LogWarning($"Target too far: {distance:F1}m (max: {maxCommandDistance}m)");
                    return;
                }
                
                // Issue move command
                concreteUnit.MoveTo(targetPosition);
            }
        }

        private void HandleUnitCommand(Vector2 screenPosition)
        {
            // Don't handle commands if we're in the middle of rectangle selection
            if (selectionRectangle != null && selectionRectangle.IsSelecting)
            {
                return;
            }

            var ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetPosition = hit.point;
                
                // Validate target position on NavMesh
                if (useNavMeshValidation && pathfindingSystem != null)
                {
                    targetPosition = pathfindingSystem.GetNearestValidPosition(targetPosition);
                }
                
                var units = selectionManager.SelectedUnits;
                
                if (units.Count == 0) return;
                
                // Check if target is too far
                float distance = Vector3.Distance(units[0].transform.position, targetPosition);
                if (distance > maxCommandDistance)
                {
                    Debug.LogWarning($"Target too far: {distance:F1}m (max: {maxCommandDistance}m)");
                    return;
                }
                
                if (useFormationMovement && units.Count > 1)
                {
                    MoveUnitsInFormation(units, targetPosition);
                }
                else
                {
                    MoveUnitsIndividually(units, targetPosition);
                }
            }
        }

        private void HandleUnitSelection(Vector2 screenPosition)
        {
            // Don't handle single selection if we're currently doing rectangle selection
            if (selectionRectangle != null && selectionRectangle.IsLongPress)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("Skipping single selection - rectangle selection in progress");
                return;
            }

            var ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Try to get Unit component on the hit object or its parent
                var unit = hit.collider.GetComponent<Unit>() ?? hit.collider.GetComponentInParent<Unit>();
                if (unit != null)
                {
                    selectionManager.SelectUnit(unit);
                    if (Debug.isDebugBuild)
                        Debug.Log($"Single unit selected: {unit.name}");
                    return;
                }
            }
            
            // If clicked on empty space, deselect all
            selectionManager.DeselectAll();
            if (Debug.isDebugBuild)
                Debug.Log("Clicked empty space - deselected all units");
        }

        private void MoveUnitsInFormation(List<Unit> units, Vector3 targetPosition)
        {
            List<Vector3> formationPositions = CalculateFormationPositions(units.Count, targetPosition, defaultFormation);
            
            // Validate all positions on NavMesh
            if (useNavMeshValidation && pathfindingSystem != null)
            {
                for (int i = 0; i < formationPositions.Count; i++)
                {
                    formationPositions[i] = pathfindingSystem.GetNearestValidPosition(formationPositions[i]);
                }
            }
            
            // Move units to formation positions
            for (int i = 0; i < units.Count && i < formationPositions.Count; i++)
            {
                units[i].MoveTo(formationPositions[i]);
            }
        }

        private void MoveUnitsIndividually(List<Unit> units, Vector3 targetPosition)
        {
            foreach (var unit in units)
            {
                unit.MoveTo(targetPosition);
            }
        }

        private List<Vector3> CalculateFormationPositions(int unitCount, Vector3 center, FormationType formation)
        {
            switch (formation)
            {
                case FormationType.Grid:
                    return GetGridFormation(center, unitCount, formationSpacing);
                case FormationType.Circle:
                    return GetCircleFormation(center, unitCount, formationSpacing);
                case FormationType.Line:
                    return GetLineFormation(center, unitCount, formationSpacing);
                case FormationType.Wedge:
                    return GetWedgeFormation(center, unitCount, formationSpacing);
                default:
                    return GetGridFormation(center, unitCount, formationSpacing);
            }
        }

        private List<Vector3> GetGridFormation(Vector3 center, int unitCount, float spacing)
        {
            int columns = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
            int rows = Mathf.CeilToInt((float)unitCount / columns);
            List<Vector3> positions = new List<Vector3>();

            int i = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (i >= unitCount) break;
                    float x = (c - (columns - 1) / 2f) * spacing;
                    float z = (r - (rows - 1) / 2f) * spacing;
                    positions.Add(center + new Vector3(x, 0, z));
                    i++;
                }
            }
            return positions;
        }

        private List<Vector3> GetCircleFormation(Vector3 center, int unitCount, float radius)
        {
            List<Vector3> positions = new List<Vector3>();
            
            for (int i = 0; i < unitCount; i++)
            {
                float angle = (2f * Mathf.PI * i) / unitCount;
                float x = center.x + radius * Mathf.Cos(angle);
                float z = center.z + radius * Mathf.Sin(angle);
                positions.Add(new Vector3(x, center.y, z));
            }
            
            return positions;
        }

        private List<Vector3> GetLineFormation(Vector3 center, int unitCount, float spacing)
        {
            List<Vector3> positions = new List<Vector3>();
            
            float totalWidth = (unitCount - 1) * spacing;
            float startX = center.x - totalWidth / 2f;
            
            for (int i = 0; i < unitCount; i++)
            {
                float x = startX + i * spacing;
                positions.Add(new Vector3(x, center.y, center.z));
            }
            
            return positions;
        }

        private List<Vector3> GetWedgeFormation(Vector3 center, int unitCount, float spacing)
        {
            List<Vector3> positions = new List<Vector3>();
            
            if (unitCount == 1)
            {
                positions.Add(center);
                return positions;
            }
            
            // Front unit
            positions.Add(center);
            
            // Side units in wedge pattern
            int sideUnits = (unitCount - 1) / 2;
            for (int i = 1; i <= sideUnits; i++)
            {
                float offset = i * spacing;
                positions.Add(center + new Vector3(-offset, 0, -offset)); // Left
                if (positions.Count < unitCount)
                    positions.Add(center + new Vector3(offset, 0, -offset)); // Right
            }
            
            return positions;
        }

        public void SelectUnitsInRectangle(Vector2 startPosition, Vector2 endPosition)// This is not connected!
        {
            // This method is now handled by SelectionRectangle
            Debug.Log("Rectangle selection is now handled by SelectionRectangle component");
        }

        public void IssueMoveCommand()// This is not connected!
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            HandleUnitCommand(mousePosition);
        }

        public void CancelMoveCommand()// This is not connected!
        {
            // Stop all selected units
            var units = selectionManager.SelectedUnits;
            foreach (var unit in units)
            {
                // Stop the unit (you might need to add a Stop() method to Unit)
                unit.MoveTo(unit.transform.position);
            }
            
            selectionManager.DeselectAll();
        }

        public void SelectUnitAtMousePosition() // This is not connected!
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            HandleUnitSelection(mousePosition);
        }

        /// <summary>
        /// Change formation type for future commands
        /// </summary>
        public void SetFormationType(FormationType formation)// This is not connected!
        {
            defaultFormation = formation;
            Debug.Log($"Formation changed to: {formation}");
        }

        /// <summary>
        /// Toggle formation movement
        /// </summary>
        public void ToggleFormationMovement()// This is not connected!
        {
            useFormationMovement = !useFormationMovement;
            Debug.Log($"Formation movement: {(useFormationMovement ? "ON" : "OFF")}");
        }
    }
}
