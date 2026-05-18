using MyGame.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using MyGame.Core.Events;


namespace MyGame.Input
{
    /// <summary>
    /// Complete input handler that detects all input and triggers events
    /// Pure input detection - no business logic, just event triggering
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private bool enableDebugLogs = false;
        
        [Header("Mouse Settings")]
        [SerializeField] private bool enableMouseInput = true;
        [SerializeField] private bool enableScrollInput = true;
        
        [Header("Keyboard Settings")]
        [SerializeField] private bool enableKeyboardInput = true;
        [SerializeField] private bool enableGamepadInput = true;

        private MyGame.Presentation.SelectionRectangle selectionRectangle;
        private MyGame.Presentation.SelectionManager selectionManager;
        private bool isLeftMousePressed = false;
        private bool isRightMousePressed = false;
        private MyGame.Core.Units.Unit candidateUnit; // stores unit under initial click

        private bool mouseOverUIThisFrame = false;

        // Input Action References for continuous polling
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction scrollAction;

        private void Awake()
        {
            // Register with DependencyContainer
            DependencyContainer.Instance.Register(this);
            
            if (enableDebugLogs)
                Debug.Log("InputHandler initialized and ready");
        }

        private void Start()
        {
            // Get required references
            selectionRectangle = DependencyContainer.Instance.TryResolve<MyGame.Presentation.SelectionRectangle>();
            selectionManager   = DependencyContainer.Instance.TryResolve<MyGame.Presentation.SelectionManager>();
            
            // Setup input actions for continuous polling
            SetupInputActions();
        }

