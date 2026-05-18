using System.Collections.Generic;
using UnityEngine;
using MyGame.Core.Units;

namespace MyGame.Core.Services
{
    /// <summary>
    /// Read-only spatial queries for units (implemented by the spatial grid service).
    /// </summary>
    public interface ISpatialUnitQuery
    {
        List<IUnit> GetUnitsInRadius(Vector3 position, float radius);
    }
}
