using PurrNet;
using UnityEngine;
public class PlayerInventory : Inventory
{
    [SerializeField] private string _viewName;
    private InventoryView _inventoryView;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        _inventoryView = InventoryView.instances[_viewName];

        if (!IsOwner)
            return;

        InstanceHandler.RegisterInstance(this);
        _inventoryView.Init(inventory: this);
        inventoryItems.onChanged += OnInventoryChanged;
    }

    protected override void OnDespawned()
    {
        base.OnDespawned();

        if (!IsOwner)
            return;

        _inventoryItems.onChanged -= OnInventoryChanged;
        InstanceHandler.UnregisterInstance<PlayerInventory>();
    }
    private void OnInventoryChanged(SyncArrayChange<InventoryItem> change)
    {
        if (_inventoryView != null && inventoryITems != null)
            _inventoryView.RedrawEverything(_inventoryItems.ToArray());
    }

public async override void Interact(int index)
    {
        if (inventoryItems[index].preset == null)
            return;

        if (!InstanceHandler.TryGetInstance(out SharedInventory sharedInventory))
        {
            Debug.LogError($"Failed to get shared inventory", context: this);
            return;
        }

        if (sharedInventory.TryAddItem(inventoryItems[index].preset, quantity: 1))
        {
            RemoveItem(index, amount: 1);
        }
    }
}



//            Debug.Log($"Interacted with index: {inventoryItems[index].preset.itemName}", context: this);
