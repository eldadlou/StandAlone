using System;
using UnityEngine;
using System.Collections.Generic;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Presentation;
using MyGame.Game;
using MyGame.Core.Services;
using MyGame.RuntimeSystems.Combat;
using Random = UnityEngine.Random;

namespace MyGame.Core.Units.Combat
{
    /// <summary>
    /// Base component for units with combat capabilities
    /// Each unit manages its own combat behavior independently
    /// </summary>
    public abstract class CombatUnit : MonoBehaviour, ICombatUnit, IAccurateUnit
    {
        [Header("Combat Settings")]
        [SerializeField] protected float detectionRadius = 15f;
        [SerializeField] protected LayerMask enemyLayerMask = -1;
        [SerializeField] protected float gunRotationSpeed = 90f;
        [SerializeField] protected float rotationThreshold = 5f;
        [SerializeField] protected float targetUpdateInterval = 0.2f; // Reduced from 0.5f for faster target acquisition
        
        [Header("Projectile Settings")]
        // Note: Projectile settings are handled by derived classes (e.g., VehicleCombatUnit)
        // Base class does not manage projectiles directly
        
        [Header("Base Accuracy Settings")]
        [SerializeField] protected float baseUnitAccuracy = 80f; // Base accuracy percentage
        [SerializeField] protected float baseMovingAccuracyPenalty = 20f; // Accuracy penalty when moving
        [SerializeField] protected float baseMaxAccuracyPenalty = 40f; // Maximum accuracy penalty
        [SerializeField] protected float basePartialHitDamageMultiplier = 0.6f; // Damage multiplier for partial hits
        [SerializeField] protected float basePartialHitChance = 25f; // Chance of partial hit when accuracy check fails
        
        [Header("Combat State")]
        [SerializeField] protected bool isInCombat = false;
        [SerializeField] protected IUnit currentTarget;
        [SerializeField] protected bool isTargetInRange = false;

        
        // Components
        protected GunTurret gunTurret;
        protected IUnit unitInterface;
        protected float lastTargetUpdateTime;
        protected float lastAttackTime;
        
        // Target persistence - prevent constant switching
        protected float targetStickTime = 2f; // How long to stick with a target
        protected float lastTargetChangeTime;
        
        // Performance optimization - cached references
        private ICentralizedCombatDetection _cachedDetectionManager;
        private bool _detectionManagerCached = false;
        private float lastCombatUpdateTime;
        private const float COMBAT_UPDATE_INTERVAL = 0.05f; // 20 times per second instead of 60
        
        // Properties
        public bool IsInCombat => isInCombat;
        public IUnit CurrentTarget => currentTarget;
        public bool IsTargetInRange => isTargetInRange;
        public bool IsGunFacingTarget => IsGunFacingCurrentTarget();
        private bool isGunFacingTarget;
        public float DetectionRadius => detectionRadius;
        public LayerMask EnemyLayerMask => enemyLayerMask;
        public float GunRotationSpeed => gunRotationSpeed;
        public float RotationThreshold => rotationThreshold;
        
        // Abstract properties that derived classes must implement
        public abstract float AttackDamage { get; }
        public abstract float AttackRange { get; }
        public abstract float AttackCooldown { get; }
        public abstract float Health { get; }
        public abstract Vector3 Position { get; }
        public abstract bool CanAttack(IUnit target);
        public abstract bool Attack(IUnit target);
        
        // Vehicle identification
        public abstract string Name { get; }  // Vehicle type name (e.g., "Big Tank", "Small Tank")
        
        // IUnit interface properties - delegate to derived class or provide defaults
        public abstract UnitType Type { get; }
        public abstract Player Owner { get; }
        public abstract float LastAttackTime { get; }
        
        // IUnit interface methods - delegate to derived class or provide defaults
        public abstract void Upgrade();
        public abstract void AssignToTeam(Team team);
        
        // IAttackable interface - delegate to derived class
        public abstract void TakeDamage(float amount);
        
        // IMovable interface - delegate to derived class
        public abstract float Speed { get; }
        public abstract bool IsMoving { get; }
        public abstract Vector3 Destination { get; }
        public abstract void MoveTo(Vector3 destination);
        public abstract void UpdatePosition(Vector3 newPosition);
        
