using System.Collections.Generic;
using UnityEngine;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Units.Combat;
using MyGame.Core.SpatialPartitioning;
using MyGame.Core.Services;

namespace MyGame.RuntimeSystems.Combat
{
    /// <summary>
    /// System that coordinates allied units to support each other in combat.
    /// When a unit is attacked, nearby friendly units will move to help and engage the attacker.
    /// </summary>
    public class AlliedSupportSystem : MonoBehaviour, IAlliedCombatSupport
    {
        [Header("Alert Settings")]
        [Tooltip("Radius within which friendly units will be alerted when an ally is attacked")]
        [SerializeField] private float alertRadius = 25f;
        
        [Tooltip("Maximum number of units that can respond to a single alert")]
        [SerializeField] private int maxRespondingUnits = 5;
        
        [Tooltip("Cooldown between alerts for the same victim unit (prevents spam)")]
        [SerializeField] private float alertCooldown = 5f;
        
        [Tooltip("Minimum damage required to trigger an alert")]
        [SerializeField] private float minDamageToAlert = 1f;
        
        [Header("Response Settings")]
        [Tooltip("Units must be within this distance to engage the attacker")]
        [SerializeField] private float engageDistance = 15f;
        
        [Tooltip("If true, responding units will move towards the attacked ally")]
        [SerializeField] private bool moveToAssist = true;
        
        [Tooltip("Minimum distance to keep from the attacked ally (not too close)")]
        [SerializeField] private float minDistanceFromAlly = 6f;
        
        [Tooltip("Maximum distance from the attacked ally when positioning")]
        [SerializeField] private float maxDistanceFromAlly = 12f;
        
        [Tooltip("How far to spread units to the sides (in degrees from the attacker direction)")]
        [SerializeField] private float flankingAngle = 45f;
        
        [Tooltip("Spacing between assisting units")]
        [SerializeField] private float unitSpacing = 4f;
        
        [Header("Responder Settings")]
        [Tooltip("Cooldown before a responding unit can be redirected to a new support call")]
        [SerializeField] private float responderRedirectCooldown = 8f;
        
        [Tooltip("If true, units already responding to support calls won't be redirected")]
        [SerializeField] private bool preventResponderRedirect = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = false;
        [SerializeField] private bool drawDebugGizmos = true;
        
        // Track alert cooldowns per victim unit
        private Dictionary<IUnit, float> lastAlertTimes = new Dictionary<IUnit, float>();
        
        // Track when each unit last responded to a support call (prevents constant repositioning)
        private Dictionary<IUnit, float> responderLastResponseTime = new Dictionary<IUnit, float>();
        
        // Track which unit each responder is currently assisting
        private Dictionary<IUnit, IUnit> responderCurrentAssignment = new Dictionary<IUnit, IUnit>();
        
        // Track recent alerts for debug visualization
        private List<AlertInfo> recentAlerts = new List<AlertInfo>();
        private float alertVisualizationDuration = 2f;
        
        // Core systems
        private ISpatialUnitQuery spatialQuery;
        
        // === CACHED COLLECTIONS TO AVOID GC ALLOCATIONS ===
        // Reusable list for nearby friendlies query
        private List<IUnit> cachedFriendlyList = new List<IUnit>(16);
        
        // Pre-allocated collider array for Physics queries (avoids OverlapSphere allocation)
        private Collider[] cachedColliderBuffer = new Collider[64];
        
        // Reusable list for cleanup operations
        private List<IUnit> cachedCleanupList = new List<IUnit>(32);
        
        // Cached comparison delegate for sorting (avoids lambda allocation)
        private IUnit cachedSortVictim;
        private System.Comparison<IUnit> cachedDistanceComparison;
        
        private struct AlertInfo
        {
            public Vector3 victimPosition;
            public Vector3 attackerPosition;
            public float time;
            public int respondersCount;
        }
        
        private void Awake()
        {
            spatialQuery = DependencyContainer.Instance.TryResolve<ISpatialUnitQuery>();
            if (spatialQuery == null)
            {
                spatialQuery = FindObjectOfType<SpatialGrid>();
            }
            
            // Initialize cached comparison delegate (avoids lambda allocation during sort)
            cachedDistanceComparison = CompareUnitsByDistanceToVictim;
            
            var container = DependencyContainer.Instance;
            container.Register(this);
            container.RegisterAs<IAlliedCombatSupport>(this);
            
            if (enableDebugLogging)
                Debug.Log("AlliedSupportSystem initialized");
        }
        
