using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public Stats statsIncreasers;

	public CombatSettings combatSettings;

	private Coroutine activeMarkersCoroutine;

	private int finalDamage;

	private int damageMultiplier;

	private HitMarkerConfigurer hitMarkerManager { get { return combatSettings.hitMarkerManager; } }
	private HitMarker[] hitMarkers { get { return combatSettings.hitMarkers; } }

	private void Start()
	{
		hitMarkerManager.ConfigureMarkers(hitMarkers);
	}

	public void ActivateMarkers(int multiplier)
	{
		damageMultiplier = multiplier;
		if (combatSettings.continuousDamage)
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
					DoDamage(dmg);
					cannotHit.Add(dmg);
				}
			}
			yield return null;
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
						DoDamage(dmg);
						cannotHit.Add(dmg);
						canHit = false;
					}
				}
			}
			else
			{
				yield return new WaitForSeconds(combatSettings.continuousDamageInterval);
				canHit = true;
			}
			cannotHit.Clear();
			yield return null;
		}
	}

	private void DoDamage(IDamageable dmg) => dmg.TakeDamage(finalDamage * damageMultiplier);
}
