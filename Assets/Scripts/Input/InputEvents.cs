using UnityEngine;
using System;

namespace MyGame.Input
{
    /// <summary>
    /// Centralized input events that decouple input handling from specific actions
    /// </summary>
    public static class InputEvents
    {
        // Mouse events
        public static event Action<Vector2> OnLeftClick;
        public static event Action<Vector2> OnRightClick;
        public static event Action<Vector2> OnMiddleClick;
        
        // Camera movement events
        public static event Action<Vector2> OnCameraMove;
        public static event Action<Vector2> OnCameraLook;
        public static event Action<Vector2> OnCameraScroll;
        
        // Unit selection events
        public static event Action<Vector2> OnUnitSelection;
        public static event Action<Vector2> OnUnitCommand;
        
        // Utility methods to trigger events
        public static void TriggerLeftClick(Vector2 position)
        {
            OnLeftClick?.Invoke(position);
        }
        
        public static void TriggerRightClick(Vector2 position)
        {
            OnRightClick?.Invoke(position);
        }
        
        public static void TriggerMiddleClick(Vector2 position)
        {
            OnMiddleClick?.Invoke(position);
        }
        
        public static void TriggerCameraMove(Vector2 direction)
        {
            OnCameraMove?.Invoke(direction);
        }
        
        public static void TriggerCameraLook(Vector2 lookDelta)
        {
            OnCameraLook?.Invoke(lookDelta);
        }
        
        public static void TriggerCameraScroll(Vector2 scrollDelta)
        {
            OnCameraScroll?.Invoke(scrollDelta);
        }
        
        public static void TriggerUnitSelection(Vector2 screenPosition)
        {
            OnUnitSelection?.Invoke(screenPosition);
        }
        
        public static void TriggerUnitCommand(Vector2 screenPosition)
        {
            OnUnitCommand?.Invoke(screenPosition);
        }
    }
} 