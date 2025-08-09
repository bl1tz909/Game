using PurrNeT;
using PurrNeT.Packing;
using UnityEngine;

public static class InventoryDataUtils
{
    public static void ItemPresetWriter(BitPacker packer, ItemPreset preset)
    {
        if (preset == null)
        {
            Packer<bool>.Write(packer, false);
            return;
        }

        Packer<bool>.Write(packer, true);
        Packer<string>.Write(packer, preset.uid);
    }

    public static void ItemPresetReader(BitPacker packer, ref ItemPreset preset)
    {
        bool hasPreset = false;
        Packer<bool>.Read(packer, ref hasPreset);

        if (!hasPreset)
        {
            preset = null;
            return;
        }

        if (!InstanceHandler.TryGetInstance(out ItemDatabase database))
        {
            Debug.LogError("Failed to get items database instance for reading ItemPreset!");
            return;
        }

        var uid = default(string);
        Packer<string>.Read(packer, ref uid);

        database.TryGetItemPreset(uid, out preset);

        // Optional: add this if you want to log when the preset is not found
        // if (preset == null)
        // {
        //     Debug.LogWarning($"ItemPreset with UID '{uid}' not found.");
        // }
    }
}