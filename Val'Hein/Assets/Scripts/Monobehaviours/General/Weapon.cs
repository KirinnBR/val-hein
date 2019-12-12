using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class Weapon : MonoBehaviour
{
	public Stats statsIncreasers;

	private Stats userStats;

	public HitMarker[] hitMarkers;

	public HitMarkerManager markersManager;

	public bool continuousDamage = false;

	[ConditionalField("continuousDamage", true)]
	public float continuousDamageInterval = 1f;

	private Coroutine activeMarkersCoroutine;

	private float finalDamage;

	private void Start()
	{
		markersManager.ConfigureMarkers(hitMarkers);
	}

	public void MergeStatsWithUser(Stats stats)
	{
		userStats = stats;
		finalDamage = statsIncreasers.baseStrength + userStats.baseStrength;
	}

	public void ActivateMarkers()
	{
		if (continuousDamage)
			activeMarkersCoroutine = StartCoroutine(CheckCollisionsContinuous());
		else
			activeMarkersCoroutine = StartCoroutine(CheckCollisions());
	}

	public void DeactivateMarkers()
	{
		StopCoroutine(activeMarkersCoroutine);
	}

	private IEnumerator CheckCollisions()
	{

		List<IDamageable> cannotHit = new List<IDamageable>();
		while (true)
		{
			foreach (var marker in hitMarkers)
			{
				if (marker.TryGetDamageable(out IDamageable dmg) && !cannotHit.Contains(dmg))
				{
					dmg.TakeDamage(finalDamage);
					cannotHit.Add(dmg);
				}
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator CheckCollisionsContinuous()
	{
		List<IDamageable> cannotHit = new List<IDamageable>();
		bool canHit = true;
		while (true)
		{
			if (canHit)
			{
				foreach (var marker in hitMarkers)
				{
					if (marker.TryGetDamageable(out IDamageable dmg) && !cannotHit.Contains(dmg))
					{
						dmg.TakeDamage(finalDamage);
						cannotHit.Add(dmg);
						canHit = false;
					}
				}
			}
			else
			{
				yield return new WaitForSeconds(continuousDamageInterval);
				canHit = true;
			}
			cannotHit.Clear();
			yield return new WaitForEndOfFrame();
		}
	}

}
