using System;
using UnityEngine;

namespace MyGame.Input
{
    /// <summary>
    /// Input events decoupled from handlers. Camera orbit/zoom use Cinemachine directly.
    /// </summary>
    public static class InputEvents
    {
        public static event Action<Vector2> OnRightClick;
        public static event Action<Vector2> OnCameraMove;
        public static event Action<Vector2> OnUnitSelection;
        public static event Action<Vector2> OnUnitCommand;

        public static void TriggerRightClick(Vector2 position) => OnRightClick?.Invoke(position);
        public static void TriggerCameraMove(Vector2 direction) => OnCameraMove?.Invoke(direction);
        public static void TriggerUnitSelection(Vector2 screenPosition) => OnUnitSelection?.Invoke(screenPosition);
        public static void TriggerUnitCommand(Vector2 screenPosition) => OnUnitCommand?.Invoke(screenPosition);

        public static void ClearAllEvents()
        {
            OnRightClick = null;
            OnCameraMove = null;
            OnUnitSelection = null;
            OnUnitCommand = null;
        }
    }
}
