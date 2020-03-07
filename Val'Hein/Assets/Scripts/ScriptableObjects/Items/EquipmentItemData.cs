using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Equipment Item", menuName = "Item/Equipment Item")]
public class EquipmentItemData : ItemData
{
    [Header("Equipment Item Settings")]

    public EquipmentType type;

    public AttributeStats buffStats;

    public override bool Sell()
    {
        Debug.Log("Selling an equipment item.");
        return true;
    }

    public override bool Use()
    {
        return player.inventory.Equip(this);
    }

    public enum EquipmentType
    {
        Gloves = 0, Chest = 1, Pants = 2, Boots = 3, LeftWeapon = 4, RightWeapon = 5
    }

    public static int EquipmentTypeLength => System.Enum.GetValues(typeof(EquipmentType)).Length;
    public int Index => (int)type;
    public bool IsWeapon => type == EquipmentType.RightWeapon || type == EquipmentType.LeftWeapon;

}
