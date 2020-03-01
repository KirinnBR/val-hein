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
        Helmet = 0, LeftWeapon = 1, RightWeapon = 2, Gloves = 3, Chest = 4, Pants = 5, Boots = 6
    }

    public static int EquipmentTypeLength => System.Enum.GetValues(typeof(EquipmentType)).Length;
    public int Index => (int)type;

}
