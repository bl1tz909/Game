using System.Collections.Generic;
using PurrNeT;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    private Dictionary<string, ItemPreset> _itemPreset = new();

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);

        var presets = Resources.LoadAll<ItemPreset>(path: "");
        foreach (var preset in presets)
        {
            if (!_itemPreset.TryAdd(preset.uid, preset))
            {
                Debug.LogError($"Duplicate item preset UID found ({preset.uid})");
            }
        }
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<ItemDatabase>();
    }

    public bool TryGetItemPreset(string uid, out ItemPreset preset)
    {
        if (string.IsNullOrEmpty(uid))
        {
            preset = null;
            return false;
        }

        return _itemPreset.TryGetValue(uid, out preset);
    }
}