        // ISkillUser interface - delegate to derived class
        public abstract void UseSkill(int skillIndex);
        public abstract List<MyGame.Core.Skills.Skill> Skills { get; }
        
        // IAnimatable interface - delegate to derived class
        public abstract void PlayAnimation(string animationName);
        public abstract event Action<string> OnAnimationEvent;
        
        // IUnit events - delegate to derived class
        public abstract event Action<IUnit> OnDeath;
        public abstract event Action<IUnit, IUnit> OnAttack;
        public abstract event Action<IUnit, Vector3> OnMove;
        
        protected virtual void Awake()
        {
            // Debug.Log($"{Name}: CombatUnit Awake called");
            
            unitInterface ??= GetComponent<IUnit>();
            gunTurret ??= GetComponentInChildren<GunTurret>();
            
            // Debug.Log($"{Name}: unitInterface found: {unitInterface != null}, gunTurret found: {gunTurret != null}");
            
            if (gunTurret == null)
            {
                Debug.LogWarning($"CombatUnit {gameObject.name} has no GunTurret component!");
            }
            
            // Initialize target persistence to a time in the past
            // This allows immediate target acquisition for new units
            // Without this, units would wait targetStickTime (2s) before acquiring their first target
            lastTargetChangeTime = -targetStickTime - 1f;
        }
        
        protected virtual void Start()
        {
            // Register with centralized systems after all systems are initialized
            StartCoroutine(RegisterWithSystemsDelayed());
        }
        
        private System.Collections.IEnumerator RegisterWithSystemsDelayed()
        {
            // Wait for systems to initialize
            yield return new WaitForSeconds(0.1f);
            RegisterWithFireSystem();
            RegisterWithDetectionManager();
            
            // Cache detection manager reference to avoid GetSystem calls every frame
            _cachedDetectionManager = SystemInitializer.GetSystem<ICentralizedCombatDetection>();
            _detectionManagerCached = true;
        }
        
        private System.Collections.IEnumerator RegisterWithFireSystemDelayed()
        {
            // Wait for systems to initialize
            yield return new WaitForSeconds(0.1f);
            RegisterWithFireSystem();
        }
        
        protected virtual void Update()
        {
            // Only log occasionally to avoid spam
            if (Time.frameCount % 60 == 0) // Log every 60 frames (about once per second)
            {
                // Debug.Log($"{Name}: CombatUnit Update called - isInCombat: {isInCombat}, hasTarget: {currentTarget != null}, team: {GetTeam()}");
            }
            
            UpdateCombat();
        }
        
        public virtual void UpdateCombat()
        {
            // PERFORMANCE: Use cached detection manager instead of GetSystem every frame
            // Only try to cache once after systems are initialized
            if (!_detectionManagerCached)
            {
                _cachedDetectionManager = SystemInitializer.GetSystem<ICentralizedCombatDetection>();
                if (_cachedDetectionManager != null)
                    _detectionManagerCached = true;
            }
            
            if (_cachedDetectionManager == null)
            {
                // Fallback: Use individual detection if centralized system is not available
                if (Time.time - lastTargetUpdateTime >= targetUpdateInterval)
                {
                    UpdateTargetDetection();
                    lastTargetUpdateTime = Time.time;
                }
            }
            else if (currentTarget == null && !isInCombat)
            {
                // Safety net: If centralized manager exists but we have no target,
                // actively search for one. This handles cases where registration timing
                // caused us to miss initial detection or enemies spawned after us.
                if (Time.time - lastTargetUpdateTime >= targetUpdateInterval)
                {
                    UpdateTargetDetection();
                    lastTargetUpdateTime = Time.time;
                }
            }
            
            // Handle combat logic - THROTTLED for performance
            // Combat calculations (distance, rotation, weapon selection) don't need 60fps
            if (isInCombat && currentTarget != null)
            {
                if (Time.time - lastCombatUpdateTime >= COMBAT_UPDATE_INTERVAL)
                {
                    HandleCombat();
                    lastCombatUpdateTime = Time.time;
                }
            }
            else if (isInCombat && currentTarget == null)
            {
                ClearTarget();
            }
        }
        
