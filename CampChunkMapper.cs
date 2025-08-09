using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CampChunkMapper : MonoBehaviour
{
    public float chunkSize = 127.5f;
    public int maxRing = 5;

    private void OnDrawGizmos()
    {
        Vector3 campWorld = transform.position;
        Gizmos.color = Color.green;

        for (int ring = 1; ring <= maxRing; ring++)
        {
            foreach (var pos in GetRingPositions(ring))
            {
                Vector3 worldPos = campWorld + new Vector3(pos.x * chunkSize, 0f, pos.y * chunkSize);
                Gizmos.DrawWireCube(worldPos, new Vector3(chunkSize, 1f, chunkSize));
            }
        }

        // Draw center camp chunk
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(campWorld, new Vector3(chunkSize, 1f, chunkSize));
    }

    private List<Vector2Int> GetRingPositions(int ring)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int x = -ring; x <= ring; x++)
        {
            for (int y = -ring; y <= ring; y++)
            {
                if (Mathf.Abs(x) == ring || Mathf.Abs(y) == ring)
                {
                    if (x != 0 || y != 0)
                        positions.Add(new Vector2Int(x, y));
                }
            }
        }

        return positions;
    }

    [ContextMenu("Export to ChunkLoader")]
    public void ExportToChunkLoader()
    {
        ChunkLoader loader = FindFirstObjectByType<ChunkLoader>();
        if (loader == null)
        {
            Debug.LogError("No ChunkLoader found in scene.");
            return;
        }

        List<ChunkLoader.RingConfig> configs = new List<ChunkLoader.RingConfig>();

        for (int ring = 1; ring <= maxRing; ring++)
        {
            ChunkLoader.RingConfig config = new ChunkLoader.RingConfig
            {
                ringNumber = ring,
                gridPositions = GetRingPositions(ring).ToArray(),
                chunksToSpawn = GetRingPositions(ring).Count
            };

            configs.Add(config);
        }

        loader.ringConfigs = configs;
        Debug.Log("Exported ring configs to ChunkLoader.");
    }
}
