using UnityEngine;
using MyGame.Core.Units;
using MyGame.Core.Units.Combat;

namespace MyGame.RuntimeSystems.Combat
{
    /// <summary>
    /// Global combat coordination for fire/projectile flow.
    /// </summary>
    public interface ICombatFireCoordinator
    {
        void RegisterCombatUnit(ICombatUnit combatUnit);
        void UnregisterCombatUnit(ICombatUnit combatUnit);
        bool ProcessAttack(IUnit attacker, IUnit target);
        void OnProjectileHit(IUnit target, Vector3 hitPosition, IUnit attacker);
    }

    /// <summary>
    /// Notifies nearby allies when a friendly unit is attacked.
    /// </summary>
    public interface IAlliedCombatSupport
    {
        void NotifyUnitAttacked(IUnit victim, IUnit attacker, float damage);
    }

    /// <summary>
    /// Centralized target acquisition for <see cref="ICombatUnit"/> instances.
    /// </summary>
    public interface ICentralizedCombatDetection
    {
        void RegisterCombatUnit(ICombatUnit combatUnit);
        void UnregisterCombatUnit(ICombatUnit combatUnit);
        IUnit GetCurrentTarget(ICombatUnit combatUnit);
    }
}
