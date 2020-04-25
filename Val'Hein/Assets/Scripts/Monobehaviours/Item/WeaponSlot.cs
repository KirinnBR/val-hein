using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour
{
    public WeaponHand weaponHand;

    public WeaponItemData item { get; set; }

    private Image img;
    private Button btn;

    private PlayerInventorySystem inventory => PlayerCenterControl.Instance.inventory;

    private void Start()
    {
        img = GetComponent<Image>();
        btn = GetComponent<Button>();
        btn.onClick.AddListener(UseItem);
    }

    public void SetSlot(WeaponItemData newItem)
    {
        item = newItem;
        img.sprite = item.icon;
    }

    public void ClearSlot()
    {
        item = null;
        img.sprite = null;
    }

    public void UseItem()
    {
        inventory.Unequip(item);
    }

}
