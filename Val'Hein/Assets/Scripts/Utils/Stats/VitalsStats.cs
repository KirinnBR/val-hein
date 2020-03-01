[System.Serializable]
public struct VitalsStats
{
	public int health;
	public int mana;
	public int stamina;

	public VitalsStats(int health, int mana, int stamina)
	{
		this.health = health;
		this.mana = mana;
		this.stamina = stamina;
	}

	public static VitalsStats operator +(VitalsStats a, VitalsStats b)
	{
		VitalsStats result = new VitalsStats
		{
			health = a.health + b.health,
			mana = a.mana + b.mana,
			stamina = a.stamina + b.stamina
		};
		return result;
	}

	public static VitalsStats operator -(VitalsStats a, VitalsStats b)
	{
		VitalsStats result = new VitalsStats
		{
			health = a.health - b.health,
			mana = a.mana - b.mana,
			stamina = a.stamina - b.stamina
		};
		return result;
	}


}