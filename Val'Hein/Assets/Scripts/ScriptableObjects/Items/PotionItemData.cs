using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Potion Item", menuName = "Item/Potion Item")]
public class PotionItemData : ItemData
{
    [Header("Potion Item Settings")]

    [SerializeField]
    private Stats buffStats;
    [SerializeField]
    private float buffTime = 10f;



    public override bool Sell()
    {
        Debug.Log("Selling a potion.");
        return true;
    }

    public override bool Use()
    {
        player.combat.Buff(buffStats, buffTime);
        player.inventory.RemoveItem(this);
        return true;
    }
}
