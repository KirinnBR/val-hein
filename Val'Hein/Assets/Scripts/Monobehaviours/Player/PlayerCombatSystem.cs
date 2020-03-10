﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

#pragma warning disable CS0649
public class PlayerCombatSystem : MonoBehaviour, IDamageable
{
	#region Combat Settings

	[Header("Combat Settings")]

	[SerializeField]
	private Stats stats;
	public Stats Stats { get { return stats; } }

	private Stats currentStats;

	[Tooltip("The radius to detect an enemy and enter in combat mode.")]
	[SerializeField]
	private float enemyDetectionRadius = 10f;
	public float EnemyDetectionRadius { get { return enemyDetectionRadius; } }

	[Tooltip("The logic representation of an attack.")]
	[SerializeField]
	private List<PlayerAttack> attacks;

	//[Tooltip("Does the player holds some kind of weapon?")]
	//[SerializeField]
	//private bool hasWeapon = true;

	[SerializeField]
	[ConditionalHide("hasWeapon", false)]
	private CombatSettings combatSettings;

	//[Tooltip("The weapon the player is holding")]
	//[ConditionalHide("hasWeapon", true)]
	//[SerializeField]
	//private Weapon weapon;

	public HealthEvent onHealthChanged = new HealthEvent();
	
	public bool HasTarget { get; private set; }
	public bool CanAttack { get; set; } = true;
	public bool MaxHealth => stats.health == currentStats.health;

	private bool isAttacking = false;
	private bool onCombat = false;

	private HitMarker[] hitMarkers => combatSettings.hitMarkers;
	private PlayerAttack currentAttack => attacks[currentAttackIndex];
	
	private bool animationFinished = true;
	private int currentAttackIndex = 0;

	private List<Transform> targetableEnemies = new List<Transform>();
	private Transform targetEnemy;
	private Collider targetEnemyCollider;

	private Coroutine computeComboCoroutine = null;
	private Coroutine activeMarkersCoroutine = null;
	private Coroutine trackAnimationCoroutine = null;
	private Coroutine updateTargetCoroutine = null;
	private Coroutine trackBuffDebuffCoroutine = null;

	private BaseState<PlayerCombatSystem> currentState;
	public PlayerAttackingState attackingState { get; } = new PlayerAttackingState();
	public PlayerAttackWaitState attackWaitState { get; } = new PlayerAttackWaitState();

	#endregion

	#region External Properties

	private LayerMask combatLayer { get { return PlayerCenterControl.Instance.combatCheckLayer; } }
	private CameraBehaviour cam { get { return PlayerCenterControl.Instance.camera; } }
	private PlayerControllerSystem controller { get { return PlayerCenterControl.Instance.controller; } }
	private PlayerInputSystem input { get { return PlayerCenterControl.Instance.input; } }
	private Animator anim { get { return PlayerCenterControl.Instance.anim; } }

	#endregion

	#region Common Methods

	void Start()
	{
		combatSettings.hitMarkerManager.ConfigureMarkers(hitMarkers);
		currentStats = stats;
	}
	// Update is called once per frame
	private void Update()
	{
		ProccessInput();
		anim.SetBool("On Combat", onCombat);
	}

	private void FixedUpdate()
	{
		SetTargetableEnemies();
	}

	private void LateUpdate()
	{
		anim.ResetTrigger("Attack");
	}

	private void ProccessInput()
	{
		if (input.Attack)
			ProccessAttackAnimation();
		if (input.Target)
			SetTarget();
	}

	private void ProccessAttackAnimation()
	{
		if (isAttacking || !CanAttack || !animationFinished) return;

		animationFinished = false;

		anim.SetInteger("Attack Index", currentAttackIndex);
		anim.SetTrigger("Attack");

		isAttacking = true;
		controller.MovementBlocked = true;
		controller.RotationBlocked = true;

		if (trackAnimationCoroutine != null)
			StopCoroutine(trackAnimationCoroutine);
		trackAnimationCoroutine = StartCoroutine(TrackAnimation());

		if (computeComboCoroutine != null)
			StopCoroutine(computeComboCoroutine);
	}

