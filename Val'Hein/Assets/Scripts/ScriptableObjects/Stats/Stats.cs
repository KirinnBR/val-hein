using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stats", menuName = "General/Stats")]
public class Stats : ScriptableObject
{
	[Header("Vitals")]
	[Header("Standart Stats")]
	public float baseHealth = 100f;
	//public float baseMana = 100f;
	public float baseStamina = 100f;
	public float baseResistance = 10f;

	[Header("Physical Damage")]
	public float baseStrength = 10f;
	[Range(0, 100)]
	public float basePrecision = 50f;

	[Header("Magic Damage")]
	public float basePower = 10f;
	public float baseIntelligence = 10f;
	public float baseRunicKnowledge = 10f;

	[Header("Physical Speed")]
	public float baseAgility = 10f;

	public void MergeStats(Stats otherStats)
	{
		baseHealth += otherStats.baseHealth;
		baseStamina += otherStats.baseStamina;
		baseResistance += otherStats.baseResistance;
		baseStrength += otherStats.baseStrength;
		basePrecision += otherStats.basePrecision;
		basePower += otherStats.basePower;
		baseIntelligence += otherStats.baseIntelligence;
		baseRunicKnowledge += otherStats.baseRunicKnowledge;
		baseAgility += otherStats.baseAgility;
	}


	public override string ToString()
	{
		return $"Base Health: {baseHealth} - Base Resistance: {baseResistance} - Base Strength: {baseStrength} - " +
			$"Base Precision: {basePrecision} - Base Power: {basePower} - " +
			$"Base Intelligence: {baseIntelligence} - Base Runic Knowledge: {baseRunicKnowledge} - Base Agility: {baseAgility}";
	}

}
