using System;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Core;
using MyGame.Core.Events;
using MyGame.Core.Units;
using MyGame.Presentation.UI;

namespace MyGame.Game
{
    /// <summary>
    /// Main game manager that coordinates all game systems
    /// Now uses dependency injection for better scalability
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private bool autoInitializeSystems = true;
        
        // Singleton instance
        public static GameManager Instance { get; private set; }
        
        // Players
        public Player Player1 { get; private set; }
        public Player AI { get; private set; }
        
        // Systems (accessed via dependency injection)
        private SystemInitializer systemInitializer;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeGame()
        {
            // Debug.Log("🎮 GameManager: Initializing game...");
            
            // Initialize players
            InitializePlayers();
            
            // Initialize systems using dependency injection
            if (autoInitializeSystems)
            {
                InitializeSystems();
            }
            
            // Debug.Log("✅ GameManager: Game initialized successfully");
        }
        
        private void InitializePlayers()
        {
            // Create players
            Player1 = new Player("Player 1", Team.Player);
            AI = new Player("AI", Team.AI);
            
            // Debug.Log($"👥 GameManager: Players initialized - {Player1.Name} vs {AI.Name}");
        }
        
        private void InitializeSystems()
        {
            // Get SystemInitializer from DependencyContainer, fallback to FindObjectOfType
            systemInitializer = DependencyContainer.Instance.TryResolve<SystemInitializer>();
            if (systemInitializer == null)
            {
                systemInitializer = FindObjectOfType<SystemInitializer>();
            }
            if (systemInitializer == null)
            {
                // Create SystemInitializer if it doesn't exist
                GameObject systemManager = new GameObject("SystemManager");
                systemInitializer = systemManager.AddComponent<SystemInitializer>();
                // Debug.Log("🔧 GameManager: Created SystemInitializer");
            }
            
            // Initialize all systems
            systemInitializer.InitializeSystems();
            
            // Verify systems are available
            //VerifySystems();
        }
        
        private void VerifySystems()
        {
            // Debug.Log("🔍 GameManager: Verifying systems...");
            //
            // // Check if all required systems are available
            // var fireSystem = SystemInitializer.GetSystem<MyGame.RuntimeSystems.Combat.LightweightFireSystem>();
            // var movementSystem = SystemInitializer.GetSystem<MyGame.RuntimeSystems.Movement.MovementSystem>();
            // var uiSystem = SystemInitializer.GetSystem<UISystem>();
            //
            // if (fireSystem != null)
            // {
            //     // Debug.Log("✅ FireSystem: Available");
            //     // Subscribe to attack events
            //     fireSystem.OnUnitAttack += HandleUnitAttack;
            //     // Debug.Log("🔗 GameManager: Subscribed to FireSystem attack events");
            // }
            // else
            //     // Debug.LogWarning("⚠️ FireSystem: Not found");
            //     
            // if (movementSystem != null)
            //     // Debug.Log("✅ MovementSystem: Available");
            //     ;
            // else
            //     // Debug.LogWarning("⚠️ MovementSystem: Not found");
            //     
            // if (uiSystem != null)
            //     // Debug.Log("✅ UISystem: Available");
            //     ;
            // else
            //     // Debug.LogWarning("⚠️ UISystem: Not found");
        }
        
        private void HandleUnitAttack(IUnit attacker, IUnit target)
        {
            // Debug.Log($"🎯 GameManager: Attack event - {attacker.Name} attacked {target.Name}");
            
            // Notify other systems about the attack
            GameEvents.TriggerUnitAttack(attacker, target);
        }
        
        /// <summary>
        /// Get a system using dependency injection
        /// </summary>
        public static T GetSystem<T>() where T : class
        {
            return SystemInitializer.GetSystem<T>();
        }
        
        /// <summary>
        /// Check if a system is available
        /// </summary>
        public static bool HasSystem<T>() where T : class
        {
            return SystemInitializer.HasSystem<T>();
        }
        
        /// <summary>
        /// Manually register a system
        /// </summary>
        public static void RegisterSystem<T>(T system) where T : class
        {
            SystemInitializer.RegisterSystem(system);
        }
        
        /// <summary>
        /// Clear all systems (useful for testing)
        /// </summary>
        public static void ClearAllSystems()
        {
            SystemInitializer.ClearAllSystems();
        }
        
        /// <summary>
        /// Restart the game (clear and reinitialize)
        /// </summary>
        public void RestartGame()
        {
            // Debug.Log("🔄 GameManager: Restarting game...");
            
            // Clear all systems
            ClearAllSystems();
            
            // Reinitialize systems
            if (systemInitializer != null)
            {
                systemInitializer.InitializeSystems();
            }
            
            // Debug.Log("✅ GameManager: Game restarted");
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