        /// <summary>
        /// Comparison method for sorting units by distance (used to avoid lambda allocations)
        /// </summary>
        private int CompareUnitsByDistanceToVictim(IUnit a, IUnit b)
        {
            float distA = Vector3.SqrMagnitude(a.Position - cachedSortVictim.Position);
            float distB = Vector3.SqrMagnitude(b.Position - cachedSortVictim.Position);
            return distA.CompareTo(distB);
        }
        
        /// <summary>
        /// Call this when a unit is attacked to alert nearby friendly units.
        /// Should be called from ProjectileBehavior when damage is dealt.
        /// </summary>
        /// <param name="victim">The unit that was attacked</param>
        /// <param name="attacker">The unit that performed the attack</param>
        /// <param name="damage">Amount of damage dealt</param>
        public void NotifyUnitAttacked(IUnit victim, IUnit attacker, float damage)
        {
            if (victim == null || attacker == null) return;
            
            // Check minimum damage threshold
            if (damage < minDamageToAlert) return;
            
            // Check cooldown for this victim
            if (lastAlertTimes.TryGetValue(victim, out float lastTime))
            {
                if (Time.time - lastTime < alertCooldown) return;
            }
            
            // Update cooldown
            lastAlertTimes[victim] = Time.time;
            
            if (enableDebugLogging)
                Debug.Log($"AlliedSupportSystem: {victim.Name} attacked by {attacker.Name} for {damage:F1} damage - alerting nearby allies");
            
            // Find and alert nearby friendly units
            int respondersCount = AlertNearbyAllies(victim, attacker);
            
            // Store for debug visualization
            if (drawDebugGizmos)
            {
                recentAlerts.Add(new AlertInfo
                {
                    victimPosition = victim.Position,
                    attackerPosition = attacker.Position,
                    time = Time.time,
                    respondersCount = respondersCount
                });
            }
        }
        
        /// <summary>
        /// Find and alert nearby friendly units to assist the victim
        /// </summary>
        private int AlertNearbyAllies(IUnit victim, IUnit attacker)
        {
            // Use cached list to avoid GC allocation
            GetNearbyFriendlyUnits(victim, cachedFriendlyList);
            
            if (cachedFriendlyList.Count == 0)
            {
                if (enableDebugLogging)
                    Debug.Log($"AlliedSupportSystem: No nearby allies found for {victim.Name}");
                return 0;
            }
            
            // Sort by distance to victim (closest first) using cached comparison delegate
            cachedSortVictim = victim;
            cachedFriendlyList.Sort(cachedDistanceComparison);
            
            int respondersCount = 0;
            
            // Use for loop instead of foreach to avoid enumerator allocation
            int count = cachedFriendlyList.Count;
            for (int i = 0; i < count; i++)
            {
                if (respondersCount >= maxRespondingUnits) break;
                
                // Try to alert this ally, passing the responder index for positioning
                if (TryAlertUnit(cachedFriendlyList[i], victim, attacker, respondersCount))
                {
                    respondersCount++;
                }
            }
            
            if (enableDebugLogging)
                Debug.Log($"AlliedSupportSystem: {respondersCount} allies responding to help {victim.Name}");
            
            return respondersCount;
        }
        