        // Cache for IUnit components to avoid repeated GetComponent calls
        private static Dictionary<Collider, IUnit> _unitComponentCache = new Dictionary<Collider, IUnit>();
        private static Dictionary<IUnit, bool> _validTargetCache = new Dictionary<IUnit, bool>();
        private static float _cacheClearTime = 0f;
        private const float CACHE_CLEAR_INTERVAL = 5f; // Clear cache every 5 seconds

        protected virtual void UpdateTargetDetection()
        {
            // Clear cache periodically to prevent memory leaks
            if (Time.time - _cacheClearTime > CACHE_CLEAR_INTERVAL)
            {
                _unitComponentCache.Clear();
                _validTargetCache.Clear();
                _cacheClearTime = Time.time;
            }

            // If we have a current target and it's still valid, stick with it
            if (currentTarget != null && IsValidTargetCached(currentTarget))
            {
                float distanceToCurrentTarget = Vector3.Distance(transform.position, currentTarget.Position);
                if (distanceToCurrentTarget <= detectionRadius)
                {
                    return; // Keep current target
                }
                else
                {
                    ClearTarget();
                }
            }
            
            // OPTIMIZATION: Use SpatialGrid if available, fallback to Physics.OverlapSphere
            List<IUnit> enemiesInRange = GetEnemiesInRangeOptimized();
            
            IUnit closestEnemy = null;
            float closestDistance = float.MaxValue;
            
            foreach (var enemy in enemiesInRange)
            {
                if (IsValidTargetCached(enemy))
                {
                    float distance = Vector3.Distance(transform.position, enemy.Position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
            }
            
            // Update target only if we found a new valid target
            if (closestEnemy != null && closestEnemy != currentTarget)
            {
                // Allow immediate target acquisition if we have no current target
                // Only apply targetStickTime delay when SWITCHING between targets
                bool canAcquireTarget = currentTarget == null || 
                                        (Time.time - lastTargetChangeTime >= targetStickTime);
                
                if (canAcquireTarget)
                {
                    Debug.Log($"{Name}: Setting new target {closestEnemy.Name} at distance {Vector3.Distance(transform.position, closestEnemy.Position):F1}m");
                    SetTarget(closestEnemy);
                    lastTargetChangeTime = Time.time;
                }
                else
                {
                    Debug.Log($"{Name}: Found enemy {closestEnemy.Name} but target stick time not ready ({(Time.time - lastTargetChangeTime):F1}s / {targetStickTime:F1}s)");
                }
            }
            else if (closestEnemy == null && currentTarget != null)
            {
                Debug.Log($"{Name}: No enemies found, clearing target");
                ClearTarget();
            }
            else if (closestEnemy == null)
            {
                Debug.Log($"{Name}: No enemies found in range");
            }
        }

        /// <summary>
        /// Optimized method to get enemies in range using SpatialGrid when available
        /// </summary>
        private List<IUnit> GetEnemiesInRangeOptimized()
        {
            // Try to use SpatialGrid first (much faster)
            var spatialQuery = DependencyContainer.Instance.TryResolve<ISpatialUnitQuery>();
            if (spatialQuery != null)
            {
                var allUnitsInRange = spatialQuery.GetUnitsInRadius(transform.position, detectionRadius);
                var enemies = new List<IUnit>();
                
                foreach (var unit in allUnitsInRange)
                {
                    if (unit != unitInterface && IsEnemy(unit))
                    {
                        enemies.Add(unit);
                    }
                }
                
                return enemies;
            }
            
            // Fallback to Physics.OverlapSphere (slower but works without SpatialGrid)
            var enemiesInRange = new List<IUnit>();
            Collider[] collidersInRange = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayerMask);
            
            Debug.Log($"{Name}: Physics.OverlapSphere found {collidersInRange.Length} colliders in range");
            
            foreach (var collider in collidersInRange)
            {
                IUnit enemy = GetCachedUnitComponent(collider);
                if (enemy != null && enemy != unitInterface)
                {
                    bool isEnemy = IsEnemy(enemy);
                    float distance = Vector3.Distance(transform.position, enemy.Position);
                    Debug.Log($"{Name}: Checking {enemy.Name} at {distance:F1}m - IsEnemy: {isEnemy}");
                    
                    if (isEnemy)
                    {
                        enemiesInRange.Add(enemy);
                    }
                }
            }
            
            Debug.Log($"{Name}: Found {enemiesInRange.Count} enemies in range");
            return enemiesInRange;
        }

        /// <summary>
        /// Cached GetComponent call to avoid repeated expensive operations
        /// Uses GetComponentInParent to find IUnit even if collider is on a child object
        /// </summary>
        private IUnit GetCachedUnitComponent(Collider collider)
        {
            if (_unitComponentCache.TryGetValue(collider, out IUnit unit))
            {
                return unit;
            }
            
            // Use GetComponentInParent to handle cases where collider is on a child object
            // but IUnit component is on the parent (common tank setup)
            unit = collider.GetComponentInParent<IUnit>();
            _unitComponentCache[collider] = unit;
            return unit;
        }

        /// <summary>
        /// Cached IsValidTarget call to avoid repeated validation
        /// </summary>
        private bool IsValidTargetCached(IUnit target)
        {
            if (target == null) return false;
            
            if (_validTargetCache.TryGetValue(target, out bool isValid))
            {
                return isValid;
            }
            
            isValid = IsValidTarget(target);
            _validTargetCache[target] = isValid;
            return isValid;
        }

        /// <summary>
        /// Fast enemy check without full validation
        /// </summary>
        private bool IsEnemy(IUnit otherUnit)
        {
            if (otherUnit == null || otherUnit == unitInterface) return false;
            if (otherUnit.Health <= 0) return false;
            
            // Check if target is on different team using Owner property
            if (Owner != null && otherUnit.Owner != null)
            {
                return Owner.Team != otherUnit.Owner.Team;
            }
            
            return false; // No team info, not an enemy
        }

        public virtual bool IsValidTarget(IUnit target)
        {
            // Basic validation - can be overridden by derived classes
            // Note: Debug logs removed - this method is called thousands of times per combat
            if (target == null) return false;
            if (target == unitInterface) return false;
            if (target.Health <= 0) return false;
            
            // Check if target is on different team using Owner property
            if (Owner != null && target.Owner != null)
            {
                return Owner.Team != target.Owner.Team;
            }
            
            return false; // Missing team information
        }
        
        public virtual void SetTarget(IUnit target)
        {
            if (target == currentTarget) 
            {
                // Debug.Log($"{Name}: SetTarget called with same target {target.Name} - ignoring");
                return;
            }
            
            // Debug.Log($"{Name}: SetTarget called for {target.Name} ({target.GetType().Name})");
            
            currentTarget = target;
            isInCombat = true;
            isTargetInRange = false;
            isGunFacingTarget = false;
            
            // Debug.Log($"{Name} ({gameObject.name}) targeting {target.Name} ({target.Name})");
        }
        
        public virtual void ClearTarget()
        {
            currentTarget = null;
            isInCombat = false;
            isTargetInRange = false;
            isGunFacingTarget = false;
        }
        
        /// <summary>
        /// Convenience method to set team for this combat unit
        /// </summary>
        public virtual void SetTeam(Team team)
        {
            if (unitInterface != null)
            {
                unitInterface.AssignToTeam(team);
                
                // Configure enemy layer mask based on team
                ConfigureEnemyLayerMask();
            }
            else
            {
                Debug.LogError($"{Name}: Cannot set team - unitInterface is null");
            }
        }
        
        /// <summary>
        /// Get the current team assignment
        /// </summary>
        public virtual Team GetTeam()
        {
            return unitInterface?.Owner?.Team ?? Team.None;
        }
        
        /// <summary>
        /// Configure enemy layer mask based on the unit's team
        /// Player units detect AI layer (7), AI units detect Player layer (6)
        /// </summary>
        protected virtual void ConfigureEnemyLayerMask()
        {
            Team currentTeam = GetTeam();
            
            if (currentTeam == Team.Player)
            {
                // Player unit should detect AI units (layer 7)
                enemyLayerMask = 1 << 7; // AI layer
                Debug.Log($"{Name}: Configured to detect AI layer (7)");
            }
            else if (currentTeam == Team.AI)
            {
                // AI unit should detect Player units (layer 6)
                enemyLayerMask = 1 << 6; // Player layer
                Debug.Log($"{Name}: Configured to detect Player layer (6)");
            }
            else
            {
                // No team assigned, use all layers as fallback
                enemyLayerMask = -1;
                Debug.LogWarning($"{Name}: No team assigned, using all layers for detection");
            }
        }
        
        protected virtual void HandleCombat()
        {
            if (currentTarget == null) return;
            
            // Check if target is still valid
            if (!IsValidTarget(currentTarget))
            {
                // Debug.Log($"{Name}: Target {currentTarget.Name} is no longer valid, clearing target");
                ClearTarget();
                return;
            }
            
            // Check range
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.Position);
            isTargetInRange = distanceToTarget <= AttackRange;
            
            // Debug.Log($"{Name}: Combat update - Distance: {distanceToTarget:F1}m, InRange: {isTargetInRange}, AttackRange: {AttackRange:F1}m");
            
            if (isTargetInRange)
            {
                // Rotate gun towards target
                RotateGunTowardsTarget();
                
                // Check if gun is facing target
                isGunFacingTarget = IsGunFacingCurrentTarget();
                
                // Debug.Log($"{Name}: Gun facing target: {isGunFacingTarget}");
                
                // Try to attack if ready
                if (isGunFacingTarget)
                {
                    // Debug.Log($"{Name}: Gun is facing target, attempting attack");
                    TryAttack();
                }
                else
                {
                    // Debug.Log($"{Name}: Gun is not yet facing target, continuing rotation");
                }
            }
            else
            {
                // Debug.Log($"{Name}: Target not in range, cannot attack");
            }
        }
        
