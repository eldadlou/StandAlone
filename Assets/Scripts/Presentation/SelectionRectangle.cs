using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using MyGame.Core.Units;
using MyGame.Core;
using MyGame.Game;
using MyGame.Input;
using MyGame.Core.Events;

namespace MyGame.Presentation
{
    /// <summary>
    /// Handles rectangle selection UI and multiple unit selection logic
    /// Uses Unity Input System and integrates with existing input events
    /// </summary>
    public class SelectionRectangle : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private RectTransform selectionRect;
        [SerializeField] private Image selectionImage;
        
        [Header("Selection Settings")]
        [SerializeField] private float longPressThreshold = 0.05f; // seconds - reduced for more responsive selection
        [SerializeField] private float dragThreshold = 5f; // pixels - minimum drag distance to start rectangle selection
        [SerializeField] private float minSelectionSize = 10f; // pixels
        [SerializeField] private Color selectionColor = new Color(0.2f, 0.6f, 1f, 0.3f);
        [SerializeField] private Color borderColor = new Color(0.2f, 0.6f, 1f, 0.8f);
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        private Vector2 startPosition;
        private Vector2 currentPosition;
        private bool isSelecting = false;
        private bool isLongPress = false;
        private float pressStartTime;
        
        private SelectionManager selectionManager;
        private Camera mainCamera;
        private InputHandler inputHandler;

        private void Awake()
        {
            // Get required components
            selectionManager = DependencyContainer.Instance.TryResolve<MyGame.Presentation.SelectionManager>();
            mainCamera = Camera.main;
            
            // Setup UI - always create if not assigned
            if (selectionRect == null || selectionImage == null)
            {
                CreateSelectionUI();
            }
            
            // Ensure UI is properly initialized
            EnsureUISetup();
            
            // Hide selection rectangle initially
            SetSelectionRectVisible(false);
            
            // Register with DependencyContainer
            DependencyContainer.Instance.Register(this);
        }

        private void EnsureUISetup()
        {
            // Make sure we have valid UI components
            if (selectionRect == null)
            {
                Debug.LogWarning("SelectionRect is null, creating default UI");
                CreateSelectionUI();
            }
            
            if (selectionImage == null)
            {
                Debug.LogWarning("SelectionImage is null, creating default UI");
                CreateSelectionUI();
            }
            
            // Set default colors if not set
            if (selectionImage != null && selectionImage.color == Color.clear)
            {
                selectionImage.color = selectionColor;
            }
        }

        private void Start()
        {
            // Find the Canvas in the scene
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                // Set the RectTransform's parent to the Canvas
                selectionRect.SetParent(canvas.transform, false);
            }
            else
            {
                Debug.LogError("No Canvas found in the scene! Selection rectangle won't work properly.");
            }
            
            // Get input handler
            inputHandler = DependencyContainer.Instance.TryResolve<MyGame.Input.InputHandler>();
            
