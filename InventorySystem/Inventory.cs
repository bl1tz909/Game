using System.Threading.Tasks;
using PurrNet;
using UnityEngine;

public abstract class Inventory : NetworkBehaviour
{
    [SerializeField] protected SyncArray<InventoryItem> _inventoryItems = new();
    
    public virtual async Task<bool> TryAddItem(ItemPreset preset, int quantity)
    {
        if (TryStack(preset, quantity))
            return true;

        return TryAddNewItem(preset, quantity);
    }

    private bool TryStack(ItemPreset preset, int quantity)
    {
        for (int i = 0; i < _inventoryItems.Length; i++)
        {
            var invItem = _inventoryItems[i];
            if (invItem.preset != preset)
                continue;

            invItem.quantity += quantity;
            _inventoryItems[i] = invItem;
            return true;
        }

        return false;
    }

    private bool TryAddNewItem(ItemPreset preset, int quantity)
    {
        for (int i = 0; i < _inventoryItems.Length; i++)
        {
            var invItem = _inventoryItems[i];
            if (invItem.preset)
                continue;

            invItem.preset = preset;
            invItem.quantity = quantity;
            _inventoryItems[i] = invItem;
            return true;
        }

        return false;
    }

    public virtual void Interact(int index)
    {

    }

    public virtual void DropItem(int index, int amount)
    {
        if (index < 0 || index >= inventoryItems.Count)
        {
            Debug.LogError($"Invalid index {index} for inventory items", context: this);
            return;
        }

        var invItem = inventoryItems[index];
        if (invItem.preset == null)
            return;

        amount = Mathf.Min(amount, invItem.quantity);
        for (int i = 0; i < amount; i++)
            SpawnItem(invItem.preset.prefab);
        RemoveItem(index, amount);
    }

    protected virtual void RemoveItem(int index, int amount)
    {
        var invItem = inventoryItems[index];
        invItem.quantity -= amount;

        if (invItem.quantity <= 0)
            invItem.preset = null;

        inventoryItems[index] = invItem;
    }

        private void SpawnItem(InventoryItem item)
    {
        Instantiate(item, position: Random.insideUnitCircle * 3, Quaternion.identify);
    

        for (int i = 0; i < amount; i++)
        {
            SpawnItem(invItem.preset.prefab);
        }

        RemoveItem(index, amount);
    }



    public struct InventoryItem
    {
        public ItemPreset preset;
        public int quantity;
    }
}