	private void SetTargetableEnemies()
	{
		targetableEnemies.Clear();
		var enemies = Physics.OverlapSphere(transform.position, enemyDetectionRadius, combatLayer, QueryTriggerInteraction.UseGlobal);
		foreach (var enemy in enemies)
		{
			var enemyT = enemy.transform;
			if (transform.Equals(enemyT)) continue;
			targetableEnemies.Add(enemyT);
		}
		onCombat = targetableEnemies.Count > 0;
	}

	private void SetTarget()
	{
		if (HasTarget)
		{
			UnsetTarget();
			return;
		}

		if (FindTarget())
		{
			anim.SetBool("Is Targeting", true);
			controller.RotationBlocked = true;
			controller.RunBlocked = true;
			HasTarget = true;
			updateTargetCoroutine = StartCoroutine(UpdateTarget());
		}
	}

	private bool FindTarget()
	{
		if (targetableEnemies.Count < 1) return false;

		float closest = enemyDetectionRadius + 10f;

		foreach (var enemy in targetableEnemies)
		{
			var dist = Vector3.Distance(transform.position, enemy.position);
			if (dist < closest)
			{
				targetEnemy = enemy;
				closest = dist;
			}
		}
		//targetEnemyIndex = targetableEnemies.IndexOf(targetEnemy);
		targetEnemyCollider = targetEnemy.GetComponent<Collider>();
		cam.SecondaryTarget = targetEnemy;
		return true;
	}

	private void UnsetTarget()
	{
		anim.SetBool("Is Targeting", false);
		controller.RotationBlocked = false;
		controller.RunBlocked = false;
		targetEnemy = null;
		HasTarget = false;
		cam.SecondaryTarget = null;
		StopCoroutine(updateTargetCoroutine);
	}

	private void ActivateMarkers()
	{
		if (combatSettings.continuousDamage)
			activeMarkersCoroutine = StartCoroutine(CheckCollisionsContinuous());
		else
			activeMarkersCoroutine = StartCoroutine(CheckCollisions());
	}

	private void DeactivateMarkers()
	{
		StopCoroutine(activeMarkersCoroutine);
	}

	private void FinishAnimation()
	{
		if (animationFinished) return;


		controller.MovementBlocked = false;

		if (!HasTarget)
			controller.RotationBlocked = false;
		
		isAttacking = false;
		animationFinished = true;

		if (currentAttackIndex == attacks.Count - 1)
			currentAttackIndex = 0;
		else
		{
			computeComboCoroutine = StartCoroutine(ComputeCombo());
			currentAttackIndex++;
		}
	}

	#endregion

	#region IDamageable Methods

	public void TakeDamage(int amount)
	{
		currentStats.health -= amount;
		if (currentStats.health <= 0)
		{
			Die();
		}
		onHealthChanged.Invoke(currentStats.health);
	}

	private void Die()
	{
		Debug.LogError("Player died");
	}

	public void Heal(VitalsStats vitalsHeal)
	{
		currentStats += vitalsHeal;
		currentStats = Stats.ClampVitals(currentStats, stats);

		if (currentStats.health > stats.health)
			currentStats.health = stats.health;

		onHealthChanged.Invoke(currentStats.health);
	}

	public void Buff(Stats buffStats)
	{
		currentStats += buffStats;
		stats += buffStats;
	}

	public void Debuff(Stats buffStats)
	{
		stats -= buffStats;
		currentStats = Stats.ClampStats(currentStats, stats);
	}

	public void Buff(AttributeStats buffAttributes)
	{
		stats += buffAttributes;
		currentStats += buffAttributes;
	}

	public void Debuff(AttributeStats debuffAttributes)
	{
		stats -= debuffAttributes;
		currentStats = Stats.ClampStats(currentStats, stats);
	}

