using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType
{
    Trash, Consumable, Potion, Armor, Weapon
}

[CreateAssetMenu(fileName = "New Item", menuName = "Loot/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Data")]

    public string name = "Item";
    public Sprite sprite;
    [Range(0.001f, 100f)]
    public float dropRate = 100f;
    public ItemType itemType = ItemType.Trash;

    public Stats statsIncreaser;

    public bool Use()
    {
        switch (itemType)
        {
            case ItemType.Trash:
                return false;
            case ItemType.Consumable:
                PlayerCenterControl.Instance.combat.HealDamage(statsIncreaser.baseHealth);
                return true;
            case ItemType.Potion:
                return true;
            case ItemType.Armor:
                return true;
            case ItemType.Weapon:
                return true;
            default:
                return false;
        }
    }

}
