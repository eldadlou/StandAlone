using UnityEngine;
using MyGame.RuntimeSystems.Combat;
using MyGame.RuntimeSystems.Movement;
using MyGame.Presentation.UI;
using MyGame.Core.Events;
using MyGame.Core.Units;
using MyGame.Core.Units.Combat;
using MyGame.RuntimeSystems.Audio;
using MyGame.RuntimeSystems.Effects;
using MyGame.Core.SpatialPartitioning;
using MyGame.Presentation;
using MyGame.Input;

namespace MyGame.Core
{
    /// <summary>
    /// Central system initializer that sets up all dependencies using dependency injection
    /// This replaces the need for FindObjectOfType calls throughout the codebase
    /// </summary>
    public class SystemInitializer : MonoBehaviour
    {
        [Header("System Prefabs")]
        [SerializeField] private GameObject fireSystemPrefab;
        [SerializeField] private GameObject movementSystemPrefab;
        [SerializeField] private GameObject uiSystemPrefab;
        [SerializeField] private GameObject detectionManagerPrefab;
        
        [Header("Auto Setup")]
        [SerializeField] private bool autoSetupOnAwake = true;
        [SerializeField] private bool createMissingSystems = true;
        
        private void Awake()
        {
            // Register self with DependencyContainer so other systems can find us
            DependencyContainer.Instance.Register(this);
            
            if (autoSetupOnAwake)
            {
                InitializeSystems();
            }
        }
        
        /// <summary>
        /// Initialize all systems and register them with the dependency container
        /// </summary>
        public void InitializeSystems()
        {
            // Debug.Log("🚀 Initializing systems with dependency injection...");
            
            // Initialize core systems
            InitializeFireSystem();
            InitializeMovementSystem();
            InitializeUISystem();
            InitializeAudioSystem();
            InitializeParticleSystem();
            InitializeUnitPoolManager();
            InitializeSpatialGrid();
            InitializeDetectionManager();
            InitializeAlliedSupportSystem();
            InitializeSelectionSystems();
            
            // Initialize event system
            InitializeEventSystem();
            
            // Initialize unit registry
            InitializeUnitRegistry();
            
            // Debug.Log("✅ All systems initialized and registered with dependency container");
        }
        
        private void InitializeFireSystem()
        {
            // Try to find existing fire system
            LightweightFireSystem fireSystem = FindObjectOfType<LightweightFireSystem>();
            
            if (fireSystem == null && createMissingSystems)
            {
                // Create fire system if it doesn't exist
                if (fireSystemPrefab != null)
                {
                    GameObject fireSystemGO = Instantiate(fireSystemPrefab);
                    fireSystem = fireSystemGO.GetComponent<LightweightFireSystem>();
                }
                else
                {
                    // Create a default fire system
                    GameObject fireSystemGO = new GameObject("LightweightFireSystem");
                    fireSystem = fireSystemGO.AddComponent<LightweightFireSystem>();
                }
                
                // Debug.Log("🔥 Created LightweightFireSystem");
            }
            
            if (fireSystem != null)
            {
                fireSystem.RegisterWithDependencyContainer();
                // Debug.Log("🔥 LightweightFireSystem registered with dependency container");
            }
        }
        
        private void InitializeMovementSystem()
        {
            // Try to find existing movement system
            MovementSystem movementSystem = FindObjectOfType<MovementSystem>();
            
            if (movementSystem == null && createMissingSystems)
            {
                // Create movement system if it doesn't exist
                if (movementSystemPrefab != null)
                {
                    GameObject movementSystemGO = Instantiate(movementSystemPrefab);
                    movementSystem = movementSystemGO.GetComponent<MovementSystem>();
                }
                else
                {
                    // Create a default movement system
                    GameObject movementSystemGO = new GameObject("MovementSystem");
                    movementSystem = movementSystemGO.AddComponent<MovementSystem>();
                }
                
                // Debug.Log("🚶 Created MovementSystem");
            }
            
            if (movementSystem != null)
            {
                DependencyContainer.Instance.Register(movementSystem);
                // Debug.Log("🚶 MovementSystem registered with dependency container");
            }
        }
        
        private void InitializeUISystem()
        {
            // Try to find existing UI system
            UISystem uiSystem = FindObjectOfType<UISystem>();
            
            if (uiSystem == null && createMissingSystems)
            {
                // Create UI system if it doesn't exist
                if (uiSystemPrefab != null)
                {
                    GameObject uiSystemGO = Instantiate(uiSystemPrefab);
                    uiSystem = uiSystemGO.GetComponent<UISystem>();
                }
                else
                {
                    // Create a default UI system
                    GameObject uiSystemGO = new GameObject("UISystem");
                    uiSystem = uiSystemGO.AddComponent<UISystem>();
                }
                
                // Debug.Log("📱 Created UISystem");
            }
            
            if (uiSystem != null)
            {
                DependencyContainer.Instance.Register(uiSystem);
                // Debug.Log("📱 UISystem registered with dependency container");
            }
        }
        
