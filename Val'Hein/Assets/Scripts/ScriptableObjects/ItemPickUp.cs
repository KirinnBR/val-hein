using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    None, Potion, Currency
}

[CreateAssetMenu(fileName = "New Item", menuName = "Spawnable/New Item")]
public class ItemPickUp : ScriptableObject
{
    [Header("Item Settings")]

    public new string name = "Item";
    public ItemType type = ItemType.None;
    public Sprite icon = null;
    public float weight = 0f;
    public bool isStackable = false;
    public bool destroyOnUse = false;
    [Range(0, 100)]
    public float spawnRate = 100f;
}
