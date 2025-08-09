using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class ChunkDecorator
{
    public static bool EnableSlopeCheck = false;
    public static bool EnableCollisionCheck = true;

    private struct Entry { public Vector3 pos; public float radius; public Entry(Vector3 p, float r) { pos = p; radius = r; } }

    private static readonly float CellSize = 10f;
    private static readonly Dictionary<Vector2Int, List<Entry>> GlobalCells = new();
    private static readonly Dictionary<int, List<Entry>> ChunkToEntries = new();

    private static Vector2Int CellAt(Vector3 p)
        => new Vector2Int(Mathf.FloorToInt(p.x / CellSize), Mathf.FloorToInt(p.z / CellSize));

    private static bool TooCloseGlobal(Vector3 p, float minSpacing)
    {
        var c = CellAt(p);
        for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
            {
                var n = new Vector2Int(c.x + dx, c.y + dz);
                if (!GlobalCells.TryGetValue(n, out var list)) continue;
                foreach (var e in list)
                    if (Vector3.Distance(p, e.pos) < Mathf.Max(minSpacing, e.radius)) return true;
            }
        return false;
    }

    private static void RegisterPoint(int chunkId, Vector3 p, float radius)
    {
        var cell = CellAt(p);
        if (!GlobalCells.TryGetValue(cell, out var list))
            GlobalCells[cell] = list = new List<Entry>();
        var e = new Entry(p, radius);
        list.Add(e);

        if (!ChunkToEntries.TryGetValue(chunkId, out var mine))
            ChunkToEntries[chunkId] = mine = new List<Entry>();
        mine.Add(e);
    }

    public static void UnregisterChunk(int chunkId)
    {
        if (!ChunkToEntries.TryGetValue(chunkId, out var mine)) return;
        foreach (var e in mine)
        {
            var cell = CellAt(e.pos);
            if (GlobalCells.TryGetValue(cell, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i].pos == e.pos) { list.RemoveAt(i); break; }
                if (list.Count == 0) GlobalCells.Remove(cell);
            }
        }
        ChunkToEntries.Remove(chunkId);
    }

    [System.Serializable]
    public class DecorationConfig
    {
        public string label;
        public string resourcePath;
        public int minCount = 100;
        public int maxCount;
        public float minScale = 1f;
        public float maxScale = 1f;
        public float minSlope = 0f;
        public float maxSlope = 30f;
        public bool isStaticObstacle = false;
        public float minSpacing = 10f;
    }

    private static List<DecorationConfig> decorationConfigs = new()
    {
        new DecorationConfig { label = "Trees", resourcePath = "EnvironmentAssets/Trees", minCount = 50, maxCount = 75, minSpacing = 5f, minScale = 1f, maxScale = 1f },
        new DecorationConfig { label = "LargeTrees", resourcePath = "EnvironmentAssets/LargeTrees", minCount = 40, maxCount = 60, minScale = 3.5f, maxScale = 4f, minSpacing = 10f, isStaticObstacle = true },
        new DecorationConfig { label = "Stumps", resourcePath = "EnvironmentAssets/Stumps", minCount = 3, maxCount = 10, minSpacing = 20f, isStaticObstacle = true },
        new DecorationConfig { label = "Plants", resourcePath = "EnvironmentAssets/Plants", minCount = 20, maxCount = 50, minSpacing = 20f, isStaticObstacle = true },
        new DecorationConfig { label = "Mushrooms", resourcePath = "EnvironmentAssets/Mushrooms", minCount = 5, maxCount = 20, minSpacing = 5f },
        new DecorationConfig { label = "Grass", resourcePath = "EnvironmentAssets/Grass", minCount = 100, maxCount = 200, minSpacing = 5f, minScale = 0.5f, maxScale = 1f, isStaticObstacle = true },
        new DecorationConfig { label = "Flowers", resourcePath = "EnvironmentAssets/Flowers", minCount = 20, maxCount = 35, minSpacing = 5f, isStaticObstacle = true },
        new DecorationConfig { label = "Branches", resourcePath = "EnvironmentAssets/Branches", minCount = 20, maxCount = 30, minSpacing = 1f },
        new DecorationConfig { label = "Bushes", resourcePath = "EnvironmentAssets/Bushes", minCount = 50, maxCount = 70, minSpacing = 10f },
        new DecorationConfig { label = "SmallRocks", resourcePath = "EnvironmentAssets/SmallRocks", minCount = 10, maxCount = 20, minSpacing = 10f },
        new DecorationConfig { label = "RockWithGrass", resourcePath = "EnvironmentAssets/RockWithGrass", minCount = 100, maxCount = 300, minSpacing = 10f, isStaticObstacle = true },
    };

    private static float ComputeBorderInset(float chunkSize)
        => Mathf.Max(0.05f, chunkSize * 0.001f);

    public static void Decorate(GameObject chunkPrefab, GameObject instance, Vector3 origin, float chunkSize, BiomeType biome, int chunkId)
    {
        // Gather exclusion markers by name
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        List<(Vector3 pos, float radius)> exclusionZones = new();

        foreach (var obj in allObjects)
        {
            if (obj.name == "exclusionMarker")
            {
                var drawer = obj.GetComponent<CampfireExclusionZoneDrawer>();
                if (drawer != null)
                    exclusionZones.Add((obj.transform.position, drawer.exclusionRadius));
                else
                    Debug.LogWarning($"[ChunkDecorator] exclusionMarker found but missing CampfireExclusionZoneDrawer.");
            }
        }

        // Setup terrain-only and obstacle layer masks
        LayerMask terrainMask = LayerMask.GetMask("Terrain");
        //LayerMask terrainMask = ~0; // all layers, for testing

        LayerMask obstacleMask = LayerMask.GetMask("Obstacles", "Decorations");

        foreach (var config in decorationConfigs)
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>($"EnvironmentAssets/{biome}/{config.label}");
            if (prefabs.Length == 0)
                prefabs = Resources.LoadAll<GameObject>($"EnvironmentAssets/{config.label}");

            if (prefabs.Length == 0)
            {
                Debug.LogWarning($"[ChunkDecorator] No prefabs found for '{config.label}' (biome {biome} or fallback).");
                continue;
            }

            int toPlace = Random.Range(config.minCount, config.maxCount + 1);
            List<Vector3> placedPositions = new();
            int placed = 0, attempts = 0;

            float inset = ComputeBorderInset(chunkSize);
            float half = chunkSize * 0.5f;
            float min = -half + inset;
            float max = half - inset;

            while (placed < toPlace && attempts < toPlace * 10)
            {
                attempts++;

                Vector3 local = new Vector3(
                    Random.Range(min, max),
                    0f,
                    Random.Range(min, max)
                );
                Vector3 worldPos = origin + local;

                if (exclusionZones.Any(zone => Vector3.Distance(worldPos, zone.pos) < zone.radius))
                    continue;

                if (!Physics.Raycast(worldPos + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f, terrainMask))
                    continue;

               // if (hit.collider == null || !(hit.collider is TerrainCollider))
               //     continue;

                float slope = Vector3.Angle(hit.normal, Vector3.up);
                if (EnableSlopeCheck && (slope < config.minSlope || slope > config.maxSlope))
                    continue;

                if (EnableCollisionCheck && Physics.CheckSphere(hit.point, config.minSpacing * 0.5f, obstacleMask))
                    continue;

                if (placedPositions.Any(p => Vector3.Distance(hit.point, p) < config.minSpacing))
                    continue;

                if (TooCloseGlobal(hit.point, config.minSpacing))
                    continue;

                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject placedObj = Object.Instantiate(
                    prefab,
                    hit.point,
                    Quaternion.Euler(0, Random.Range(0, 360), 0)
                );

                if (placedObj == null) continue;

                float scale = Random.Range(config.minScale, config.maxScale);
                placedObj.transform.localScale = Vector3.one * scale;
                placedObj.transform.parent = instance.transform;

                placedPositions.Add(hit.point);
                RegisterPoint(chunkId, hit.point, config.minSpacing);
                placed++;
            }
        }
    }
}
