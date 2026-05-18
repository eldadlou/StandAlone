using System;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Core.Units;
using MyGame.Core;
using MyGame.Presentation;

namespace MyGame.RuntimeSystems.Combat
{
    public class FireSystem : MonoBehaviour
    {
        [Header("System Settings")]
        [SerializeField] public bool enableFireSystem = false; // Disable to prevent conflicts with CombatUnit
        
        [Header("Combat Settings")] public bool enableProjectiles = true;
        public GameObject projectilePrefab;
        public float projectileSpeed = 20f;

        [Header("Detection & Targeting")] public float detectionRadius = 15f;
        public LayerMask enemyLayerMask = -1;
        public float targetUpdateInterval = 0.5f;

        [Header("Gun Rotation")] public float gunRotationSpeed = 90f; // degrees per second
        public float rotationThreshold = 5f; // degrees tolerance for "facing target"
        public Transform gunTurret; // Assign the gun/turret transform in inspector
        public bool useGunTurretComponent = true; // Use GunTurret component if available

        [Header("Attack Cooldown")] public float attackCooldown = 2f;
        public bool useIndividualCooldowns = true;

        // Private fields
        private Dictionary<IUnit, float> unitCooldowns = new Dictionary<IUnit, float>();
        private Dictionary<IUnit, IUnit> currentTargets = new Dictionary<IUnit, IUnit>();
        private Dictionary<IUnit, bool> isRotating = new Dictionary<IUnit, bool>();
        private Dictionary<IUnit, GunTurret> unitGunTurrets = new Dictionary<IUnit, GunTurret>();
        private float lastTargetUpdateTime;

        private void Awake()
        {
            // Register with DependencyContainer
            DependencyContainer.Instance.Register(this);
            
            if (!enableFireSystem)
            {
                // Debug.LogWarning("FireSystem is disabled to prevent conflicts with CombatUnit system");
            }
        }

        public void SubscribeToUnit(IUnit unit)
        {
            if (!enableFireSystem) return;
            
            unit.OnAttack += HandleAttack;
            unit.OnAnimationEvent += HandleAnimationEvent;

            // Initialize cooldown tracking
            if (useIndividualCooldowns)
            {
                unitCooldowns[unit] = 0f;
            }

            // Find GunTurret component if available
            if (useGunTurretComponent && unit is MonoBehaviour unitMono)
            {
                var gunTurret = unitMono.GetComponentInChildren<GunTurret>();
                if (gunTurret != null)
                {
                    unitGunTurrets[unit] = gunTurret;
                    // Configure GunTurret with FireSystem settings
                    gunTurret.SetRotationSpeed(gunRotationSpeed);
                    gunTurret.SetRotationThreshold(rotationThreshold);
                }
            }
        }

        public void UnsubscribeFromUnit(IUnit unit)
        {
            if (!enableFireSystem) return;
            
            unit.OnAttack -= HandleAttack;
            unit.OnAnimationEvent -= HandleAnimationEvent;

            // Clean up tracking
            unitCooldowns.Remove(unit);
            currentTargets.Remove(unit);
            isRotating.Remove(unit);
            unitGunTurrets.Remove(unit);
        }

        private void Update()
        {
            if (!enableFireSystem) return;
            
            // Update target detection periodically
            if (Time.time - lastTargetUpdateTime >= targetUpdateInterval)
            {
                UpdateTargetDetection();
                lastTargetUpdateTime = Time.time;
            }

            // Update gun rotations
            UpdateGunRotations();
        }

        private void UpdateTargetDetection()
        {
            // Find all units in the scene
            var allUnits = FindObjectsOfType<MonoBehaviour>();

            foreach (var unit in allUnits)
            {
                if (unit is IUnit iUnit)
                {
                    // Find nearest enemy within detection radius
                    IUnit nearestEnemy = FindNearestEnemy(iUnit);

                    if (nearestEnemy != null)
                    {
                        currentTargets[iUnit] = nearestEnemy;

                        // Check if we can attack (cooldown, range, etc.)
                        if (CanAttackTarget(iUnit, nearestEnemy))
                        {
                            // Start rotating gun if not already facing target
                            if (!IsGunFacingTarget(iUnit, nearestEnemy))
                            {
                                isRotating[iUnit] = true;
                            }
                            else
                            {
                                // Gun is facing target, perform attack
                                PerformAttack(iUnit, nearestEnemy);
                            }
                        }
                    }
                    else
                    {
                        // No target found, stop rotating
                        currentTargets.Remove(iUnit);
                        isRotating[iUnit] = false;
                    }
                }
            }
        }

        private IUnit FindNearestEnemy(IUnit unit)
        {
            IUnit nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            var allUnits = FindObjectsOfType<MonoBehaviour>();

            foreach (var obj in allUnits)
            {
                if (obj is IUnit otherUnit && otherUnit != unit)
                {
                    // Check if it's an enemy (different team)
                    if (IsEnemy(unit, otherUnit))
                    {
                        float distance = Vector3.Distance(unit.Position, otherUnit.Position);

                        if (distance <= detectionRadius && distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestEnemy = otherUnit;
                        }
                    }
                }
            }

            return nearestEnemy;
        }

        private bool IsEnemy(IUnit unit1, IUnit unit2)
        {
            // Check if units are on different teams
            if (unit1.Owner != null && unit2.Owner != null)
            {
                bool isEnemy = unit1.Owner.Team != unit2.Owner.Team;
                // Debug.Log($"FireSystem: {unit1.Name} ({unit1.Owner.Team}) vs {unit2.Name} ({unit2.Owner.Team}) - IsEnemy: {isEnemy}");
                return isEnemy;
            }

            // If no owner info, log warning and treat as enemy (fallback)
            // Debug.LogWarning($"FireSystem: Missing team information for {unit1.Name} or {unit2.Name} - treating as enemy");
            return true;
        }

