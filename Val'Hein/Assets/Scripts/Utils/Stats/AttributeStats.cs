﻿using System;

[System.Serializable]
public struct AttributeStats
{
	public int resistance;
	public int strength;
	[UnityEngine.Range(0, 100)]
	public int precision;
	public int power;
	public int intelligence;
	public int runicKnowledge;
	public int agility;

	public AttributeStats(int resistance, int strength, int precision, int power, int intelligence, int runicKnowledge, int agility)
	{
		this.resistance = resistance;
		this.strength = strength;
		this.precision = precision;
		this.power = power;
		this.intelligence = intelligence;
		this.runicKnowledge = runicKnowledge;
		this.agility = agility;
	}

	public static AttributeStats Clamp(AttributeStats toBeClamped, AttributeStats clampReference)
	{
		var result = toBeClamped;

		if (result.agility > clampReference.agility) result.agility = clampReference.agility;
		if (result.intelligence > clampReference.intelligence) result.intelligence = clampReference.intelligence;
		if (result.power > clampReference.power) result.power = clampReference.power;
		if (result.precision > clampReference.precision) result.precision = clampReference.precision;
		if (result.resistance > clampReference.resistance) result.resistance = clampReference.resistance;
		if (result.runicKnowledge > clampReference.runicKnowledge) result.runicKnowledge = clampReference.runicKnowledge;
		if (result.strength > clampReference.strength) result.strength = clampReference.strength;


		return result;
	}

	public static AttributeStats operator +(AttributeStats a, AttributeStats b)
	{
		AttributeStats result = new AttributeStats
		{
			agility = a.agility + b.agility,
			intelligence = a.intelligence + b.intelligence,
			power = a.power + b.power,
			precision = a.precision + b.precision,
			resistance = a.resistance + b.resistance,
			runicKnowledge = a.runicKnowledge + b.runicKnowledge,
			strength = a.strength + b.strength
		};
		return result;
	}
	
	public static AttributeStats operator -(AttributeStats a, AttributeStats b)
	{
		AttributeStats result = new AttributeStats
		{
			agility = a.agility - b.agility,
			intelligence = a.intelligence - b.intelligence,
			power = a.power - b.power,
			precision = a.precision - b.precision,
			resistance = a.resistance - b.resistance,
			runicKnowledge = a.runicKnowledge - b.runicKnowledge,
			strength = a.strength - b.strength
		};
		return result;
	}

}