        private void InitializeAudioSystem()
        {
            // Try to find existing audio system
            AudioSystem audioSystem = FindObjectOfType<AudioSystem>();
            
            if (audioSystem == null && createMissingSystems)
            {
                // Create audio system if it doesn't exist
                GameObject audioSystemGO = new GameObject("AudioSystem");
                audioSystem = audioSystemGO.AddComponent<AudioSystem>();
                // Debug.Log("🔊 Created AudioSystem");
            }
            
            if (audioSystem != null)
            {
                DependencyContainer.Instance.Register(audioSystem);
                // Debug.Log("🔊 AudioSystem registered with dependency container");
            }
        }
        
        private void InitializeParticleSystem()
        {
            // Try to find existing particle system
            UnitParticleSystem particleSystem = FindObjectOfType<UnitParticleSystem>();
            
            if (particleSystem == null && createMissingSystems)
            {
                // Create particle system if it doesn't exist
                GameObject particleSystemGO = new GameObject("UnitParticleSystem");
                particleSystem = particleSystemGO.AddComponent<UnitParticleSystem>();
                // Debug.Log("✨ Created UnitParticleSystem");
            }
            
            if (particleSystem != null)
            {
                // Use manual registration method
                particleSystem.RegisterWithDependencyContainer();
                // Debug.Log("✨ UnitParticleSystem registered with dependency container");
            }
        }
        
        private void InitializeUnitPoolManager()
        {
            // Try to find existing unit pool manager
            UnitPoolManager unitPoolManager = FindObjectOfType<UnitPoolManager>();
            
            if (unitPoolManager == null && createMissingSystems)
            {
                // Create unit pool manager if it doesn't exist
                GameObject unitPoolManagerGO = new GameObject("UnitPoolManager");
                unitPoolManager = unitPoolManagerGO.AddComponent<UnitPoolManager>();
                // Debug.Log("🎒 Created UnitPoolManager");
            }
            
            if (unitPoolManager != null)
            {
                DependencyContainer.Instance.Register(unitPoolManager);
                // Debug.Log("🎒 UnitPoolManager registered with dependency container");
            }
        }
        
        private void InitializeSpatialGrid()
        {
            // Try to find existing spatial grid
            SpatialGrid spatialGrid = FindObjectOfType<SpatialGrid>();
            
            if (spatialGrid == null && createMissingSystems)
            {
                // Create spatial grid if it doesn't exist
                GameObject spatialGridGO = new GameObject("SpatialGrid");
                spatialGrid = spatialGridGO.AddComponent<SpatialGrid>();
                // Debug.Log("🗺️ Created SpatialGrid");
            }
            
            if (spatialGrid != null)
            {
                DependencyContainer.Instance.Register(spatialGrid);
                // Debug.Log("🗺️ SpatialGrid registered with dependency container");
            }
        }
        
        private void InitializeDetectionManager()
        {
            // Try to find existing detection manager
            var detectionManager = FindObjectOfType<MyGame.RuntimeSystems.Combat.CentralizedDetectionManager>();
            
            if (detectionManager == null && createMissingSystems)
            {
                // Create detection manager if it doesn't exist
                if (detectionManagerPrefab != null)
                {
                    GameObject detectionManagerGO = Instantiate(detectionManagerPrefab);
                    detectionManager = detectionManagerGO.GetComponent<MyGame.RuntimeSystems.Combat.CentralizedDetectionManager>();
                }
                else
                {
                    // Create a default detection manager
                    GameObject detectionManagerGO = new GameObject("CentralizedDetectionManager");
                    detectionManager = detectionManagerGO.AddComponent<MyGame.RuntimeSystems.Combat.CentralizedDetectionManager>();
                }
                
                // Debug.Log("🔍 Created CentralizedDetectionManager");
            }
            
            if (detectionManager != null)
            {
                DependencyContainer.Instance.Register(detectionManager);
                // Debug.Log("🔍 CentralizedDetectionManager registered with dependency container");
            }
        }
        
        private void InitializeAlliedSupportSystem()
        {
            // Try to find existing allied support system
            AlliedSupportSystem supportSystem = FindObjectOfType<AlliedSupportSystem>();
            
            if (supportSystem == null && createMissingSystems)
            {
                // Create allied support system if it doesn't exist
                GameObject supportSystemGO = new GameObject("AlliedSupportSystem");
                supportSystem = supportSystemGO.AddComponent<AlliedSupportSystem>();
                // Debug.Log("🤝 Created AlliedSupportSystem");
            }
            
            if (supportSystem != null)
            {
                DependencyContainer.Instance.Register(supportSystem);
                // Debug.Log("🤝 AlliedSupportSystem registered with dependency container");
            }
        }
        
