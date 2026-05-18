using MyGame.Core.Units;
using UnityEngine;

namespace MyGame.Core.Configuration
{
    /// <summary>
    /// Centralized game configuration system
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MyGame/Game Configuration")]
    public class GameConfig : ScriptableObject
    {
        [Header("Movement Settings")]
        public float defaultUnitSpeed = 5f;
        public float stoppingDistance = 0.5f;
        public float rotationSpeed = 5f;
        public float formationSpacing = 2f;
        public float separationRadius = 1.5f;
        public float separationStrength = 2f;
        public float maxCommandDistance = 100f;
        
        [Header("Combat Settings")]
        public float defaultAttackDamage = 15f;
        public float defaultAttackRange = 10f;
        public float defaultAttackCooldown = 2f;
        public float projectileSpeed = 20f;
        public bool enableProjectiles = true;
        
        [Header("Unit Settings")]
        public float defaultHealth = 100f;
        public float tankHealth = 150f;
        public float soldierHealth = 80f;
        public float aircraftHealth = 60f;
        public float helicopterHealth = 90f;
        public float truckHealth = 120f;
        
        [Header("Spatial Grid Settings")]
        public float cellSize = 10f;
        public Vector2 gridSize = new Vector2(1000f, 1000f);
        
        [Header("Object Pool Settings")]
        public int projectilePoolSize = 50;
        public int explosionPoolSize = 20;
        public int effectPoolSize = 30;
        
        [Header("Performance Settings")]
        public int maxUnits = 1000;
        public int maxUndoSteps = 50;
        public bool enableSpatialPartitioning = true;
        public bool enableObjectPooling = true;
        
        [Header("UI Settings")]
        public float notificationDuration = 3f;
        public bool showDebugInfo = false;
        
        [Header("Audio Settings")]
        public float masterVolume = 1f;
        public float sfxVolume = 0.8f;
        public float musicVolume = 0.6f;
        
        // Singleton pattern
        private static GameConfig _instance;
        public static GameConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameConfig>("GameConfig");
                    if (_instance == null)
                    {
                        Debug.LogError("GameConfig not found in Resources folder! Create one at Assets/Resources/GameConfig.asset");
                        _instance = CreateInstance<GameConfig>();
                    }
                }
                return _instance;
            }
        }
        
        private void OnEnable()
        {
            _instance = this;
        }
        
        /// <summary>
        /// Get unit health based on unit type
        /// </summary>
        public float GetUnitHealth(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Tank => tankHealth,
                UnitType.Soldier => soldierHealth,
                UnitType.Aircraft => aircraftHealth,
                UnitType.Helicopter => helicopterHealth,
                UnitType.Truck => truckHealth,
                _ => defaultHealth
            };
        }
        
        /// <summary>
        /// Get unit speed based on unit type
        /// </summary>
        public float GetUnitSpeed(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Tank => defaultUnitSpeed * 0.8f,
                UnitType.Soldier => defaultUnitSpeed,
                UnitType.Aircraft => defaultUnitSpeed * 2f,
                UnitType.Helicopter => defaultUnitSpeed * 1.5f,
                UnitType.Truck => defaultUnitSpeed * 0.6f,
                _ => defaultUnitSpeed
            };
        }
        
        /// <summary>
        /// Get attack damage based on unit type
        /// </summary>
        public float GetAttackDamage(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Tank => defaultAttackDamage * 1.5f,
                UnitType.Soldier => defaultAttackDamage,
                UnitType.Aircraft => defaultAttackDamage * 2f,
                UnitType.Helicopter => defaultAttackDamage * 1.3f,
                UnitType.Truck => defaultAttackDamage * 0.3f,
                _ => defaultAttackDamage
            };
        }
        
        /// <summary>
        /// Get attack range based on unit type
        /// </summary>
        public float GetAttackRange(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Tank => defaultAttackRange * 1.5f,
                UnitType.Soldier => defaultAttackRange,
                UnitType.Aircraft => defaultAttackRange * 2f,
                UnitType.Helicopter => defaultAttackRange * 1.8f,
                UnitType.Truck => defaultAttackRange * 0.5f,
                _ => defaultAttackRange
            };
        }
        
        /// <summary>
        /// Get attack cooldown based on unit type
        /// </summary>
        public float GetAttackCooldown(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Tank => defaultAttackCooldown * 1.2f,
                UnitType.Soldier => defaultAttackCooldown * 0.8f,
                UnitType.Aircraft => defaultAttackCooldown * 1.5f,
                UnitType.Helicopter => defaultAttackCooldown * 1.3f,
                UnitType.Truck => defaultAttackCooldown * 2f,
                _ => defaultAttackCooldown
            };
        }
    }
}