        public virtual void RotateGunTowardsTarget()
        {
            if (gunTurret is null || currentTarget == null) 
            {
                Debug.LogWarning($"{Name}: RotateGunTowardsTarget failed - gunTurret or currentTarget is null");
                return;
            }
            
            // Debug.Log($"{Name}: Rotating gun towards target {currentTarget.Name} at position {currentTarget.Position}");
            gunTurret.SetTarget(currentTarget.Position);
        }
        
        protected virtual bool IsGunFacingCurrentTarget()
        {
            if (gunTurret is null || currentTarget == null) 
            {
                Debug.LogWarning($"{Name}: IsGunFacingCurrentTarget failed - gunTurret or currentTarget is null");
                return false;
            }
            
            isGunFacingTarget = gunTurret.IsFacingTarget(currentTarget.Position);
            
            if (isGunFacingTarget)
            {
                // Debug.Log($"{Name}: Gun is facing target {currentTarget.Name}");
            }
            else
            {
                // Debug.Log($"{Name}: Gun is NOT facing target {currentTarget.Name} - continuing rotation");
            }
            
            return isGunFacingTarget;
        }
        
        public virtual bool TryAttack()
        {
            if (currentTarget == null) 
            {
                Debug.LogWarning($"{Name}: TryAttack failed - no current target");
                return false;
            }
            
            // Check cooldown
            if (Time.time - lastAttackTime < AttackCooldown)
            {
                // Debug.Log($"{Name}: TryAttack failed - cooldown not ready ({(Time.time - lastAttackTime):F1}s / {AttackCooldown:F1}s)");
                return false;
            }
            
            // Debug.Log($"{Name}: Attempting attack on {currentTarget.Name} - Range: {Vector3.Distance(transform.position, currentTarget.Position):F1}m, Max: {AttackRange:F1}m");
            
            // Create projectile
            CreateProjectile(currentTarget);
            
            // Update attack time
            lastAttackTime = Time.time;
            // Debug.Log($"{Name}: Attack successful on {currentTarget.Name}!");
            
            // Notify LightweightFireSystem for effects and coordination
            NotifyFireSystemOfAttack(currentTarget);
            
            return true;
        }
        
