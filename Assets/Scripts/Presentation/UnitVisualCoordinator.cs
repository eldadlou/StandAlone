using MyGame.Core;
using MyGame.Core.Services;
using UnityEngine;
using MyGame.Core.Units;

namespace MyGame.Presentation
{
    /// <summary>
    /// Coordinates all visual aspects of units without mixing presentation concerns into the Unit class
    /// </summary>
    public class UnitVisualCoordinator : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private UnitVisual unitVisual;
        
        [Header("Destination Marker Settings")]
        [SerializeField] private float destinationMarkerTimeout = 3f;
        
        private Unit unit;
        private bool isInitialized = false;

        private void Awake()
        {
            // Get the Unit component
            unit = GetComponent<Unit>();
            if (unit == null)
            {
                // Debug.LogError($"UnitVisualCoordinator requires a Unit component on {gameObject.name}!");
                return;
            }

            // Get the UnitVisual component
            if (unitVisual == null)
            {
                unitVisual = GetComponent<UnitVisual>();
                // Debug.Log($"Found UnitVisual component: {unitVisual != null}");
            }
        }

        private void Start()
        {
            // Try to initialize if unit data is available
            TryInitialize();
        }

        /// <summary>
        /// Try to initialize the visual coordinator when unit data becomes available
        /// </summary>
        public void TryInitialize()
        {
            if (isInitialized) return;
            
            if (unit == null)
            {
                // Debug.LogError($"Unit component not found on {gameObject.name}!");
                return;
            }

            if (unitVisual == null)
            {
                // Debug.LogError($"UnitVisual component not found on {gameObject.name}!");
                return;
            }

            // Check if unit data is available
            UnitData unitData = unit.GetUnitData();
            if (unitData == null)
            {
                // Debug.LogWarning($"Unit data not available yet for {gameObject.name}, will retry later");
                return;
            }

            // Debug.Log($"Initializing UnitVisual for {gameObject.name}");
            unitVisual.Initialize(unitData);
            isInitialized = true;
            // Debug.Log($"UnitVisualCoordinator initialized successfully for {gameObject.name}");
        }

        /// <summary>
        /// Called by Unit when it moves - handles all visual aspects
        /// </summary>
        public void OnUnitMove(Vector3 destination)
        {
            if (!isInitialized) 
            {
                // Debug.LogWarning($"UnitVisualCoordinator not initialized for {gameObject.name}");
                return;
            }
            
//            Debug.Log($"Unit {gameObject.name} moving to {destination}");
            
            // Handle destination marker
            unitVisual?.ShowDestinationMarker(destination);
            
            // Start the fallback timer to hide marker after timeout
            // This ensures the marker hides even if OnPositionUpdate is not called
            CancelInvoke(nameof(HideDestinationMarkerDelayed));
            Invoke(nameof(HideDestinationMarkerDelayed), destinationMarkerTimeout);
            
            // Could add more visual effects here (dust trails, etc.)
        }

        /// <summary>
        /// Called by Unit when position updates - handles all visual aspects
        /// </summary>
        public void OnPositionUpdate(Vector3 newPosition)
        {
            if (!isInitialized) return;
            
            // Update visual position
            unitVisual?.UpdatePosition(newPosition);
            
            // Check if unit has reached destination and hide marker
            if (unit != null && !unit.IsMoving)
            {
                unitVisual?.HideDestinationMarker();
                CancelInvoke(nameof(HideDestinationMarkerDelayed));
            }
            else
            {
                // Reset the fallback timer - hide marker after timeout regardless
                CancelInvoke(nameof(HideDestinationMarkerDelayed));
                Invoke(nameof(HideDestinationMarkerDelayed), destinationMarkerTimeout);
            }
            
            // Could add more visual effects here (footsteps, etc.)
        }
        
        /// <summary>
        /// Fallback method to hide destination marker after timeout
        /// </summary>
        private void HideDestinationMarkerDelayed()
        {
            unitVisual?.HideDestinationMarker();
        }

        /// <summary>
        /// Called by Unit when selection state changes - handles all visual aspects
        /// </summary>
        public void OnSelectionChanged(bool selected)
        {
            // Try to initialize if not already initialized
            if (!isInitialized)
            {
                TryInitialize();
            }
            
            if (!isInitialized) 
            {
                // Debug.LogWarning($"UnitVisualCoordinator not initialized for {gameObject.name}");
                return;
            }
            
//            Debug.Log($"Unit {gameObject.name} selection changed to: {selected}");
            
            // Update selection visuals
            unitVisual?.SetSelected(selected);
            
            // Trigger additional visual effects
            if (selected)
            {
                DependencyContainer.Instance.TryResolve<ISelectionParticleFeedback>()?.SpawnSelectionEffect(unit);
                DependencyContainer.Instance.TryResolve<ISelectionAudioFeedback>()?.PlaySelectionSound();
            }
        }

        /// <summary>
        /// Called by Unit when it takes damage - handles all visual aspects
        /// </summary>
        public void OnUnitDamaged(float damageAmount)
        {
            if (!isInitialized) return;
            
            // Could add damage flash effects, health bar updates, etc.
            // Debug.Log($"Visual: Unit {unit.Type} took {damageAmount} damage");
        }

        /// <summary>
        /// Called by Unit when it attacks - handles all visual aspects
        /// </summary>
        public void OnUnitAttack(IUnit target)
        {
            if (!isInitialized) return;
            
            // Could add attack animations, muzzle flash, etc.
            // Debug.Log($"Visual: Unit {unit.Type} attacked {target.Type}");
        }

        /// <summary>
        /// Called by Unit when it dies - handles all visual aspects
        /// </summary>
        public void OnUnitDeath()
        {
            if (!isInitialized) return;
            
            // Hide destination marker when unit dies
            CancelInvoke(nameof(HideDestinationMarkerDelayed));
            unitVisual?.HideDestinationMarker();
            
            // Could add death animations, explosion effects, etc.
            // Debug.Log($"Visual: Unit {unit.Type} died");
        }

        /// <summary>
        /// Called by Unit when it uses a skill - handles all visual aspects
        /// </summary>
        public void OnSkillUsed(int skillIndex)
        {
            if (!isInitialized) return;
            
            // Could add skill effects, animations, etc.
            // Debug.Log($"Visual: Unit {unit.Type} used skill {skillIndex}");
        }

        /// <summary>
        /// Called by Unit when it upgrades - handles all visual aspects
        /// </summary>
        public void OnUnitUpgrade()
        {
            if (!isInitialized) return;
            
            // Could add upgrade effects, level up animations, etc.
            // Debug.Log($"Visual: Unit {unit.Type} upgraded");
        }
        
        /// <summary>
        /// Reset visual state when unit is returned to pool
        /// </summary>
        public void ResetVisuals()
        {
            if (!isInitialized) return;
            
            // Reset selection state
            unitVisual?.SetSelected(false);
            
            // Hide destination marker and cancel any pending timeout
            CancelInvoke(nameof(HideDestinationMarkerDelayed));
            unitVisual?.HideDestinationMarker();
            
            // Reset any other visual states
            // Debug.Log($"Visual: Reset visuals for {unit.Type}");
        }
    }
} 