using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class ItemSlot : MonoBehaviour
{
	public ItemData item { get; set; }

	private Image img;
    private Button btn;

    private InventorySystem inventory { get { return PlayerCenterControl.Instance.inventory; } }

    private void Start()
    {
        img = GetComponent<Image>();
        btn = GetComponent<Button>();
        btn.onClick.AddListener(UseItem);
    }

    public void SetSlot(ItemData newItem)
    {
        item = newItem;
        img.sprite = item.sprite;
        img.enabled = true;
        Debug.Log("New slot on " + name + ": " + item.name);
    }

    public void ClearSlot()
    {
        if (item != null)
        {
            Debug.Log("Cleared slot " + name + ": " + item.name);
        }
        
        item = null;
        img.sprite = null;
        img.enabled = false;
    }

    public void UseItem()
    {
        if (item.Use())
        {
            inventory.RemoveItem(item);
        }
    }


}
