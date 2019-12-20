using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Definitions", menuName = "Entities/Stats/LevelDefinitions")]
public class LevelDefinitions : ScriptableObject
{
	[System.Serializable]
	public class StatsLevelUp
	{
		public float healthModifier;
		public float resistanceModifier;
		public float strengthModifier;
		public float precisionModifier;
		public float powerModifier;
		public float intelligenceModifier;
		public float runicKnowledgeModifier;
		public float agilityModifier;
		public float experienceToLevelUpModifier;
	}

	[Header("Experience")]
	[HideInInspector]
	public float experience = 0f;
	public float experienceToLevelUp = 100f;
	[HideInInspector]
	public int level = 1;
	public StatsLevelUp[] onLevelUps;

	[Header("References")]
	public Stats statsToIncrease;

	public void GiveExperience(float ammount)
	{
		experience += ammount;
		if (experience >= experienceToLevelUp)
		{
			LevelUp();
		}
	}

	public void LevelUp()
	{
		level++;
		experience = 0;
		var modifiers = onLevelUps[level - 1];
		statsToIncrease.baseHealth += modifiers.healthModifier;
		statsToIncrease.baseResistance += modifiers.resistanceModifier;
		statsToIncrease.baseStrength += modifiers.strengthModifier;
		statsToIncrease.basePrecision += modifiers.precisionModifier;
		statsToIncrease.basePower += modifiers.powerModifier;
		statsToIncrease.baseIntelligence += modifiers.intelligenceModifier;
		statsToIncrease.baseRunicKnowledge += modifiers.runicKnowledgeModifier;
		statsToIncrease.baseAgility += modifiers.agilityModifier;
		experienceToLevelUp += modifiers.experienceToLevelUpModifier;
	}

}
