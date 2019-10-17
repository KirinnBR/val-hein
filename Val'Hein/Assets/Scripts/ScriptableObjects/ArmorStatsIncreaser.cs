using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ArmorStatsIncreaser", menuName = "Entities/Armor/ArmorStatsIncreaser")]
public class ArmorStatsIncreaser : ScriptableObject
{
	[System.Serializable]
	public struct Armor
	{
		public Mesh mesh;
		public Texture2D texture;
		public Stats modifiers;
	};


	[Header("Helmet")]
	public Armor helmetArmor;

	[Header("Chestplate")]
	public Armor chestplateArmor;

	[Header("eu esqueci essa parte")]
	public Armor bracoArmor;

	[Header("A mano serio")]
	public Armor legArmor;

	[Header("A este ponto eu desisto de me lembrar.")]
	public Armor aquelaDoPe;

}
