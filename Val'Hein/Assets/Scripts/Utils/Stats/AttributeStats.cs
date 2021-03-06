﻿[System.Serializable]
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

	public void Clamp(AttributeStats max)
	{
		if (agility > max.agility) agility = max.agility;
		if (intelligence > max.intelligence) intelligence = max.intelligence;
		if (power > max.power) power = max.power;
		if (precision > max.precision) precision = max.precision;
		if (resistance > max.resistance) resistance = max.resistance;
		if (runicKnowledge > max.runicKnowledge) runicKnowledge = max.runicKnowledge;
		if (strength > max.strength) strength = max.strength;
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