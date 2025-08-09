using UnityEngine;

public class PrefabTest : MonoBehaviour
{
    void Start()
    {
        var prefabs = Resources.LoadAll<GameObject>("EnvironmentAssets/Forest/LargeTrees");
        Debug.Log($"Manual test found {prefabs.Length} prefabs in LargeTrees");
        foreach (var p in prefabs)
        {
            Debug.Log($"Found prefab: {p.name}");
        }
    }
}
