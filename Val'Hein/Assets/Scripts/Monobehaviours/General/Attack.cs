using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack
{
	[Tooltip("Multiplier damage a certain attack will deal.")]
	[Range(1f, 10f)]
	public int damageMultiplier;
	[Tooltip("The length of the specific attack animation.")]
	public int animationLength;
	[Tooltip("When to enable and disable Hit Markers. P.S.: X and Y must always be lesser than Animation Length. X must always be lesser than Y.")]
	public Vector2[] hitMarkersTime;
}

[System.Serializable]
public class PlayerAttack : Attack
{
	[Tooltip("The time, in seconds, to combo after the end of animation.")]
	public float timeToBlendCombo;
}

[System.Serializable]
public class NPCAttack : Attack
{
	[Tooltip("The delay to use another attack.")]
	public float timeToRest;
}