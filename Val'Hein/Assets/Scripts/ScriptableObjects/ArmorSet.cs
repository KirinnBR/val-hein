using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Armor Set", menuName = "Entities/Armor Set")]
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

public enum ArmorType
{
	Head, Chest, Hands, Legs, Boots, Misc
}