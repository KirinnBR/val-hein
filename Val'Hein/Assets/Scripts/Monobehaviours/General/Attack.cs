using UnityEngine;

[System.Serializable]
public class Attack
{
	[Tooltip("Multiplier damage a certain attack will deal.")]
	[Range(1f, 10f)]
	public float damageMultiplier;
}

[System.Serializable]
public class ComboAttack : Attack
{
	[Tooltip("The time to blend the next attack before the end of animation of this attack.")]
	[Range(0.001f, 5f)]
	public float timeToBlendCombo;
}

[System.Serializable]
public class CooldownAttack : Attack
{
	[Tooltip("The cooldown of this attack.")]
	public float cooldown;
}