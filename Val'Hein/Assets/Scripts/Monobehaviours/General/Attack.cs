using UnityEngine;

[System.Serializable]
public class Attack
{
	[Tooltip("The name of the trigger for the attack to be called.")]
	public string triggerName = "Attack";
	[Tooltip("Multiplier damage a certain attack will deal.")]
	[Range(1f, 10f)]
	public float damageMultiplier = 1f;
	public Attack()
	{
		triggerName = "Attack";
		damageMultiplier = 1f;
	}
}