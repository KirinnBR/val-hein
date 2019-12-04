using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Armor Stats Increaser", menuName = "Entities/Armor/ArmorStatsIncreaser")]
public class ArmorStatsIncreaser : ScriptableObject
{
	[System.Serializable]
	public struct Armor
	{
		public GameObject armorPrefab;
		public Stats modifiers;
	};

	[Header("Helmet")]
	public Armor helmetArmor;

	[Header("Chestplate")]
	public Armor chestplateArmor;

	[Header("Gauntlet")]
	public Armor gauntletArmor;

	[Header("Greaves")]
	public Armor greavesArmor;

	[Header("Feet")]
	public Armor feetArmor;

}
