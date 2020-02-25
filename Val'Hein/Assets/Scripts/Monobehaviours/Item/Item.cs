using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ItemData item;

    public void Interact()
    {
        PlayerCenterControl.Instance.inventory.StoreItem(item);
        Destroy(gameObject);
    }

    public void Spawn(Vector3 position)
    {
        if (Util.ChanceOf(item.dropRate))
        {
            Instantiate(gameObject, position, Quaternion.identity);
        }
    }

}