        /// <summary>
        /// Get all friendly units within alert radius of the victim (fills provided list to avoid GC)
        /// </summary>
        private void GetNearbyFriendlyUnits(IUnit victim, List<IUnit> resultList)
        {
            resultList.Clear();
            
            if (spatialQuery != null)
            {
                var unitsInRange = spatialQuery.GetUnitsInRadius(victim.Position, alertRadius);
                
                foreach (var unit in unitsInRange)
                {
                    if (IsFriendlyUnit(victim, unit) && unit != victim)
                    {
                        resultList.Add(unit);
                    }
                }
            }
            else
            {
                // Fallback to Physics.OverlapSphereNonAlloc (avoids array allocation)
                int hitCount = Physics.OverlapSphereNonAlloc(victim.Position, alertRadius, cachedColliderBuffer);
                
                for (int i = 0; i < hitCount; i++)
                {
                    IUnit unit = cachedColliderBuffer[i].GetComponentInParent<IUnit>();
                    if (unit != null && IsFriendlyUnit(victim, unit) && unit != victim)
                    {
                        // Check for duplicates (same unit can have multiple colliders)
                        bool alreadyAdded = false;
                        for (int j = 0; j < resultList.Count; j++)
                        {
                            if (resultList[j] == unit)
                            {
                                alreadyAdded = true;
                                break;
                            }
                        }
                        if (!alreadyAdded)
                            resultList.Add(unit);
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if two units are on the same team
        /// </summary>
        private bool IsFriendlyUnit(IUnit unit1, IUnit unit2)
        {
            if (unit1 == null || unit2 == null) return false;
            if (unit1.Owner == null || unit2.Owner == null) return false;
            
            return unit1.Owner.Team == unit2.Owner.Team;
        }
        
        /// <summary>
        /// Alert a specific unit to assist the victim against the attacker
        /// </summary>
        /// <param name="responderIndex">Index of this responder (used for positioning)</param>
        private bool TryAlertUnit(IUnit ally, IUnit victim, IUnit attacker, int responderIndex)
        {
            // Get the CombatUnit component for this ally
            CombatUnit allyCombat = GetCombatUnit(ally);
            if (allyCombat == null) return false;
            
            // Check if this responder is on cooldown (recently responded to another call)
            if (preventResponderRedirect && IsResponderOnCooldown(ally, victim))
            {
                if (enableDebugLogging)
                    Debug.Log($"AlliedSupportSystem: {ally.Name} on cooldown, skipping (already assisting another unit)");
                return false;
            }
            
            // Check if ally already has a target
            if (allyCombat.CurrentTarget != null)
            {
                // Only override if the new attacker is closer
                float currentTargetDist = Vector3.Distance(ally.Position, allyCombat.CurrentTarget.Position);
                float attackerDist = Vector3.Distance(ally.Position, attacker.Position);
                
                // Keep current target if it's closer
                if (currentTargetDist <= attackerDist)
                {
                    if (enableDebugLogging)
                        Debug.Log($"AlliedSupportSystem: {ally.Name} keeping current target (closer)");
                    return false;
                }
            }
            
            // Check if attacker is in engage distance
            float distanceToAttacker = Vector3.Distance(ally.Position, attacker.Position);
            
            if (distanceToAttacker <= engageDistance)
            {
                // Directly engage the attacker - no movement needed, just set target
                allyCombat.SetTarget(attacker);
                
                // Track this response (but with shorter cooldown since no movement involved)
                RecordResponderAssignment(ally, victim, isMovementRequired: false);
                
                if (enableDebugLogging)
                    Debug.Log($"AlliedSupportSystem: {ally.Name} engaging attacker {attacker.Name} at distance {distanceToAttacker:F1}m");
                
                return true;
            }
            else if (moveToAssist)
            {
                // Move to a flanking position beside the victim
                Vector3 moveTarget = CalculateFlankingPosition(ally, victim, attacker, responderIndex);
                ally.MoveTo(moveTarget);
                
                // Set the attacker as the target so they'll engage when in range
                allyCombat.SetTarget(attacker);
                
                // Track this response with full cooldown since movement is involved
                RecordResponderAssignment(ally, victim, isMovementRequired: true);
                
                if (enableDebugLogging)
                    Debug.Log($"AlliedSupportSystem: {ally.Name} moving to flank position to assist {victim.Name} against {attacker.Name}");
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a responder unit is on cooldown from a recent response
        /// </summary>
        private bool IsResponderOnCooldown(IUnit responder, IUnit newVictim)
        {
            // Check if responder has responded recently
            if (responderLastResponseTime.TryGetValue(responder, out float lastTime))
            {
                if (Time.time - lastTime < responderRedirectCooldown)
                {
                    // Check if they're already assisting the same victim (allow re-engagement with same ally)
                    if (responderCurrentAssignment.TryGetValue(responder, out IUnit currentVictim))
                    {
                        // Allow if it's the same victim they're already helping
                        if (currentVictim == newVictim)
                            return false;
                    }
                    
                    // On cooldown and it's a different victim - don't redirect
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Record that a responder has been assigned to help a victim
        /// </summary>
        private void RecordResponderAssignment(IUnit responder, IUnit victim, bool isMovementRequired)
        {
            // Only apply full cooldown if movement was required
            // If just engaging nearby enemy, use shorter cooldown
            responderLastResponseTime[responder] = isMovementRequired ? Time.time : Time.time - (responderRedirectCooldown * 0.5f);
            responderCurrentAssignment[responder] = victim;
        }
        
        /// <summary>
        /// Calculate a flanking position to the side of the victim, not too close and spread out from other responders
        /// </summary>
        /// <param name="responderIndex">Index of this responder (0, 1, 2, etc.) used to spread units out</param>
        private Vector3 CalculateFlankingPosition(IUnit ally, IUnit victim, IUnit attacker, int responderIndex)
        {
            // Get the direction from victim to attacker (the "front" direction)
            Vector3 victimToAttacker = (attacker.Position - victim.Position).normalized;
            
            // Calculate the perpendicular direction (to the side)
            Vector3 sideDirection = Vector3.Cross(victimToAttacker, Vector3.up).normalized;
            
            // Alternate sides: even indices go left, odd indices go right
            bool goRight = (responderIndex % 2 == 0);
            
            // Calculate the flanking angle based on responder index
            // First two units flank at base angle, subsequent units spread further
            int pairIndex = responderIndex / 2; // 0,1 -> 0, 2,3 -> 1, 4,5 -> 2
            float currentAngle = flankingAngle + (pairIndex * 20f); // Each pair spreads 20 degrees more
            currentAngle = Mathf.Min(currentAngle, 80f); // Cap at 80 degrees to stay in front-ish
            
            // Convert angle to radians
            float angleRad = currentAngle * Mathf.Deg2Rad;
            
            // Calculate the flanking direction by rotating the front direction
            Vector3 flankDirection;
            if (goRight)
            {
                // Rotate right (clockwise when viewed from above)
                flankDirection = Quaternion.Euler(0, currentAngle, 0) * victimToAttacker;
            }
            else
            {
                // Rotate left (counter-clockwise)
                flankDirection = Quaternion.Euler(0, -currentAngle, 0) * victimToAttacker;
            }
            
            // Normalize to be safe
            flankDirection.Normalize();
            
            // Calculate distance from victim - spread units out
            float distanceFromVictim = minDistanceFromAlly + (pairIndex * unitSpacing);
            distanceFromVictim = Mathf.Clamp(distanceFromVictim, minDistanceFromAlly, maxDistanceFromAlly);
            
            // Calculate the final position
            // Position is to the side of the victim, facing towards where the attacker is
            Vector3 sideOffset = sideDirection * (goRight ? 1f : -1f) * (minDistanceFromAlly + pairIndex * unitSpacing * 0.5f);
            Vector3 forwardOffset = victimToAttacker * (distanceFromVictim * 0.5f); // Slightly forward towards attacker
            
            Vector3 flankPosition = victim.Position + sideOffset + forwardOffset;
            
            // Keep the Y position same as the ally's current Y (ground level)
            flankPosition.y = ally.Position.y;
            
            if (enableDebugLogging)
                Debug.Log($"AlliedSupportSystem: Calculated flank position for responder {responderIndex}: {flankPosition} (side: {(goRight ? "right" : "left")}, angle: {currentAngle})");
            
            return flankPosition;
        }
        
        /// <summary>
        /// Get the CombatUnit component for an IUnit
        /// </summary>
        private CombatUnit GetCombatUnit(IUnit unit)
        {
            if (unit is CombatUnit combatUnit)
                return combatUnit;
            
            if (unit is MonoBehaviour mono)
                return mono.GetComponent<CombatUnit>();
            
            return null;
        }
        
        private void Update()
        {
            // Clean up old alert visualizations (manual loop to avoid lambda allocation)
            if (drawDebugGizmos && recentAlerts.Count > 0)
            {
                float currentTime = Time.time;
                for (int i = recentAlerts.Count - 1; i >= 0; i--)
                {
                    if (currentTime - recentAlerts[i].time > alertVisualizationDuration)
                    {
                        recentAlerts.RemoveAt(i);
                    }
                }
            }
            
            // Clean up old cooldown entries (prevent memory leak)
            CleanupCooldowns();
        }
        
        private float lastCleanupTime = 0f;
        private void CleanupCooldowns()
        {
            if (Time.time - lastCleanupTime < 10f) return;
            lastCleanupTime = Time.time;
            
            float currentTime = Time.time;
            
            // Use cached list to avoid GC allocation
            cachedCleanupList.Clear();
            
            // Cleanup victim alert times
            foreach (var kvp in lastAlertTimes)
            {
                if (kvp.Key == null || currentTime - kvp.Value > alertCooldown * 2)
                {
                    cachedCleanupList.Add(kvp.Key);
                }
            }
            
            for (int i = 0; i < cachedCleanupList.Count; i++)
            {
                lastAlertTimes.Remove(cachedCleanupList[i]);
            }
            
            // Cleanup responder tracking
            cachedCleanupList.Clear();
            foreach (var kvp in responderLastResponseTime)
            {
                if (kvp.Key == null || currentTime - kvp.Value > responderRedirectCooldown * 2)
                {
                    cachedCleanupList.Add(kvp.Key);
                }
            }
            
            for (int i = 0; i < cachedCleanupList.Count; i++)
            {
                responderLastResponseTime.Remove(cachedCleanupList[i]);
                responderCurrentAssignment.Remove(cachedCleanupList[i]);
            }
        }
        
        /// <summary>
        /// Manually alert nearby allies (can be called from other systems)
        /// </summary>
        public void RequestSupport(IUnit requestingUnit, IUnit threat)
        {
            if (requestingUnit == null || threat == null) return;
            
            NotifyUnitAttacked(requestingUnit, threat, minDamageToAlert + 1);
        }
        
        /// <summary>
        /// Get the alert radius for UI/debugging
        /// </summary>
        public float GetAlertRadius() => alertRadius;
        
        /// <summary>
        /// Set the alert radius dynamically
        /// </summary>
        public void SetAlertRadius(float radius)
        {
            alertRadius = Mathf.Max(1f, radius);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!drawDebugGizmos) return;
            
            // Draw recent alerts
            foreach (var alert in recentAlerts)
            {
                float alpha = 1f - (Time.time - alert.time) / alertVisualizationDuration;
                
                // Alert radius around victim
                Gizmos.color = new Color(0f, 1f, 0f, alpha * 0.3f);
                Gizmos.DrawWireSphere(alert.victimPosition, alertRadius);
                
                // Line from victim to attacker
                Gizmos.color = new Color(1f, 0f, 0f, alpha);
                Gizmos.DrawLine(alert.victimPosition, alert.attackerPosition);
                
                // Indicator at victim position
                Gizmos.color = new Color(1f, 1f, 0f, alpha);
                Gizmos.DrawWireSphere(alert.victimPosition, 1f);
                
                // Draw flanking zones (min and max distance)
                Gizmos.color = new Color(0f, 0.5f, 1f, alpha * 0.2f);
                Gizmos.DrawWireSphere(alert.victimPosition, minDistanceFromAlly);
                Gizmos.color = new Color(0f, 0.5f, 1f, alpha * 0.1f);
                Gizmos.DrawWireSphere(alert.victimPosition, maxDistanceFromAlly);
                
                // Draw example flanking positions
                Vector3 victimToAttacker = (alert.attackerPosition - alert.victimPosition).normalized;
                Vector3 sideDirection = Vector3.Cross(victimToAttacker, Vector3.up).normalized;
                
                // Left flank position
                Vector3 leftFlank = alert.victimPosition + sideDirection * -minDistanceFromAlly + victimToAttacker * (minDistanceFromAlly * 0.5f);
                // Right flank position
                Vector3 rightFlank = alert.victimPosition + sideDirection * minDistanceFromAlly + victimToAttacker * (minDistanceFromAlly * 0.5f);
                
                Gizmos.color = new Color(0f, 1f, 1f, alpha);
                Gizmos.DrawWireSphere(leftFlank, 0.8f);
                Gizmos.DrawWireSphere(rightFlank, 0.8f);
            }
        }
    }
}
