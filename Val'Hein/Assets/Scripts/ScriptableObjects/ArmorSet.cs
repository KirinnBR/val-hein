using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArmorType
{
	Head, Chest, Hands, Legs, Boots, Misc
}

[CreateAssetMenu(fileName = "New Armor Set", menuName = "Stats/Armor/ArmorSet")]
public class ArmorSet : ScriptableObject
{
	[System.Serializable]
	public struct Armor
	{
		public GameObject armorPrefab;
		public Stats modifiers;
		public ArmorType type;
	};

	public Armor[] armorParts;
}