	public void Buff(Stats buffStats, float buffTime)
	{
		if (trackBuffDebuffCoroutine != null)
			StopCoroutine(trackBuffDebuffCoroutine);
		trackBuffDebuffCoroutine = StartCoroutine(TrackBuffDebuff(buffStats, buffTime, true));
	}

	public void Debuff(Stats debuffStats, float buffTime)
	{
		if (trackBuffDebuffCoroutine != null)
			StopCoroutine(trackBuffDebuffCoroutine);
		trackBuffDebuffCoroutine = StartCoroutine(TrackBuffDebuff(debuffStats, buffTime, false));
	}

	private IEnumerator TrackBuffDebuff(Stats buffStats, float buffTime, bool isBuff)
	{
		if (isBuff)
		{
			stats += buffStats;
			currentStats += buffStats;
			onHealthChanged.Invoke(currentStats.health);
			yield return new WaitForSeconds(buffTime);
			stats -= buffStats;
			currentStats = Stats.ClampStats(currentStats, stats);
			onHealthChanged.Invoke(currentStats.health);
		}
		else
		{
			stats -= buffStats;
			currentStats = Stats.ClampStats(currentStats, stats);
			onHealthChanged.Invoke(currentStats.health);
			yield return new WaitForSeconds(buffTime);

		}

		
	}


	#endregion

	#region Coroutines

	private IEnumerator TrackAnimation()
	{
		int currentFrame = 0;
		bool isHitMarkersActive = false;
		int currentHitMarkerIndex = 0;
		int hitMarkerLength = currentAttack.hitMarkersTime.Length;
		while (true)
		{
			if (currentFrame == currentAttack.animationLength)
			{
				FinishAnimation();
				break;
			}

			if (currentHitMarkerIndex < hitMarkerLength)
			{
				var hitMarkerTime = currentAttack.hitMarkersTime[currentHitMarkerIndex];

				if (!isHitMarkersActive)
				{
					if (hitMarkerTime != null)
					{
						if (currentFrame == hitMarkerTime.x)
						{
							ActivateMarkers();
							isHitMarkersActive = true;
						}
					}
				}
				else
				{
					if (currentFrame == hitMarkerTime.y)
					{
						DeactivateMarkers();
						isHitMarkersActive = false;
						currentHitMarkerIndex++;
					}
				}
			}
			currentFrame++;
			yield return null;
		}
	}

	private IEnumerator ComputeCombo()
	{
		int aux = currentAttackIndex;
		yield return new WaitForSeconds(currentAttack.timeToBlendCombo);
		currentAttackIndex = 0;
	}

	private void DoDamage(IDamageable dmg) => dmg.TakeDamage(currentStats.strength * currentAttack.damageMultiplier);

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
			yield return new WaitForSeconds(combatSettings.continuousDamageInterval);
			cannotHit.Clear();
			yield return null;
		}
	}

	private IEnumerator UpdateTarget()
	{
		var completeTwirl = false;
		while (true)
		{
			if (!targetableEnemies.Contains(targetEnemy) || targetEnemyCollider.enabled == false)
			{
				if (!FindTarget())
					break;
				else
				{
					completeTwirl = false;
				}
			}

			//Mouse ScrollWheel Input.

			if (completeTwirl)
			{
				transform.LookAt(new Vector3(targetEnemy.position.x, transform.position.y, targetEnemy.position.z));
			}
			else
			{
				var look = Quaternion.LookRotation(new Vector3(targetEnemy.position.x, transform.position.y, targetEnemy.position.z) - transform.position);
				transform.rotation = Quaternion.Slerp(transform.rotation, look, controller.TurnSpeed * Time.deltaTime * 2f);
				if (Quaternion.Angle(transform.rotation, look) < 5f)
				{
					transform.rotation = look;
					completeTwirl = true;
				}
			}
			yield return null;
		}
		UnsetTarget();
	}

	#endregion
}

[System.Serializable]
public class HealthEvent : UnityEvent<float> { }