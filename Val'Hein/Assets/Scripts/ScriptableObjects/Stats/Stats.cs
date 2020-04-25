using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Stats
{
	public VitalsStats vitals;
	public AttributeStats attributes;

	public Stats(VitalsStats vitals, AttributeStats attributes)
	{
		this.vitals = vitals;
		this.attributes = attributes;
	}

	public Stats(AttributeStats attributes, VitalsStats vitals)
	{
		this.vitals = vitals;
		this.attributes = attributes;
	}

	public Stats(VitalsStats vitals) : this()
	{
		this.vitals = vitals;
	}

	public Stats(AttributeStats attributes) : this()
	{
		this.attributes = attributes;
	}

	public int health { get => vitals.health; set => vitals.health = value; }
	public int mana { get => vitals.mana; set => vitals.mana = value; }
	public int stamina { get => vitals.stamina; set => vitals.stamina = value; }
	public int resistance { get => attributes.resistance; set => attributes.resistance = value; }
	public int strength { get => attributes.strength; set => attributes.strength = value; }
	public int precision { get => attributes.precision; set => attributes.precision = value; }
	public int power { get => attributes.power; set => attributes.power = value; }
	public int intelligence { get => attributes.intelligence; set => attributes.intelligence = value; }
	public int runicKnowledge { get => attributes.runicKnowledge; set => attributes.runicKnowledge = value; }
	public int agility { get => attributes.agility; set => attributes.agility = value; }

	public static Stats operator +(Stats a, Stats b)
	{
		Stats result = new Stats
		{
			vitals = a.vitals + b.vitals,
			attributes = a.attributes + b.attributes
		};
		return result;
	}

	public static Stats operator -(Stats a, Stats b)
	{
		Stats result = new Stats
		{
			vitals = a.vitals - b.vitals,
			attributes = a.attributes - b.attributes
		};
		return result;
	}

}
