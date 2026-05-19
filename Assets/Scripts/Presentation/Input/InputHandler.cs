using MyGame.Core;
using MyGame.Core.Services;
using MyGame.Core.Units;
using MyGame.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using MyGame.Core.Events;

namespace MyGame.Input
{
    /// <summary>
    /// Input detection for selection, commands, and WASD camera pan.
    /// Orbit and zoom are handled by Cinemachine Input Axis Controller on the vcam.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private bool enableDebugLogs;
        [SerializeField] private bool enableMouseInput = true;
        [SerializeField] private bool enableKeyboardInput = true;

        private SelectionRectangle selectionRectangle;
        private bool isLeftMousePressed;
        private bool isRightMousePressed;
        private ISelectableUnit candidateUnit;
        private bool mouseOverUIThisFrame;
        private InputAction moveAction;

        private void Awake()
        {
            DependencyContainer.Instance.Register(this);
        }

        private void Start()
        {
            selectionRectangle = DependencyContainer.Instance.TryResolve<SelectionRectangle>();

            var playerInput = GetComponent<PlayerInput>();
            if (playerInput?.actions != null)
                moveAction = playerInput.actions["Move"];
            else
                Debug.LogWarning("PlayerInput or actions missing on InputSystem. WASD pan will not work.");
        }

        private void Update()
        {
            mouseOverUIThisFrame = EventSystem.current != null &&
                                   EventSystem.current.IsPointerOverGameObject();

            PollCameraPan();
            PollMouseSelection();
            PollMouseCommands();
        }

        private void PollCameraPan()
        {
            if (!enableKeyboardInput || moveAction == null)
            {
                InputEvents.TriggerCameraMove(Vector2.zero);
                return;
            }

            // Always send current value (including zero on release) so pan stops when keys are up.
            InputEvents.TriggerCameraMove(moveAction.ReadValue<Vector2>());
        }

        private void PollMouseSelection()
        {
            if (!enableMouseInput || Mouse.current == null || mouseOverUIThisFrame)
                return;

            var mousePosition = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame && !isLeftMousePressed)
                HandleLeftPress(mousePosition);

            if (Mouse.current.leftButton.wasReleasedThisFrame && isLeftMousePressed)
                HandleLeftRelease(mousePosition);
        }

        private void PollMouseCommands()
        {
            if (!enableMouseInput || Mouse.current == null || mouseOverUIThisFrame)
                return;

            var mousePosition = Mouse.current.position.ReadValue();

            if (Mouse.current.rightButton.wasPressedThisFrame && !isRightMousePressed)
                HandleRightPress(mousePosition);

            if (Mouse.current.rightButton.wasReleasedThisFrame && isRightMousePressed)
                isRightMousePressed = false;
        }

        void OnLeftClick(InputValue value)
        {
            if (!enableMouseInput || Mouse.current == null || mouseOverUIThisFrame)
                return;

            var mousePosition = Mouse.current.position.ReadValue();
            if (value.isPressed && !isLeftMousePressed)
                HandleLeftPress(mousePosition);
            else if (!value.isPressed && isLeftMousePressed)
                HandleLeftRelease(mousePosition);
        }

        private void HandleLeftPress(Vector2 mousePosition)
        {
            isLeftMousePressed = true;
            candidateUnit = null;
            SelectionUtility.TryGetSelectableAtScreen(mousePosition, out candidateUnit);
            GameEvents.TriggerSelectionStart(mousePosition);
        }

        private void HandleLeftRelease(Vector2 mousePosition)
        {
            isLeftMousePressed = false;
            GameEvents.TriggerSelectionEnd(mousePosition);
            candidateUnit = null;
        }

        void OnRightClick(InputValue value)
        {
            if (!enableMouseInput || Mouse.current == null || mouseOverUIThisFrame)
                return;

            var mousePosition = Mouse.current.position.ReadValue();
            if (value.isPressed && !isRightMousePressed)
                HandleRightPress(mousePosition);
            else if (!value.isPressed && isRightMousePressed)
                isRightMousePressed = false;
        }

        private void HandleRightPress(Vector2 mousePosition)
        {
            isRightMousePressed = true;

            if (selectionRectangle != null && selectionRectangle.IsSelecting)
            {
                GameEvents.TriggerSelectionClear();
                return;
            }

            InputEvents.TriggerRightClick(mousePosition);
            InputEvents.TriggerUnitCommand(mousePosition);
        }

        public Vector2 GetMousePosition() => Mouse.current.position.ReadValue();

        public bool IsInRectangleSelection() =>
            selectionRectangle != null && selectionRectangle.IsSelecting;

        public bool IsLeftMousePressed() => isLeftMousePressed;
        public bool IsRightMousePressed() => isRightMousePressed;
        public ISelectableUnit CandidateUnit => candidateUnit;
    }
}