        private void SetupInputActions()
        {
            // Get the PlayerInput component to access the Input Actions
            var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null && playerInput.actions != null)
            {
                moveAction = playerInput.actions["Move"];
                lookAction = playerInput.actions["Look"];
                scrollAction = playerInput.actions["WheelScroll1"];
                
                if (enableDebugLogs)
                    Debug.Log("Input actions setup complete");
            }
            else
            {
                Debug.LogWarning("PlayerInput component or actions not found! Camera movement may not work.");
            }
        }

        private void Update()
        {
            // Cache UI state once per frame; safe outside of input callbacks
            mouseOverUIThisFrame = UnityEngine.EventSystems.EventSystem.current != null &&
                                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            
            // Poll for continuous camera input
            PollCameraInput();
        }

        private void PollCameraInput()
        {
            if (!enableKeyboardInput) return;
            
            // Poll move input for continuous camera movement
            if (moveAction != null)
            {
                Vector2 moveInput = moveAction.ReadValue<Vector2>();
                if (moveInput.magnitude > 0.1f) // Only trigger if there's meaningful input
                {
                    InputEvents.TriggerCameraMove(moveInput);
                    
                    if (enableDebugLogs)
                        Debug.Log($"Polled Input: Camera Move - {moveInput}");
                }
            }
            
            // Poll look input for continuous camera rotation
            if (lookAction != null)
            {
                Vector2 lookInput = lookAction.ReadValue<Vector2>();
                if (lookInput.magnitude > 0.1f) // Only trigger if there's meaningful input
                {
                    InputEvents.TriggerCameraLook(lookInput);
                    
                    if (enableDebugLogs)
                        Debug.Log($"Polled Input: Camera Look - {lookInput}");
                }
            }
            
            // Poll scroll input for continuous zoom
            if (scrollAction != null && enableScrollInput)
            {
                Vector2 scrollInput = scrollAction.ReadValue<Vector2>();
                if (scrollInput.magnitude > 0.1f) // Only trigger if there's meaningful input
                {
                    InputEvents.TriggerCameraScroll(scrollInput);
                    
                    if (enableDebugLogs)
                        Debug.Log($"Polled Input: Camera Scroll - {scrollInput}");
                }
            }
        }

        #region Camera Movement Input (Event-based - kept for compatibility)

        void OnMove(InputValue value)
        {
            if (!enableKeyboardInput) return;
            
            Vector2 moveInput = value.Get<Vector2>();
            InputEvents.TriggerCameraMove(moveInput);
            
            if (enableDebugLogs)
                Debug.Log($"Event Input: Camera Move - {moveInput}");
        }

        void OnLook(InputValue value)
        {
            if (!enableKeyboardInput) return;
            
            Vector2 lookInput = value.Get<Vector2>();
            InputEvents.TriggerCameraLook(lookInput);
            
            if (enableDebugLogs)
                Debug.Log($"Event Input: Camera Look - {lookInput}");
        }

        void OnWheelScroll(InputValue value)
        {
            if (!enableScrollInput) return;
            
            Vector2 scrollInput = value.Get<Vector2>();
            InputEvents.TriggerCameraScroll(scrollInput);
            
            if (enableDebugLogs)
                Debug.Log($"Event Input: Camera Scroll - {scrollInput}");
        }

        #endregion

        #region Mouse Input

        void OnLeftClick(InputValue value)
        {
            if (!enableMouseInput) return;
            
            // Block input when mouse is over UI
            if (mouseOverUIThisFrame) return;
            
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            
            if (value.isPressed)
            {
                // Mouse button pressed
                isLeftMousePressed = true;

                // Raycast to find potential unit under cursor for quick-click selection
                candidateUnit = null;
                {
                    var ray = Camera.main.ScreenPointToRay(mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        candidateUnit = hit.collider.GetComponent<MyGame.Core.Units.Unit>() ?? hit.collider.GetComponentInParent<MyGame.Core.Units.Unit>();
                    }
                }
                
                // Trigger selection start event
                GameEvents.TriggerSelectionStart(mousePosition);
                
                if (enableDebugLogs)
                    Debug.Log($"Input: Left Click Pressed at {mousePosition}");
            }
            else
            {
                // Mouse button released
                isLeftMousePressed = false;
                
                // Trigger selection end event
                GameEvents.TriggerSelectionEnd(mousePosition);
                
                // Clear candidate unit for next click
                candidateUnit = null;
                
                if (enableDebugLogs)
                    Debug.Log($"Input: Left Click Released at {mousePosition}");
            }
        }

        void OnRightClick(InputValue value)
        {
            if (!enableMouseInput) return;
            
            // Block input when mouse is over UI
            if (mouseOverUIThisFrame) return;
            
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            
            if (value.isPressed)
            {
                // Mouse button pressed
                isRightMousePressed = true;
                
                // Check if rectangle selection is active
                if (selectionRectangle != null && selectionRectangle.IsSelecting)
                {
                    // Cancel rectangle selection
                    GameEvents.TriggerSelectionClear();
                    if (enableDebugLogs)
                        Debug.Log("Input: Right Click - cancelled rectangle selection");
                }
                else
                {
                    // Normal right-click behaviour
                    InputEvents.TriggerRightClick(mousePosition);
                    InputEvents.TriggerUnitCommand(mousePosition);
                }
                
                if (enableDebugLogs)
                    Debug.Log($"Input: Right Click Pressed at {mousePosition}");
            }
            else
            {
                // Mouse button released
                isRightMousePressed = false;
                
                if (enableDebugLogs)
                    Debug.Log($"Input: Right Click Released at {mousePosition}");
            }
        }

        void OnMiddleClick(InputValue value)
        {
            if (!enableMouseInput) return;
            Debug.Log( " clikkk" );
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            InputEvents.TriggerMiddleClick(mousePosition);
            
            if (enableDebugLogs)
                Debug.Log($"Input: Middle Click at {mousePosition}");
        }

        #endregion

        #region Additional Input Methods (for future expansion)

        void OnWheelScroll1(InputValue value)
        {
            // Alternative scroll input (if needed)
            if (!enableScrollInput) return;
   //         Debug.Log($"Input: Wheel Scroll 1 - {value}");
            Vector2 scrollInput = value.Get<Vector2>();
            InputEvents.TriggerCameraScroll(scrollInput);
        }

        void OnWheelMiddleClick1(InputValue value)
        {
            // Alternative middle click (if needed)
            if (!enableMouseInput) return;
     //       Debug.Log($"Input: Wheel Middle Click 1 - {value}");
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            InputEvents.TriggerMiddleClick(mousePosition);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get current mouse position
        /// </summary>
        public Vector2 GetMousePosition()
        {
            return Mouse.current.position.ReadValue();
        }


        // public bool mouseOverUIThisFrame()
        // {
        //     // Use Pointer.current.id so this works correctly inside Input System callbacks
        //     if (UnityEngine.EventSystems.EventSystem.current == null || UnityEngine.InputSystem.Pointer.current == null)
        //         return false;
        //     return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(UnityEngine.InputSystem.Pointer.current.deviceId);
        // }

        /// <summary>
        /// Check if currently in rectangle selection mode
        /// </summary>
        public bool IsInRectangleSelection()
        {
            return selectionRectangle != null && selectionRectangle.IsSelecting;
        }

        /// <summary>
        /// Check if left mouse button is currently pressed
        /// </summary>
        public bool IsLeftMousePressed()
        {
            return isLeftMousePressed;
        }

        /// <summary>
        /// Check if right mouse button is currently pressed
        /// </summary>
        public bool IsRightMousePressed()
        {
            return isRightMousePressed;
        }

        /// <summary>
        /// Enable/disable input categories
        /// </summary>
        public void SetInputEnabled(bool mouse, bool keyboard, bool scroll, bool gamepad)
        {
            enableMouseInput = mouse;
            enableKeyboardInput = keyboard;
            enableScrollInput = scroll;
            enableGamepadInput = gamepad;
        }

        /// <summary>
        /// Get the candidate unit from the last mouse press (for single-click selection)
        /// </summary>
        public MyGame.Core.Units.Unit CandidateUnit => candidateUnit;

        /// <summary>
        /// Toggle debug logs
        /// </summary>
        public void SetDebugLogs(bool enabled)
        {
            enableDebugLogs = enabled;
        }

        #endregion
    }
} 