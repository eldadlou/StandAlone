using UnityEngine;
using MyGame.Core.Interfaces;

namespace MyGame.Core.Services
{
    public interface IExplosionService
    {
        void CreateExplosion(Vector3 position, float radius, float damage, IDestructible source = null);
        void SubscribeToDestructible(IDestructible destructible);
    }
}
