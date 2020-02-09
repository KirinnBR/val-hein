using UnityEngine;

[System.Serializable]
public class Attack
{
	[Tooltip("Multiplier damage a certain attack will deal.")]
	[Range(1f, 10f)]
	public float damageMultiplier;
}

[System.Serializable]
public class PlayerAttack : Attack
{
	[Tooltip("The time to blend the next attack before the end of animation of this attack.")]
	[Range(0.001f, 5f)]
	public float timeToBlendCombo;
}

[System.Serializable]
public class NPCAttack : Attack
{
	[Tooltip("The delay to use another attack.")]
	[Range(0f, 10f)]
	public float timeToRest;
}