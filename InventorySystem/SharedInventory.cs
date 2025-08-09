using System;
using PurrNet;
using UnityEngine;

public class SharedInventory : Inventory
{
    [SerializeField] private InventoryView _inventoryView;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InstanceHandler.UnregisterInstance<SharedInventory>();
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        _inventoryView.Init(inventory: this);
        inventoryItems.onChanged += OnInventoryChanged;
    }

    protected override void OnDespawned()
    {
        base.OnDespawned();
        inventoryItems.onChanged -= OnInventoryChanged;
    }

    [ServerRpc(requireOwnership: false)]
    public async override Task<bool> TryAddItem(ItemPreset preset, int quantity)
    {
        return await base.TryAddItem(preset, quantity);
    }

    private void OnInventoryChanged(SyncArrayChange<InventoryItem> change)
    {
        _inventoryView.RedrawEverything(inventoryItems.ToArray());
    }

    public async override void Interact(int index)
    {
        if (!InstanceHandler.TryGetInstance(out PlayerInventory playerInventory))
        {
            Debug.LogError($"Failed to get shared inventory", context: this);
            return;
        }

        if (inventoryItems[index].preset == null)
            return;

        if (await playerInventory.TryAddItem(inventoryItems[index].preset, quantity: 1))
        {
            RemoveItem(index, amount: 1);
        }
    }

    public override void DropItem(int index, int amount)
    {
        // Implement drop logic here or override from base Inventory
    }

    [ServerRpc(requiredOwnership:false)]
    protected override void DropItem(int index, int amount)
    {
        base.RemoveItem(index, amount);
    }
        

}