        /// <summary>
        /// Create and fire a projectile at the target with accuracy system
        /// Derived classes must implement their own projectile creation logic
        /// </summary>
        protected abstract void CreateProjectile(IUnit target);
        
        /// <summary>
        /// Notify the LightweightFireSystem of an attack for effects and coordination
        /// </summary>
        protected virtual void NotifyFireSystemOfAttack(IUnit target)
        {
            var fireCoordinator = SystemInitializer.GetSystem<ICombatFireCoordinator>();
            if (fireCoordinator != null)
                fireCoordinator.ProcessAttack(unitInterface, target);
            else
                Debug.LogWarning($"{Name}: ICombatFireCoordinator not found - attack effects may not work");
        }
        
        /// <summary>
        /// Register this combat unit with the LightweightFireSystem
        /// </summary>
        protected virtual void RegisterWithFireSystem()
        {
            var fireCoordinator = SystemInitializer.GetSystem<ICombatFireCoordinator>();
            if (fireCoordinator != null && unitInterface is ICombatUnit combatUnit)
                fireCoordinator.RegisterCombatUnit(combatUnit);
        }
        
        /// <summary>
        /// Register this combat unit with the CentralizedDetectionManager
        /// </summary>
        protected virtual void RegisterWithDetectionManager()
        {
            var detection = SystemInitializer.GetSystem<ICentralizedCombatDetection>();
            if (detection != null)
                detection.RegisterCombatUnit(this);
        }
        
