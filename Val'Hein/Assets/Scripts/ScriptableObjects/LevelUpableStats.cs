using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New LevelUpableStats", menuName = "Entities/Stats/LevelUpableStats")]
public class LevelUpableStats : Stats
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
	public float experience = 0f;
	public float experienceToLevelUp = 100f;
	public int level = 0;
	public StatsLevelUp[] onLevelUps;

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
		baseHealth += modifiers.healthModifier;
		baseResistance += modifiers.resistanceModifier;
		baseStrength += modifiers.strengthModifier;
		basePrecision += modifiers.precisionModifier;
		basePower += modifiers.powerModifier;
		baseIntelligence += modifiers.intelligenceModifier;
		baseRunicKnowledge += modifiers.runicKnowledgeModifier;
		baseAgility += modifiers.agilityModifier;
		experienceToLevelUp += modifiers.experienceToLevelUpModifier;
	}

}
