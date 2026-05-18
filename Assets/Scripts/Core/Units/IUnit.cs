using UnityEngine;
using System;
using MyGame.Core.Interfaces;
using MyGame.Game;

namespace MyGame.Core.Units
{
    public interface IUnit : IMovable, IAttackable, ISkillUser, IAnimatable
    {
        event Action<IUnit> OnDeath;
        event Action<IUnit, IUnit> OnAttack; // Attacker, Target
        event Action<IUnit, Vector3> OnMove; // Unit, Destination

        void Upgrade();
        UnitType Type { get; }
        Player Owner { get; }
        
        string Name { get; }
        new float Health { get; }
        Vector3 Position { get; }
        
        // Team management
        void AssignToTeam(Team team);
        
        // Combat methods
        bool CanAttack(IUnit target);
        bool Attack(IUnit target);
        
        // Combat properties
        float AttackDamage { get; }
        float AttackRange { get; }
        float AttackCooldown { get; }
        float LastAttackTime { get; }
    }
}
