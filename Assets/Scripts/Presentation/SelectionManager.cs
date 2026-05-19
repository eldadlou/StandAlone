using System.Collections.Generic;
using MyGame.Core;
using MyGame.Core.Services;
using MyGame.Core.Units;
using MyGame.Core.Events;
using UnityEngine;

namespace MyGame.Presentation
{
    public class SelectionManager : MonoBehaviour
    {
        public List<ISelectableUnit> SelectedUnits { get; private set; } = new List<ISelectableUnit>();

        private void Awake()
        {
            // Register with DependencyContainer
            DependencyContainer.Instance.Register(this);
        }

        // Select a single unit (deselects others)
        public void SelectUnit(ISelectableUnit unit)
        {
            DeselectAll();
            if (unit != null && CanSelect(unit))
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
        public void SelectUnits(List<ISelectableUnit> units)
        {
            DeselectAll();
            foreach (var unit in units)
            {
                if (unit != null && CanSelect(unit) && !SelectedUnits.Contains(unit))
                {
                    SelectedUnits.Add(unit);
                    unit.SetSelected(true);
                }
            }
        }

        // Add a unit to the current selection
        public void AddToSelection(ISelectableUnit unit)
        {
            if (unit != null && CanSelect(unit) && !SelectedUnits.Contains(unit))
            {
                SelectedUnits.Add(unit);
                unit.SetSelected(true);
            }
        }

        // Remove a unit from the current selection
        public void RemoveFromSelection(ISelectableUnit unit)
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
        public void ToggleSelection(ISelectableUnit unit)// TODO: connected to nothing
        {
            if (unit != null && CanSelect(unit))
            {
                if (SelectedUnits.Contains(unit))
                    RemoveFromSelection(unit);
                else
                    AddToSelection(unit);
            }
        }

        private static bool CanSelect(ISelectableUnit unit)
        {
            return unit is IUnit u && SelectionUtility.IsPlayerSelectable(u);
        }
    }
}
