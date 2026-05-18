using UnityEngine;
using UnityEngine.UI;
using System;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Services;

namespace MyGame.Presentation.UI
{
    /// <summary>
    /// Handles UI updates in response to unit events
    /// </summary>
    public class UISystem : MonoBehaviour, IUnitSelectionPresentation
    {
        [Header("Unit Info Panel")]
        public GameObject unitInfoPanel;
        public Text unitNameText;
        public Text unitHealthText;
        public Text unitTypeText;
        public Slider healthBar;

        [Header("Game Status")]
        public Text gameStatusText;
        public Text unitCountText;

        [Header("Notifications")]
        public GameObject notificationPanel;
        public Text notificationText;

        private IUnit selectedUnit;
        private int totalUnits = 0;
        private int unitsAlive = 0;

        private void Awake()
        {
            var container = DependencyContainer.Instance;
            container.Register(this);
            container.RegisterAs<IUnitSelectionPresentation>(this);
        }

        public void SubscribeToUnit(IUnit unit)
        {
            if (unit == null) return;

            Debug.Log($"📱 UISystem: Subscribing to unit {unit.Name} ({unit.Type})");

            unit.OnDeath += HandleUnitDeath;
            unit.OnAttack += HandleUnitAttack;
            unit.OnMove += HandleUnitMove;
            unit.OnAnimationEvent += HandleAnimationEvent;

            totalUnits++;
            unitsAlive++;
            UpdateUnitCount();
            
            Debug.Log($"📱 UISystem: Successfully subscribed to {unit.Name}. Total units: {totalUnits}, Alive: {unitsAlive}");
        }

        public void UnsubscribeFromUnit(IUnit unit)
        {
            if (unit == null) return;

            unit.OnDeath -= HandleUnitDeath;
            unit.OnAttack -= HandleUnitAttack;
            unit.OnMove -= HandleUnitMove;
            unit.OnAnimationEvent -= HandleAnimationEvent;
        }

        public void SetSelectedUnit(IUnit unit)
        {
            selectedUnit = unit;
            UpdateUnitInfoPanel();
        }

        private void HandleUnitDeath(IUnit unit)
        {
            unitsAlive--;
            UpdateUnitCount();

            // Show death notification
            ShowNotification($"{unit.Type} has been destroyed!");

            // Update UI if this was the selected unit
            if (selectedUnit == unit)
            {
                selectedUnit = null;
                UpdateUnitInfoPanel();
            }

            Debug.Log($"📱 UI: Unit {unit.Type} died. Units alive: {unitsAlive}/{totalUnits}");
        }

        private void HandleUnitAttack(IUnit attacker, IUnit target)
        {
            // Add null checks to prevent NullReferenceException
            if (attacker == null)
            {
                Debug.LogWarning("📱 UI: Attack event received with null attacker");
                return;
            }
            
            if (target == null)
            {
                Debug.LogWarning("📱 UI: Attack event received with null target");
                ShowNotification($"{attacker.Type} attacks unknown target!");
                Debug.Log($"📱 UI: {attacker.Type} attacked unknown target");
                return;
            }
            
            ShowNotification($"{attacker.Type} attacks {target.Type}!");
            Debug.Log($"📱 UI: {attacker.Type} attacked {target.Type}");
        }

        private void HandleUnitMove(IUnit unit, Vector3 destination)
        {
            // Update unit info if this is the selected unit
            if (selectedUnit == unit)
            {
                UpdateUnitInfoPanel();
            }
            // Debug.Log($"📱 UI: {unit.Type} moved to {destination}");
        }

        private void HandleAnimationEvent(string eventName)
        {
            switch (eventName.ToLower())
            {
                case "fire":
                    ShowNotification("Unit fired!");
                    break;
                case "reload":
                    ShowNotification("Unit reloading...");
                    break;
                case "upgrade":
                    ShowNotification("Unit upgraded!");
                    break;
            }

            Debug.Log($"📱 UI: Animation event: {eventName}");
        }

        private void UpdateUnitInfoPanel()
        {
            if (unitInfoPanel == null) return;

            if (selectedUnit != null)
            {
                unitInfoPanel.SetActive(true);
                
                if (unitNameText != null)
                    unitNameText.text = $"Unit: {selectedUnit.Type}";
                
                if (unitHealthText != null)
                    unitHealthText.text = $"Health: {selectedUnit.Health:F0}";
                
                if (unitTypeText != null)
                    unitTypeText.text = $"Type: {selectedUnit.Type}";
                
                if (healthBar != null)
                    healthBar.value = selectedUnit.Health / 100f; // Assuming max health is 100
            }
            else
            {
                unitInfoPanel.SetActive(false);
            }
        }

        private void UpdateUnitCount()
        {
            if (unitCountText != null)
                unitCountText.text = $"Units: {unitsAlive}/{totalUnits}";
        }

        private void ShowNotification(string message)
        {
            if (notificationPanel != null && notificationText != null)
            {
                notificationPanel.SetActive(true);
                notificationText.text = message;
                
                // Hide notification after 3 seconds
                Invoke(nameof(HideNotification), 3f);
            }
        }

        private void HideNotification()
        {
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }

        public void UpdateGameStatus(string status)
        {
            if (gameStatusText != null)
                gameStatusText.text = status;
        }
    }
} 