        private bool CanAttackTarget(IUnit attacker, IUnit target)
        {
            if (attacker == null || target == null) return false;

            // Check if target is still valid
            if (!IsEnemy(attacker, target)) return false;

            // Check range
            float distance = Vector3.Distance(attacker.Position, target.Position);
            if (distance > attacker.AttackRange) return false;

            // Check cooldown
            if (useIndividualCooldowns)
            {
                if (unitCooldowns.ContainsKey(attacker))
                {
                    if (Time.time - unitCooldowns[attacker] < attackCooldown)
                        return false;
                }
            }
            else
            {
                // Use unit's built-in cooldown
                if (Time.time - attacker.LastAttackTime < attacker.AttackCooldown)
                    return false;
            }

            return true;
        }

        private bool IsGunFacingTarget(IUnit unit, IUnit target)
        {
            // Use GunTurret component if available
            if (unitGunTurrets.ContainsKey(unit))
            {
                return unitGunTurrets[unit].IsFacingTarget(target.Position);
            }

            // Fallback to direct transform manipulation
            if (gunTurret == null) return true; // If no gun turret, assume always facing

            Vector3 directionToTarget = (target.Position - unit.Position).normalized;
            Vector3 gunForward = gunTurret.forward;

            // Ignore Y component for ground-based targeting
            directionToTarget.y = 0;
            gunForward.y = 0;

            float angle = Vector3.Angle(gunForward, directionToTarget);
            return angle <= rotationThreshold;
        }

        private void UpdateGunRotations()
        {
            foreach (var kvp in currentTargets)
            {
                IUnit unit = kvp.Key;
                IUnit target = kvp.Value;

                if (isRotating.ContainsKey(unit) && isRotating[unit])
                {
                    RotateGunTowardsTarget(unit, target);
                }
            }
        }

        private void RotateGunTowardsTarget(IUnit unit, IUnit target)
        {
            // Use GunTurret component if available
            if (unitGunTurrets.ContainsKey(unit))
            {
                unitGunTurrets[unit].SetTarget(target.Position);

                // Check if we're now facing the target
                if (unitGunTurrets[unit].IsFacingTarget(target.Position))
                {
                    isRotating[unit] = false;
                }

                return;
            }

            // Fallback to direct transform manipulation
            if (gunTurret == null) return;

            Vector3 directionToTarget = (target.Position - unit.Position).normalized;
            directionToTarget.y = 0; // Keep rotation on horizontal plane

            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                gunTurret.rotation = Quaternion.RotateTowards(
                    gunTurret.rotation,
                    targetRotation,
                    gunRotationSpeed * Time.deltaTime
                );

                // Check if we're now facing the target
                if (IsGunFacingTarget(unit, target))
                {
                    isRotating[unit] = false;
                }
            }
        }

        private void PerformAttack(IUnit attacker, IUnit target)
        {
            // Update cooldown
            if (useIndividualCooldowns)
            {
                unitCooldowns[attacker] = Time.time;
            }

            // Trigger the attack through the unit's attack system
            attacker.Attack(target);
        }

        private void HandleAttack(IUnit attacker, IUnit target)
        {
            if (attacker == null || target == null) return;

            // Play attack animation
            attacker.PlayAnimation("Fire");

            // Create visual effects
            CreateAttackEffects(attacker, target);

            // Debug.Log($"FireSystem: {attacker.GetType().Name} attacking {target.GetType().Name}");
        }

        private void HandleAnimationEvent(string eventName)
        {
            if (eventName == "Fire")
            {
                // Handle fire animation event
                // Debug.Log("FireSystem: Fire animation event triggered");
            }
        }

        private void CreateAttackEffects(IUnit attacker, IUnit target)
        {
            if (!enableProjectiles) return;

            // Create projectile effect
            if (projectilePrefab != null)
            {
                // Use GunTurret position if available, otherwise use unit position
                Vector3 firePosition = attacker.Position;
                if (unitGunTurrets.ContainsKey(attacker))
                {
                    firePosition = unitGunTurrets[attacker].transform.position;
                }
                else if (gunTurret != null)
                {
                    firePosition = gunTurret.position;
                }

                GameObject projectile = Instantiate(projectilePrefab, firePosition, Quaternion.identity);

                // Set projectile direction
                Vector3 direction = (target.Position - firePosition).normalized;
                projectile.transform.rotation = Quaternion.LookRotation(direction);

                // Add projectile behavior
                ProjectileBehavior projectileBehavior = projectile.GetComponent<ProjectileBehavior>();
                if (projectileBehavior == null)
                {
                    projectileBehavior = projectile.AddComponent<ProjectileBehavior>();
                }

                projectileBehavior.Initialize(attacker, target, projectileSpeed);
            }
        }

        // Public methods for external control
        public void SetDetectionRadius(float radius)
        {
            detectionRadius = radius;
        }

        public void SetGunRotationSpeed(float speed)
        {
            gunRotationSpeed = speed;
        }

        public void SetAttackCooldown(float cooldown)
        {
            attackCooldown = cooldown;
        }

        public IUnit GetCurrentTarget(IUnit unit)
        {
            return currentTargets.ContainsKey(unit) ? currentTargets[unit] : null;
        }

        public bool IsRotating(IUnit unit)
        {
            return isRotating.ContainsKey(unit) && isRotating[unit];
        }

        // Debug visualization
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                // Draw detection radius
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, detectionRadius);

                // Draw current targets
                foreach (var kvp in currentTargets)
                {
                    if (kvp.Key != null && kvp.Value != null)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(kvp.Key.Position, kvp.Value.Position);
                    }
                }
            }
        }
    }

}