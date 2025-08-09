using PurrNet;
using UnityEngine;

public class PlayerClicker : NetworkBehaviour
{
    [SerializeField] private Inventory _inventory;

    private async void Update()
    {
        if (!IsOwner)
            return;

        if (!Input.GetKeyDown(KeyCode.Mouse0))
            return;

        RaycastHit2D hit = Physics2D.Raycast(
            origin: Camera.main.ScreenToWorldPoint(Input.mousePosition),
            direction: Vector2.zero
        );

        if (!hit.collider)
            return;

        if (hit.collider.TryGetComponent(out Item item) &&
            await _inventory.TryAddItem(item.preset, quantity: 1))
        {
            Destroy(item.gameObject);
        }
    }
}