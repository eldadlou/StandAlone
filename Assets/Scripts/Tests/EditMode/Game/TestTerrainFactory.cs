using UnityEngine;

namespace MyGame.Tests.Game
{
    internal static class TestTerrainFactory
    {
        public static Terrain CreateFlatTerrain(Vector3 position, Vector3 size)
        {
            var data = new TerrainData
            {
                heightmapResolution = 33,
                size = size
            };

            var go = Terrain.CreateTerrainGameObject(data);
            go.name = "TestTerrain";
            go.transform.position = position;
            return go.GetComponent<Terrain>();
        }
    }
}
