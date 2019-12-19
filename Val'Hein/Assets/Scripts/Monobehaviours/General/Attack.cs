using UnityEngine;

[System.Serializable]
public class Attack
{
	[Tooltip("Multiplier damage a certain attack will deal.")]
	[Range(1f, 10f)]
	public float damageMultiplier = 1f;
	[Tooltip("The time to blend the next attack before the end of animation of this attack.")]
	[Range(0.001f, 5f)]
	public float timeToBlendCombo = 0.2f;
	public Attack()
	{
		damageMultiplier = 1f;
		timeToBlendCombo = 0.2f;
	}
}