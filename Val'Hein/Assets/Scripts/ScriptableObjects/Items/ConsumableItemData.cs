using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Consumable Item", menuName = "Item/Consumable Item")]
public class ConsumableItemData : ItemData
{

    [Header("Consumable Item Settings")]

    [SerializeField]
    private VitalsStats vitalsBuffs;


    public override bool Sell()
    {
        Debug.Log("Selling a consumable item.");
        return true;
    }

    public override bool Use()
    {
        //player.combat.Heal(vitalsBuffs);
        player.inventory.RemoveItem(this);
        return true;
    }

}
