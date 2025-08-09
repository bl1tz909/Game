using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChunkLoader : MonoBehaviour
{
    [System.Serializable]
    public class RingConfig
    {
        public int ringNumber;
        public int chunksToSpawn;
        public int poiChunksToSpawn;
        public Vector2Int[] gridPositions;
        public BiomeType biome;
    }

    public List<RingConfig> ringConfigs = new List<RingConfig>();
    public bool proceduralFallbackEnabled = true;

    [SerializeField] private float chunkSize = 127.5f;

    // Track chunk instances -> ids so we can unregister when unloading
    private readonly Dictionary<GameObject, int> chunkToId = new();

    void Start()
    {
        LoadChunksForAllRings();
    }

    void LoadChunksForAllRings()
    {
        GameObject[] allPrefabs = Resources.LoadAll<GameObject>("WorldChunks");
        GameObject[] proceduralChunks = Resources.LoadAll<GameObject>("TerrainBases");

        Debug.Log($"Loaded {proceduralChunks.Length} procedural chunks");

        foreach (var config in ringConfigs)
        {
            var matchingPrefabs = allPrefabs
                .Where(p =>
                {
                    var info = p.GetComponent<ChunkInfo>();
                    return info != null && info.ring == config.ringNumber && info.chunkType == ChunkType.POI;
                })
                .ToList();

            var shuffledPositions = config.gridPositions.OrderBy(_ => Random.value).Take(config.chunksToSpawn).ToList();
            var poiPrefabs = matchingPrefabs.OrderBy(_ => Random.value).Take(config.poiChunksToSpawn).ToList();

            for (int i = 0; i < config.chunksToSpawn; i++)
            {
                Vector2Int pos = shuffledPositions[i];
                GameObject chunkPrefab;

                if (i < config.poiChunksToSpawn && i < poiPrefabs.Count)
                {
                    chunkPrefab = poiPrefabs[i];
                    Debug.Log($"Spawning POI for ring {config.ringNumber} at {pos}");
                }
                else if (proceduralFallbackEnabled && proceduralChunks.Length > 0)
                {
                    chunkPrefab = proceduralChunks[Random.Range(0, proceduralChunks.Length)];
                    Debug.Log($"Spawning procedural chunk for ring {config.ringNumber} at {pos}");
                }
                else
                {
                    Debug.LogWarning($"No chunk prefab found for ring {config.ringNumber} at index {i}");
                    continue;
                }

                Vector3 worldPos = GridToWorld(pos);
                GameObject instance = Instantiate(chunkPrefab, worldPos, Quaternion.identity);

                // Use the INSTANCE's ChunkInfo (not the prefab’s) 
                var info = instance.GetComponent<ChunkInfo>();

                // Make a stable id from the integer grid coords
                int chunkId = MakeChunkId(pos);
                chunkToId[instance] = chunkId;

                // Only decorate procedural chunks
                if (info != null)
                {
                    info.biome = config.biome; // tag the instance for reference
                    Debug.Log($"[ChunkLoader] Decorating {instance.name} at {worldPos} for biome {config.biome}");

                    // UPDATED: pass chunkId to enable cross‑chunk spacing
                    ChunkDecorator.Decorate(chunkPrefab, instance, worldPos, chunkSize, config.biome, chunkId);
                }
            }
        }
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        // Assumes worldPos is the bottom-left of the chunk footprint.
        // If your chunks are centered on their transform, adjust by -chunkSize * 0.5f on X/Z.
        return new Vector3(gridPos.x * chunkSize, 0f, gridPos.y * chunkSize);
    }

    private static int MakeChunkId(Vector2Int gridPos)
    {
        // Stable hash from integer grid cell
        unchecked { return (gridPos.x * 73856093) ^ (gridPos.y * 19349663); }
    }

    // Call this when you remove a chunk instance
    public void UnloadChunk(GameObject instance)
    {
        if (instance == null) return;

        if (chunkToId.TryGetValue(instance, out var id))
        {
            ChunkDecorator.UnregisterChunk(id);
            chunkToId.Remove(instance);
        }
        Destroy(instance);
    }

    // Example bulk unload (optional helper)
    public void UnloadAllChunks()
    {
        foreach (var kv in new List<KeyValuePair<GameObject, int>>(chunkToId))
        {
            ChunkDecorator.UnregisterChunk(kv.Value);
            if (kv.Key) Destroy(kv.Key);
        }
        chunkToId.Clear();
    }
}
