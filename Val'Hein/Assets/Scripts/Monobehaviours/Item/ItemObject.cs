using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{
    public ItemData item { get; set; } = null;

    public void Interact()
    {
        PlayerCenterControl.Instance.inventory.StoreItem(item);
        Destroy(gameObject);
    }

}