        // Visual debugging
        protected virtual void OnDrawGizmosSelected()
        {
            // Detection radius
            Gizmos.color = isInCombat ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
            
            // Target line
            if (currentTarget == null) return;
            Gizmos.color = isGunFacingTarget ? Color.green : Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.Position);
        }
        
        #region Accuracy System (Base Implementation)
        
        /// <summary>
        /// Calculate current accuracy based on movement
        /// </summary>
        public virtual float CalculateCurrentAccuracy()
        {
            float finalAccuracy = baseUnitAccuracy;
            
            // Apply movement penalty
            if (IsMoving)
            {
                float movementPenalty = Mathf.Min(baseMovingAccuracyPenalty, baseMaxAccuracyPenalty);
                finalAccuracy -= movementPenalty;
            }
            
            // Ensure accuracy is within valid range (0-100)
            finalAccuracy = Mathf.Clamp(finalAccuracy, 0f, 100f);
            
            return finalAccuracy;
        }
        
        /// <summary>
        /// Perform accuracy check and return hit result
        /// </summary>
        public virtual HitResult PerformAccuracyCheck()
        {
            float currentAccuracy = CalculateCurrentAccuracy();
            float randomRoll = UnityEngine.Random.Range(0f, 100f);
            
            if (randomRoll <= currentAccuracy)
            {
                // Full hit
                return HitResult.FullHit;
            }
            else
            {
                // Miss or partial hit
                float partialHitRoll = UnityEngine.Random.Range(0f, 100f);
                if (partialHitRoll <= basePartialHitChance)
                {
                    return HitResult.PartialHit;
                }
                else
                {
                    return HitResult.Miss;
                }
            }
        }
        
        /// <summary>
        /// Calculate damage based on hit result
        /// </summary>
        public virtual float CalculateDamage(HitResult hitResult)
        {
            float baseDamage = AttackDamage;
            
            switch (hitResult)
            {
                case HitResult.FullHit:
                    return baseDamage;
                case HitResult.PartialHit:
                    return baseDamage * basePartialHitDamageMultiplier;
                case HitResult.Miss:
                    return 0f;
                default:
                    return baseDamage;
            }
        }
        
        /// <summary>
        /// Get accuracy information for debugging/UI
        /// </summary>
        public virtual AccuracyInfo GetAccuracyInfo()
        {
            return new AccuracyInfo
            {
                BaseAccuracy = baseUnitAccuracy,
                CurrentAccuracy = CalculateCurrentAccuracy(),
                IsMoving = IsMoving,
                MovementPenalty = IsMoving ? Mathf.Min(baseMovingAccuracyPenalty, baseMaxAccuracyPenalty) : 0f,
                WeaponType = "Standard Weapon"
            };
        }
        
        #endregion
    }
}
