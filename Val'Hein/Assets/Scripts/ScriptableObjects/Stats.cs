using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stats", menuName = "Entities/Stats/Stats")]
public class Stats : ScriptableObject
{
	[Header("Vitals")]
	[Header("Standart Stats")]
	public float baseHealth = 100f;
	public float baseResistance = 10f;

	[Header("Physical damage")]
	public float baseStrength = 10f;
	[Range(0, 100)]
	public float basePrecision = 50f;

	[Header("Magic Damage")]
	public float basePower = 10f;
	public float baseIntelligence = 10f;
	public float baseRunicKnowledge = 10f;

	[Header("Physical speed")]
	public float baseAgility = 10f;


}
