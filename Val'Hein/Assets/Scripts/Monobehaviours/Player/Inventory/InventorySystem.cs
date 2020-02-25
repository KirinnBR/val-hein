using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventorySystem : MonoBehaviour
{
    [SerializeField]
    private float interactSphereRadius = 5f;

    private InputSystem input { get => PlayerCenterControl.Instance.input; }
    private UISystem ui { get => PlayerCenterControl.Instance.ui; }
    private LayerMask itemsLayer { get => PlayerCenterControl.Instance.ItemsLayer; }

    public UnityEvent onItemsChangeCallback = new UnityEvent();
    public List<ItemData> storedItems = new List<ItemData>();

    private bool CanPickUpItems { get; set; } = false;
    private Item item;

    private void FixedUpdate()
    {
        Collider[] itemCollider = new Collider[1];
        var n = Physics.OverlapSphereNonAlloc(transform.position, interactSphereRadius, itemCollider, itemsLayer);
        CanPickUpItems = n > 0 && itemCollider[0].TryGetComponent(out item);
    }

    private void Update()
    {
        if (CanPickUpItems)
        {
            if (input.Interact)
            {
                item.Interact();
            }
        }
    }

    public void StoreItem(ItemData item)
    {
        storedItems.Add(item);
        onItemsChangeCallback.Invoke();
    }

    public void RemoveItem(ItemData item)
    {
        if (storedItems.Remove(item))
        {
            onItemsChangeCallback.Invoke();
        }
        else
        {
            Debug.LogError("Couldn't find " + item.name + " in player's inventory.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactSphereRadius);
    }

}
