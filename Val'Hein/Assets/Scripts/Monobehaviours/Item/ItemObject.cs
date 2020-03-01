using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ItemData item;

    public void Interact()
    {
        if (PlayerCenterControl.Instance.inventory.StoreItem(item))
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Can't be picked up.");
        }
    }

    public void Spawn(Vector3 position)
    {
        if (Util.ChanceOf(item.dropRate))
        {
            Instantiate(gameObject, position, Quaternion.identity);
        }
    }

}
