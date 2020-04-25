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

}
