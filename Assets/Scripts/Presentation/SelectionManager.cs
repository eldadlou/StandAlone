using System.Collections.Generic;
using MyGame.Core;
using MyGame.Core.Services;
using UnityEngine;
using MyGame.Core.Units;
using MyGame.Core.Events;

namespace MyGame.Presentation
{
    public class SelectionManager : MonoBehaviour
    {
        public List<Unit> SelectedUnits { get; private set; } = new List<Unit>();

        private void Awake()
        {
            // Register with DependencyContainer
            DependencyContainer.Instance.Register(this);
        }

        // Select a single unit (deselects others)
        public void SelectUnit(Unit unit)
        {
            DeselectAll();
            if (unit != null)
            {
                SelectedUnits.Add(unit);
                unit.SetSelected(true);
                
                // Trigger selection event
                GameEvents.TriggerUnitSelected(unit);
                
                // UI system handles its own selection effects
                DependencyContainer.Instance.TryResolve<IUnitSelectionPresentation>()?.SetSelectedUnit(unit);
            }
        }


        // Select multiple units (deselects others)
        public void SelectUnits(List<Unit> units)
        {
            DeselectAll();
            foreach (var unit in units)
            {
                if (unit != null && !SelectedUnits.Contains(unit))
                {
                    SelectedUnits.Add(unit);
                    unit.SetSelected(true);
                }
            }
        }

        // Add a unit to the current selection
        public void AddToSelection(Unit unit)
        {
            if (unit != null && !SelectedUnits.Contains(unit))
            {
                SelectedUnits.Add(unit);
                unit.SetSelected(true);
            }
        }

        // Remove a unit from the current selection
        public void RemoveFromSelection(Unit unit)
        {
            if (unit != null && SelectedUnits.Contains(unit))
            {
                SelectedUnits.Remove(unit);
                unit.SetSelected(false);
            }
        }

        // Deselect all units
        public void DeselectAll()
        {
            foreach (var unit in SelectedUnits)
            {
                if (unit != null)
                {
                    unit.SetSelected(false);
                    // Trigger deselection event
                    GameEvents.TriggerUnitDeselected(unit);
                }
            }
            SelectedUnits.Clear();
        }

        // Toggle selection state of a unit
        public void ToggleSelection(Unit unit)// TODO: connected to nothing
        {
            if (unit != null)
            {
                if (SelectedUnits.Contains(unit))
                    RemoveFromSelection(unit);
                else
                    AddToSelection(unit);
            }
        }
    }
}
