using UnityEngine;
using MyGame.Core;

namespace MyGame.Core
{
    /// <summary>
    /// Validates that all required systems are present in the scene
    /// Attach this to a GameObject in your startup scene
    /// </summary>
    public class SystemValidator : MonoBehaviour
    {
        [Header("Validation Settings")]
        public bool validateOnStart = true;
        public bool logSuccess = true;

        private void Start()
        {
            if (validateOnStart)
                ValidateSystems();
        }

        [ContextMenu("Validate Systems")]
        public void ValidateSystems()
        {
            bool isValid = ValidateDependencyContainer();
            
            if (isValid && logSuccess)
            {
                Debug.Log("✅ All systems validated successfully!");
            }
            else if (!isValid)
            {
                Debug.LogError("❌ System validation failed! Check the console for missing systems.");
            }
        }

        [ContextMenu("Clear All References")]
        public void ClearAllReferences()
        {
            DependencyContainer.Instance.Clear();
            Debug.Log("🗑️ All service references cleared.");
        }

        private bool ValidateDependencyContainer()
        {
            var container = DependencyContainer.Instance;
            
            // Check if all required systems are registered
            bool allValid = true;
            
            if (!container.IsRegistered<MyGame.RuntimeSystems.Movement.MovementSystem>())
            {
                Debug.LogError("MovementSystem not found in DependencyContainer!");
                allValid = false;
            }
            
            if (!container.IsRegistered<MyGame.RuntimeSystems.Movement.PathfindingSystem>())
            {
                Debug.LogError("PathfindingSystem not found in DependencyContainer!");
                allValid = false;
            }
            
            if (!container.IsRegistered<MyGame.RuntimeSystems.Combat.FireSystem>())
            {
                Debug.LogError("FireSystem not found in DependencyContainer!");
                allValid = false;
            }
            
            if (!container.IsRegistered<MyGame.RuntimeSystems.Combat.ExplosionSystem>())
            {
                Debug.LogError("ExplosionSystem not found in DependencyContainer!");
                allValid = false;
            }
            
            if (!container.IsRegistered<MyGame.Game.GameManager>())
            {
                Debug.LogError("GameManager not found in DependencyContainer!");
                allValid = false;
            }
            
            if (!container.IsRegistered<MyGame.Input.CommandSystem>())
            {
                Debug.LogError("CommandSystem not found in DependencyContainer!");
                allValid = false;
            }
            
            if (!container.IsRegistered<MyGame.Input.InputHandler>())
            {
                Debug.LogError("InputHandler not found in DependencyContainer!");
                allValid = false;
            }
            
            if (!container.IsRegistered<MyGame.RuntimeSystems.Audio.AudioSystem>())
            {
                Debug.LogError("AudioSystem not found in DependencyContainer!");
                allValid = false;
            }
            
            if (!container.IsRegistered<MyGame.RuntimeSystems.Effects.UnitParticleSystem>())
            {
                Debug.LogError("UnitParticleSystem not found in DependencyContainer!");
                allValid = false;
            }
            
            if (!container.IsRegistered<MyGame.Presentation.UI.UISystem>())
            {
                Debug.LogError("UISystem not found in DependencyContainer!");
                allValid = false;
            }
            
            if (!container.IsRegistered<MyGame.Presentation.SelectionManager>())
            {
                Debug.LogError("SelectionManager not found in DependencyContainer!");
                allValid = false;
            }
            
            if (!container.IsRegistered<MyGame.Presentation.SelectionRectangle>())
            {
                Debug.LogError("SelectionRectangle not found in DependencyContainer!");
                allValid = false;
            }
            
            return allValid;
        }
    }
} 