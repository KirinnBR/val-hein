using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class CombatObject : MonoBehaviour, IDamageable
{
	[Header("Combat Settings")]

	[SerializeField]
	private Stats m_stats;
	public Stats stats => m_stats;
	protected Stats currentStats;
	
	[SerializeField]
	private HitMarker[] m_hitMarkers;
	public HitMarker[] hitMarkers => m_hitMarkers;

	[SerializeField]
	[Rename("Continuous Damage?")]
	private bool m_continuousDamage = false;
	public bool continuousDamage => m_continuousDamage;

	[Tooltip("The interval, in seconds, it takes to detect another hit when the damage is continuous.")]
	public float m_continuousDamageInterval;
	public float continuousDamageInterval => m_continuousDamageInterval;

	[SerializeField]
	private LayerMask m_combatLayer;
	public LayerMask combatLayer => m_combatLayer;

	public UnityEvent OnTakeDamage = new UnityEvent();
	public UnityEvent OnHeal = new UnityEvent();


	private Coroutine activeMarkersCoroutine = null;
	protected bool hitMarkersActive = false;


	protected virtual void Start()
	{
		currentStats = m_stats;

		foreach (var marker in hitMarkers)
		{
			marker.hitLayer = combatLayer;
			marker.ownerTag = tag;
			marker.triggerInteraction = QueryTriggerInteraction.UseGlobal;
		}
	}

	public virtual void TakeDamage(int ammount, DamageType type)
	{
		OnTakeDamage.Invoke();
	}

	protected virtual void Die() {}

	#region Buff and Debuff Methods

	public void Heal(VitalsStats heal)
	{
		currentStats.vitals += heal;
		currentStats.vitals.Clamp(m_stats.vitals);
		OnHeal.Invoke();
	}

	public void Buff(AttributeStats attributes, float buffTime = 0f)
	{
		m_stats.attributes += attributes;
		currentStats.attributes += attributes;
		if (buffTime > 0f)
		{
			TrackBuff(new Stats(attributes), buffTime);
		}
	}
	
	public void Buff(AttributeStats attributes, VitalsStats vitals, float buffTime = 0f)
	{
		m_stats.vitals += vitals;
		m_stats.attributes += attributes;
		currentStats.vitals += vitals;
		currentStats.attributes += attributes;
		if (buffTime > 0f)
		{
			TrackBuff(new Stats(vitals, attributes), buffTime);
		}
	}

	public void Debuff(AttributeStats attributes, float buffTime = 0f)
	{
		m_stats.attributes -= attributes;
		currentStats.attributes.Clamp(m_stats.attributes);
		if (buffTime > 0f)
		{
			TrackDebuff(new Stats(attributes), buffTime);
		}
	}

	public void Debuff(AttributeStats attributes, VitalsStats vitals, float buffTime = 0f)
	{
		m_stats.vitals -= vitals;
		m_stats.attributes -= attributes;
		currentStats.vitals.Clamp(m_stats.vitals);
		currentStats.attributes.Clamp(m_stats.attributes);
		if (buffTime > 0f)
		{
			TrackDebuff(new Stats(vitals, attributes), buffTime);
		}
	}

	private IEnumerator TrackBuff(Stats buffStats, float buffTime)
	{
		yield return new WaitForSeconds(buffTime);
		Debuff(buffStats.attributes, buffStats.vitals);
	}

	private IEnumerator TrackDebuff(Stats buffStats, float buffTime)
	{
		yield return new WaitForSeconds(buffTime);
		Buff(buffStats.attributes, buffStats.vitals);
	}

	#endregion

	#region Hit Marker Methods

	protected void ActivateMarkers()
	{
		hitMarkersActive = true;
		if (continuousDamage)
			activeMarkersCoroutine = StartCoroutine(CheckCollisionsContinuous());
		else
			activeMarkersCoroutine = StartCoroutine(CheckCollisions());
	}

	protected void DeactivateMarkers()
	{
		hitMarkersActive = false;
		StopCoroutine(activeMarkersCoroutine);
	}

	#endregion

	#region Hit Marker Coroutines

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
			yield return new WaitForSeconds(m_continuousDamageInterval);
			cannotHit.Clear();
			yield return null;
		}
	}

	#endregion

	protected abstract void DoDamage(IDamageable dmg);

}
