using UnityEngine;

public enum ChunkType { Camp, POI, Procedural }
public enum BiomeType { Forest, Swamp, Burnt, Snow }

public class ChunkInfo : MonoBehaviour
{
    public int ring = 0;
    public string chunkID = "000";
    public ChunkType chunkType = ChunkType.POI;
    public BiomeType biome = BiomeType.Forest;
}
