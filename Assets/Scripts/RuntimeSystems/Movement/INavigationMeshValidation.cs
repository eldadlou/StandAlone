using UnityEngine;

namespace MyGame.RuntimeSystems.Movement
{
    /// <summary>
    /// NavMesh-aware position validation (implemented by <see cref="PathfindingSystem"/>).
    /// </summary>
    public interface INavigationMeshValidation
    {
        Vector3 GetNearestValidPosition(Vector3 position, float maxDistance = 10f);
    }
}
