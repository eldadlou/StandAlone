using UnityEngine;
using System;

namespace MyGame.Core.Units.Combat
{
    /// <summary>
    /// Interface for units that have combat capabilities
    /// Each combat unit manages its own targeting, rotation, and firing
    /// </summary>
    public interface ICombatUnit : IUnit
    {
        // Combat state
        bool IsInCombat { get; }
        IUnit CurrentTarget { get; }
        bool IsTargetInRange { get; }
        bool IsGunFacingTarget { get; }
         
        // Combat methods
        void SetTarget(IUnit target);
        void ClearTarget();
        bool TryAttack();
        void UpdateCombat();
        
        // Detection
        float DetectionRadius { get; }
        LayerMask EnemyLayerMask { get; }
        
        // Gun/Turret control
        float GunRotationSpeed { get; }
        float RotationThreshold { get; }
        void RotateGunTowardsTarget();
    }
}