            // Subscribe to events
            GameEvents.OnSelectionStart += HandleSelectionStart;
            GameEvents.OnSelectionEnd += HandleSelectionEnd;
            GameEvents.OnSelectionClear += HandleSelectionClear;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            GameEvents.OnSelectionStart -= HandleSelectionStart;
            GameEvents.OnSelectionEnd -= HandleSelectionEnd;
            GameEvents.OnSelectionClear -= HandleSelectionClear;
        }

        private void CreateSelectionUI()
        {
            // Find or create Canvas for UI elements
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                // Create a Canvas if none exists
                GameObject canvasGO = new GameObject("SelectionCanvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000; // Ensure it's on top
                
                // Add CanvasScaler for proper scaling
                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                // Add GraphicRaycaster for UI interaction
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            
            // Create selection rectangle UI
            GameObject rectGO = new GameObject("SelectionRectangle");
            rectGO.transform.SetParent(canvas.transform, false);
            
            selectionRect = rectGO.AddComponent<RectTransform>();
            selectionImage = rectGO.AddComponent<Image>();
            
            // Setup image with better visual properties
            selectionImage.color = selectionColor;
            selectionImage.type = Image.Type.Sliced;
            
            // Create a simple border effect
            // var borderImage = rectGO.AddComponent<Image>(); // cant add 2 images to the same object// makes exeption line after 
            // borderImage.color = borderColor;
            // borderImage.type = Image.Type.Sliced;
            // borderImage.raycastTarget = false; // Don't block input
            
            // Position in screen space
            selectionRect.anchorMin = Vector2.zero;
            selectionRect.anchorMax = Vector2.zero;
            selectionRect.pivot = Vector2.zero;
            
            // Ensure it's visible
            selectionRect.gameObject.SetActive(false);
            
            if (showDebugInfo)
                Debug.Log("Selection UI created on Canvas");
        }

        private void Update()
        {
            // If selection is active but mouse button is no longer pressed, end selection
            if (isSelecting && !Mouse.current.leftButton.isPressed)
            {
                EndSelection();
                return; // EndSelection resets state; no further processing needed this frame
            }

            // Handle continuous mouse tracking for rectangle selection
            if (isSelecting)
            {
                HandleMouseDrag();
            }
        }



        private void HandleMouseDrag()
        {
            if (!isSelecting) return;
            
            // Get current mouse position
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            float dragDistance = Vector2.Distance(currentMousePos, startPosition);
            
            if (showDebugInfo)
                Debug.Log($"HandleMouseDrag - dragDistance: {dragDistance:F1}px, threshold: {dragThreshold}px, time: {Time.time - pressStartTime:F2}s");
            
            // Check if mouse has moved significantly OR enough time has passed
            if (dragDistance > dragThreshold || (!isLongPress && Time.time - pressStartTime > longPressThreshold))
            {
                currentPosition = currentMousePos;
                
                // Start rectangle selection if not already started
                if (!isLongPress)
                {
                    isLongPress = true;
                    SetSelectionRectVisible(true);
                    
                    if (showDebugInfo)
                        Debug.Log($"Rectangle selection started - drag: {dragDistance:F1}px, time: {Time.time - pressStartTime:F2}s");
                    
                    // Debug UI state
                    if (selectionRect != null)
                    {
                        Debug.Log($"SelectionRect active: {selectionRect.gameObject.activeInHierarchy}, visible: {selectionRect.gameObject.activeSelf}");
                    }
                    else
                    {
                        Debug.LogWarning("SelectionRect is null when trying to show selection!");
                    }
                }
                
                // Update selection rectangle
                UpdateSelectionRect();
            }
        }

        private void HandleSelectionStart(Vector2 screenPosition)
        {
//            Debug.Log($"HandleSelectionStart called with position: {screenPosition}");
            StartSelection(screenPosition);
        }

        public void StartSelection(Vector2 screenPosition)
        {
            // Reset state for new selection
            startPosition = screenPosition;
            currentPosition = screenPosition;
            isSelecting = true;
            isLongPress = false;
            pressStartTime = Time.time;
            
            if (showDebugInfo)
                Debug.Log($"Selection started at {screenPosition} - isSelecting: {isSelecting}");
        }

        private void HandleSelectionEnd(Vector2 screenPosition)
        {
            EndSelection();
        }

        private void HandleSelectionClear()
        {
            ForceEndSelection();
        }

        public void EndSelection()
        {
            if (showDebugInfo)
                Debug.Log($"EndSelection called - isLongPress: {isLongPress}, isSelecting: {isSelecting}");
            
            if (isLongPress)
            {
                // Perform multiple selection
                SelectUnitsInRectangle();
            }
            else
            {
                // Single unit selection - get the unit from InputHandler's cached candidate
                var inputHandler = DependencyContainer.Instance.TryResolve<MyGame.Input.InputHandler>();
                if (inputHandler != null && inputHandler.CandidateUnit != null)
                {
                    var selectionManager = DependencyContainer.Instance.TryResolve<MyGame.Presentation.SelectionManager>();
                    if (selectionManager != null)
                    {
                        selectionManager.SelectUnit(inputHandler.CandidateUnit);
                        if (showDebugInfo)
                            Debug.Log($"Single click - selected unit: {inputHandler.CandidateUnit.name}");
                    }
                }
                else
                {
                    // Clicked on empty ground - deselect all units
                    var selectionManager = DependencyContainer.Instance.TryResolve<MyGame.Presentation.SelectionManager>();
                    if (selectionManager != null)
                    {
                        selectionManager.DeselectAll();
                        if (showDebugInfo)
                            Debug.Log("Single click on empty ground - deselected all units");
                    }
                }
            }
            
            // Reset state
            isSelecting = false;
            isLongPress = false;
            SetSelectionRectVisible(false);
            
            if (showDebugInfo)
                Debug.Log("Selection state reset");
        }

        private void UpdateSelectionRect()
        {
            if (!isLongPress || selectionRect == null) return;
            
            // Calculate rectangle bounds
            float x = Mathf.Min(startPosition.x, currentPosition.x);
            float y = Mathf.Min(startPosition.y, currentPosition.y);
            float width = Mathf.Abs(currentPosition.x - startPosition.x);
            float height = Mathf.Abs(currentPosition.y - startPosition.y);
            
            // Ensure minimum size for visibility
            if (width < 5f) width = 5f;
            if (height < 5f) height = 5f;
            
            // Update rectangle position and size
            selectionRect.anchoredPosition = new Vector2(x, y);
            selectionRect.sizeDelta = new Vector2(width, height);
            
            // Ensure the rectangle is visible and active
            if (!selectionRect.gameObject.activeInHierarchy)
            {
                selectionRect.gameObject.SetActive(true);
            }
            
            if (showDebugInfo)
                Debug.Log($"Selection rect: {x}, {y}, {width}x{height}");
        }

        private void SelectUnitsInRectangle()
        {
            if (selectionManager == null)
            {
                Debug.LogWarning("SelectionManager is null in SelectUnitsInRectangle");
                return;
            }
            
            if (mainCamera == null)
            {
                Debug.LogWarning("MainCamera is null in SelectUnitsInRectangle");
                return;
            }
            
            // Get rectangle bounds in screen space
            Rect screenRect = GetSelectionScreenRect();
            
            if (showDebugInfo)
                Debug.Log($"SelectUnitsInRectangle - screenRect: {screenRect}, minSize: {minSelectionSize}");
            
            // Distance check disabled - allow selection from any distance
            // if (screenRect.width < minSelectionSize || screenRect.height < minSelectionSize)
            // {
            //     if (showDebugInfo)
            //         Debug.Log("Selection too small, ignoring");
            //     return;
            // }
            
            // Get units via event system (decoupled approach)
            List<IUnit> allUnits = GameEvents.GetAllUnits();
            
            if (showDebugInfo)
                Debug.Log($"Event system returned {allUnits.Count} units total");
            
            // Use event-driven unit list (much more efficient)
            List<Unit> selectedUnits = new List<Unit>();
            
            foreach (var unit in allUnits)
            {
                if (unit is Unit concreteUnit)
                {
                    bool inRect = IsUnitInSelectionRect(concreteUnit, screenRect);
                    if (showDebugInfo)
                        Debug.Log($"Unit {concreteUnit.name} in rect: {inRect}");
                    
                    if (inRect)
                    {
                        selectedUnits.Add(concreteUnit);
                    }
                }
            }
            
            // Apply selection
            if (selectedUnits.Count > 0)
            {
                selectionManager.SelectUnits(selectedUnits);
                
                if (showDebugInfo)
                    Debug.Log($"Selected {selectedUnits.Count} units in rectangle (event-driven)");
            }
            else
            {
                // If no units selected, deselect all
                selectionManager.DeselectAll();
                
                if (showDebugInfo)
                    Debug.Log("No units in selection rectangle - deselecting all");
            }
        }

        private Rect GetSelectionScreenRect()
        {
            float x = Mathf.Min(startPosition.x, currentPosition.x);
            float y = Mathf.Min(startPosition.y, currentPosition.y);
            float width = Mathf.Abs(currentPosition.x - startPosition.x);
            float height = Mathf.Abs(currentPosition.y - startPosition.y);
            
            return new Rect(x, y, width, height);
        }

        private bool IsUnitInSelectionRect(Unit unit, Rect screenRect)
        {
            if (unit == null) return false;
            
            // Check if camera is valid
            if (mainCamera == null)
            {
                Debug.LogError("MainCamera is null in IsUnitInSelectionRect!");
                return false;
            }
            
            // Convert unit world position to screen position
            Vector3 unitScreenPos = mainCamera.WorldToScreenPoint(unit.transform.position);
            
            if (showDebugInfo)
                Debug.Log($"Unit {unit.name} - world pos: {unit.transform.position}, screen pos: {unitScreenPos}");
            
            // Check if unit is in front of camera
            if (unitScreenPos.z < 0) 
            {
                if (showDebugInfo)
                    Debug.Log($"Unit {unit.name} is behind camera (z: {unitScreenPos.z})");
                return false;
            }
            
            // Check if unit is within selection rectangle
            bool inRect = screenRect.Contains(unitScreenPos);
            if (showDebugInfo)
                Debug.Log($"Unit {unit.name} in rect {screenRect}: {inRect}");
            
            return inRect;
        }

        private void SetSelectionRectVisible(bool visible)
        {
            if (selectionRect != null)
            {
                selectionRect.gameObject.SetActive(visible);
                
                if (showDebugInfo)
                    Debug.Log($"Selection rectangle visibility set to: {visible}");
            }
            else
            {
                Debug.LogWarning("Cannot set selection rectangle visibility - selectionRect is null!");
            }
        }

        /// <summary>
        /// Get the current selection rectangle bounds
        /// </summary>
        public Rect GetCurrentSelectionRect()
        {
            if (!isLongPress) return Rect.zero;
            return GetSelectionScreenRect();
        }

        /// <summary>
        /// Check if currently selecting
        /// </summary>
        public bool IsSelecting => isSelecting;

        /// <summary>
        /// Check if this is a long press selection
        /// </summary>
        public bool IsLongPress => isLongPress;

        /// <summary>
        /// Set the long press threshold
        /// </summary>
        public void SetLongPressThreshold(float threshold)
        {
            longPressThreshold = threshold;
        }

        /// <summary>
        /// Set the minimum selection size
        /// </summary>
        public void SetMinSelectionSize(float minSize)
        {
            minSelectionSize = minSize;
        }

        /// <summary>
        /// Set the drag threshold for starting rectangle selection
        /// </summary>
        public void SetDragThreshold(float threshold)
        {
            dragThreshold = threshold;
        }

        /// <summary>
        /// Force end selection (useful for cleanup)
        /// </summary>
        public void ForceEndSelection()
        {
            if (isSelecting)
            {
                EndSelection();
            }
        }

        /// <summary>
        /// Check if selection rectangle is currently visible
        /// </summary>
        public bool IsVisible => selectionRect != null && selectionRect.gameObject.activeInHierarchy;
    }
} 