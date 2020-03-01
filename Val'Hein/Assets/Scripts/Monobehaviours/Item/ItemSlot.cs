using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class ItemSlot : MonoBehaviour
{
	public ItemData item { get; set; }

	protected Image img;
    protected Button btn;

    private void Start()
    {
        img = GetComponent<Image>();
        btn = GetComponent<Button>();
        btn.onClick.AddListener(UseItem);
    }

    public virtual void SetSlot(ItemData newItem)
    {
        item = newItem;
        img.sprite = item.icon;
    }

    public virtual void ClearSlot()
    {
        item = null;
        img.sprite = null;
    }

    public virtual void UseItem()
    {
        if (item != null)
        {
            item.Use();
        }
        else
        {
            Debug.Log("There's nothing in this slot.");
        }
    }


}
