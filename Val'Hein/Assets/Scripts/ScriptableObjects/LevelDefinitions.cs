using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Definitions", menuName = "Entities/Level Definitions")]
public class LevelDefinitions : ScriptableObject
{

	[Header("Experience")]
	[HideInInspector]
	public float experience = 0f;
	public float experienceToLevelUp = 100f;
	[HideInInspector]
	public int level = 1;

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
		Debug.Log("Leveled up!");
		/*
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
		*/
	}

}
