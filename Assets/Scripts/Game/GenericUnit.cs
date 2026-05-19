using UnityEngine;
using MyGame.Core.Units;

namespace MyGame.Core.Units
{
    /// <summary>
    /// Configurable <see cref="Unit"/> for prefabs that are not Tank/Truck (soldier, aircraft, etc.).
    /// Add this plus <see cref="Combat.VehicleCombatUnit"/> on model prefabs used by <see cref="UnitPoolManager"/>.
    /// </summary>
    public class GenericUnit : Unit
    {
        [SerializeField] private UnitType unitType = UnitType.Soldier;
        [SerializeField] private float initialHealth = 100f;
        [SerializeField] private float initialSpeed = 5f;

        public override UnitType Type => unitType;

        protected override float GetInitialHealth() => initialHealth;
        protected override float GetInitialSpeed() => initialSpeed;
    }
}
