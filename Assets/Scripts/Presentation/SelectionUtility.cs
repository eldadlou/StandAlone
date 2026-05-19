using System.Collections.Generic;
using MyGame.Core;
using MyGame.Core.Events;
using MyGame.Core.Services;
using MyGame.Core.Units;
using UnityEngine;

namespace MyGame.Presentation
{
    /// <summary>
    /// Raycasts and unit queries used by single- and multi-select.
    /// </summary>
    public static class SelectionUtility
    {
        private const float MaxRayDistance = 5000f;

        public static bool IsPlayerSelectable(IUnit unit)
        {
            if (unit == null)
                return false;

            if (unit.Owner != null)
                return unit.Owner.Team == Team.Player;

            // Fallback when Owner is not set yet: layer 6 = Player
            if (unit is Component component)
                return component.gameObject.layer == 6;

            return false;
        }

        public static bool TryGetSelectableAtScreen(Vector2 screenPosition, out ISelectableUnit selectable)
        {
            selectable = null;
            var camera = Camera.main;
            if (camera == null)
                return false;

            var ray = camera.ScreenPointToRay(screenPosition);
            var hits = Physics.RaycastAll(ray, MaxRayDistance, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            if (hits == null || hits.Length == 0)
                return false;

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                var unit = FindSelectableOnTransform(hit.collider.transform);
                if (unit != null)
                {
                    selectable = unit;
                    return true;
                }
            }

            return false;
        }

        public static List<IUnit> GetSelectableUnitsInScene()
        {
            var fromEvents = GameEvents.GetAllUnits();
            if (fromEvents != null && fromEvents.Count > 0)
                return FilterPlayerUnits(fromEvents);

            var fallback = new List<IUnit>();
            var behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IUnit unit && IsPlayerSelectable(unit))
                    fallback.Add(unit);
            }

            return fallback;
        }

        public static List<ISelectableUnit> GetUnitsInScreenRect(Rect screenRect, Camera camera)
        {
            var result = new List<ISelectableUnit>();
            if (camera == null)
                return result;

            foreach (var unit in GetSelectableUnitsInScene())
            {
                if (unit is not ISelectableUnit selectable)
                    continue;

                var screenPos = camera.WorldToScreenPoint(unit.Position);
                if (screenPos.z < 0f)
                    continue;

                if (screenRect.Contains(new Vector2(screenPos.x, screenPos.y)))
                    result.Add(selectable);
            }

            return result;
        }

        private static List<IUnit> FilterPlayerUnits(List<IUnit> units)
        {
            var filtered = new List<IUnit>(units.Count);
            foreach (var unit in units)
            {
                if (IsPlayerSelectable(unit))
                    filtered.Add(unit);
            }

            return filtered;
        }

        /// <summary>
        /// World point on terrain/ground for move commands (skips unit colliders).
        /// </summary>
        public static bool TryGetMoveTargetAtScreen(Vector2 screenPosition, out Vector3 worldPoint)
        {
            worldPoint = Vector3.zero;
            var camera = Camera.main;
            if (camera == null)
                return false;

            var ray = camera.ScreenPointToRay(screenPosition);
            var hits = Physics.RaycastAll(ray, MaxRayDistance, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            if (hits != null && hits.Length > 0)
            {
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                foreach (var hit in hits)
                {
                    if (IsUnitLayer(hit.collider.gameObject.layer))
                        continue;

                    worldPoint = hit.point;
                    return true;
                }
            }

            return TrySampleTerrainUnderScreen(screenPosition, out worldPoint);
        }

        private static bool IsUnitLayer(int layer) => layer == 6 || layer == 7;

        private static bool TryGetTerrain(Terrain assignedTerrain, out Terrain terrain)
        {
            terrain = assignedTerrain;
            if (terrain != null)
                return true;

            terrain = Terrain.activeTerrain;
            if (terrain != null)
                return true;

            var terrains = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            if (terrains == null || terrains.Length == 0)
            {
                terrain = null;
                return false;
            }

            terrain = terrains[0];
            return true;
        }

        private static Vector3 SampleTerrainSurface(Terrain terrain, float worldX, float worldZ)
        {
            var samplePos = new Vector3(worldX, 0f, worldZ);
            var height = terrain.SampleHeight(samplePos) + terrain.transform.position.y;
            return new Vector3(worldX, height, worldZ);
        }

        private static bool TrySampleTerrainUnderScreen(Vector2 screenPosition, out Vector3 worldPoint)
        {
            worldPoint = Vector3.zero;
            if (!TryGetTerrain(null, out var terrain))
                return false;

            var camera = Camera.main;
            if (camera == null)
                return false;

            var ray = camera.ScreenPointToRay(screenPosition);
            var plane = new Plane(Vector3.up, terrain.transform.position);
            if (!plane.Raycast(ray, out var distance))
                return false;

            var point = ray.GetPoint(distance);
            worldPoint = SampleTerrainSurface(terrain, point.x, point.z);
            return true;
        }

        private static ISelectableUnit FindSelectableOnTransform(Transform hitTransform)
        {
            var current = hitTransform;
            while (current != null)
            {
                var behaviours = current.GetComponents<MonoBehaviour>();
                foreach (var behaviour in behaviours)
                {
                    if (behaviour is ISelectableUnit selectable
                        && behaviour is IUnit unit
                        && IsPlayerSelectable(unit))
                    {
                        return selectable;
                    }
                }

                current = current.parent;
            }

            return null;
        }
    }
}