        private void InitializeEventSystem()
        {
            // Subscribe all systems to unit creation events
            GameEvents.OnUnitCreated += SubscribeSystemsToUnit;
            // Debug.Log("📡 Event system initialized with automatic unit subscription");
            
            // Handle existing units that were created before system initialization
            SubscribeToExistingUnits();
        }
        
        /// <summary>
        /// Subscribe systems to units that already exist in the scene
        /// This handles the case where units are created before SystemInitializer runs
        /// </summary>
        private void SubscribeToExistingUnits()
        {
            // Find all existing units in the scene
            Unit[] existingUnits = FindObjectsOfType<Unit>();
            // Debug.Log($"🔗 SystemInitializer: Found {existingUnits.Length} existing units to subscribe to");
            
            foreach (var unit in existingUnits)
            {
                if (unit != null)
                {
                    SubscribeSystemsToUnit(unit);
                }
            }
        }
        
        /// <summary>
        /// Automatically subscribe all systems to a newly created unit
        /// </summary>
        private void SubscribeSystemsToUnit(IUnit unit)
        {
            if (unit == null) return;
            
            // Debug.Log($"🔗 SystemInitializer: Auto-subscribing systems to unit {unit.Name}");
            
            // Get all systems that can subscribe to units
            var uiSystem = GetSystem<UISystem>();
            var audioSystem = GetSystem<AudioSystem>();
            var particleSystem = GetSystem<UnitParticleSystem>();
            var fireSystem = GetSystem<ICombatFireCoordinator>();
            
            // Debug.Log($"🔗 SystemInitializer: Systems found - UISystem: {uiSystem != null}, AudioSystem: {audioSystem != null}, UnitParticleSystem: {particleSystem != null}, LightweightFireSystem: {fireSystem != null}");
            
            // LightweightFireSystem works differently - it registers combat units, not subscribes to events
            if (fireSystem != null && unit is ICombatUnit combatUnit)
            {
                fireSystem.RegisterCombatUnit(combatUnit);
                // Debug.Log($"🔗 LightweightFireSystem registered combat unit {unit.Name}");
            }
            
            // Subscribe each system to the unit
            if (uiSystem != null)
            {
                uiSystem.SubscribeToUnit(unit);
                // Debug.Log($"🔗 UISystem subscribed to {unit.Name}");
            }
            
            if (audioSystem != null)
            {
                audioSystem.SubscribeToUnit(unit);
                // Debug.Log($"🔗 AudioSystem subscribed to {unit.Name}");
            }
            
            if (particleSystem != null)
            {
                particleSystem.SubscribeToUnit(unit);
                // Debug.Log($"🔗 UnitParticleSystem subscribed to {unit.Name}");
            }
            
            // Debug.Log($"🔗 SystemInitializer: {unit.Name} now has event subscribers");
        }
        
        private void InitializeUnitRegistry()
        {
            // Clear any existing unit registry
            Unit.ClearRegistry();
            // Debug.Log("📋 Unit registry initialized");
        }

        private void InitializeSelectionSystems()
        {
            if (!createMissingSystems)
                return;

            var selectionManager = FindFirstObjectByType<SelectionManager>();
            var selectionRectangle = FindFirstObjectByType<SelectionRectangle>();
            var inputHandler = FindFirstObjectByType<InputHandler>();
            var commandSystem = FindFirstObjectByType<CommandSystem>();

            if (selectionManager != null && selectionRectangle != null && inputHandler != null && commandSystem != null)
                return;

            var go = GameObject.Find("SelectionSystems") ?? new GameObject("SelectionSystems");

            if (selectionManager == null)
                go.AddComponent<SelectionManager>();
            if (selectionRectangle == null)
                go.AddComponent<SelectionRectangle>();
            if (inputHandler == null)
                go.AddComponent<InputHandler>();
            if (commandSystem == null)
                go.AddComponent<CommandSystem>();
        }
        
        /// <summary>
        /// Get a system from the dependency container
        /// </summary>
        public static T GetSystem<T>() where T : class
        {
            return DependencyContainer.Instance.TryResolve<T>();
        }
        
        /// <summary>
        /// Check if a system is available
        /// </summary>
        public static bool HasSystem<T>() where T : class
        {
            return DependencyContainer.Instance.IsRegistered<T>();
        }
        
        /// <summary>
        /// Manually register a system (useful for testing or custom setup)
        /// </summary>
        public static void RegisterSystem<T>(T system) where T : class
        {
            DependencyContainer.Instance.Register(system);
            // Debug.Log($"📦 Manually registered system: {typeof(T).Name}");
        }
        
        /// <summary>
        /// Clear all systems (useful for testing)
        /// </summary>
        public static void ClearAllSystems()
        {
            DependencyContainer.Instance.Clear();
            Unit.ClearRegistry();
            // Debug.Log("🧹 All systems cleared");
        }
    }
}
