using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class EquipmentSlot : MonoBehaviour
{
    public EquipmentItemData.EquipmentType equipmentType = 0;

    public EquipmentItemData item { get; set; }

    private Image img;
    private Button btn;

    private PlayerInventorySystem inventory => PlayerCenterControl.Instance.inventory;

    private void Start()
    {
        img = GetComponent<Image>();
        btn = GetComponent<Button>();
        btn.onClick.AddListener(UseItem);
    }

    public void SetSlot(EquipmentItemData newItem)
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
