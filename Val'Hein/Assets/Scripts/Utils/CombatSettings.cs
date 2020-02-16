using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CombatSettings
{
	[Tooltip("The hit spheres of the attacks.")]
	public HitMarker[] hitMarkers;
	[Tooltip("Utility class that helps the configuration of the referenced markers.")]
	public HitMarkerConfigurer hitMarkerManager;
	[Rename("Continuous Damage?")]
	[Tooltip("Is the hit continous?")]
	public bool continuousDamage;
	[ConditionalHide("continuousDamage", true)]
	[Tooltip("The interval, in seconds, it takes to detect another hit when the damage is continuous.")]
	public float continuousDamageInterval;
}