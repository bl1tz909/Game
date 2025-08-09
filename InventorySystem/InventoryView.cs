using UnityEngine;
public class InventoryView : MonoBehaviour
{

    [SerializeField] private string _viewName;
    [SerializeField] private InventoryTile[] _inventoryTiles;

    public static Dictionary<string, InventoryView> instances = new();

    private Inventory _inventory;

    private void Awake()
    {
        for (int i = 0; i < _inventoryTiles.Length; i++)
        {
            _inventoryTiles[i].Init(inventoryView: this, i);
        }
    }

    private void OnDestroy()
    {
        instances.Remove(_viewName);
    }

    public void Init(Inventory inventory)
    {
        _inventory = inventory;
    }

    public void RedrawEverything(InventoryItem[] inventoryItems)
    {
        for (int i = 0; i < _inventoryTiles.Length; i++)
        {
            var invItem = inventoryItems[i];
            if (_inventoryTiles.Length <= i)
            {
                Debug.LogError($"Sent more items than tiles! {i} > {_inventoryTiles.Length}", context: this);
                return;
            }

            _inventoryTiles[i].SetItem(invItem);
        }
    }

    public void Dropitem(int index)
    {
        _inventory.DropItem(index, amount: 1);
    }
    public void Interact(int index)
    {
        _inventory.interact(index);
    }
}
