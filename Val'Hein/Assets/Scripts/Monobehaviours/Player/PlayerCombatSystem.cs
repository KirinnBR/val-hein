using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649
public class PlayerCombatSystem : CombatObject
{
	#region Combat Settings

	[Tooltip("The radius to detect an enemy and enter in combat mode.")]
	[SerializeField]
	private float m_enemyDetectionRadius = 10f;
	public float enemyDetectionRadius { get { return m_enemyDetectionRadius; } }

	[Tooltip("The logic representation of an attack.")]
	[SerializeField]
	private List<PlayerAttack> m_attacks;
	public List<PlayerAttack> attacks => m_attacks;
	
	public bool HasTarget { get; private set; }

	private bool onCombat = false;
	public PlayerAttack currentAttack => attacks[currentAttackIndex];

	private int currentAttackIndex = 0;

	//Track attack animation variables.
	public int currentAttackFrame = 0;
	private int currentHitMarkerIndex = 0;

	private readonly List<Transform> targetableEnemies = new List<Transform>();
	private Transform targetEnemy;
	private Collider targetEnemyCollider;

	private Coroutine computeComboCoroutine = null;
	private Coroutine updateTargetCoroutine = null;
	private bool addFrame = true;

	#endregion

	#region External Properties

	private CameraBehaviour cam { get { return PlayerCenterControl.Instance.camera; } }
	private Animator anim { get { return PlayerCenterControl.Instance.anim; } }

	#endregion

	#region Common Methods

	// Update is called once per frame
	private void Update()
	{
		anim.SetBool("On Combat", onCombat);
	}

	private void LateUpdate()
	{
		anim.ResetTrigger("Attack");
	}

	public void ProccessAttackAnimation()
	{
		anim.SetInteger("Attack Index", currentAttackIndex);
		anim.SetBool("Is Attacking", true);
		anim.SetTrigger("Attack");

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
			HasTarget = true;
			updateTargetCoroutine = StartCoroutine(UpdateTarget());
		}
	}

	private bool FindTarget()
	{
		if (targetableEnemies.Count < 1) return false;

		float closest = m_enemyDetectionRadius + 10f;

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
		targetEnemy = null;
		HasTarget = false;
		cam.SecondaryTarget = null;
		StopCoroutine(updateTargetCoroutine);
	}

	public void FinishAnimation()
	{
		if (currentAttackIndex == m_attacks.Count - 1)
			currentAttackIndex = 0;
		else
		{
			computeComboCoroutine = StartCoroutine(ComputeCombo());
			currentAttackIndex++;
		}

		currentAttackFrame = 0;
		currentHitMarkerIndex = 0;
		anim.SetBool("Is Attacking", false);
	}

	public void UpdateAnimation()
	{
		if (currentHitMarkerIndex < currentAttack.hitMarkersTime.Length)
		{
			var hitMarkerTime = currentAttack.hitMarkersTime[currentHitMarkerIndex];

			if (hitMarkersActive)
			{
				if (currentAttackFrame == hitMarkerTime.y)
				{
					DeactivateMarkers();
					currentHitMarkerIndex++;
				}
			}
			else
			{
				if (hitMarkerTime != null)
				{
					if (currentAttackFrame == hitMarkerTime.x)
					{
						ActivateMarkers();
					}
				}
			}
		}

		if (currentAttack.m_30FramesSample)
		{
			if (addFrame)
			{
				currentAttackFrame++;
				addFrame = false;
			}
			else
			{
				addFrame = true;
			}
		}
		else
		{
			currentAttackFrame++;
		}
		
	}

	#endregion

	#region Coroutines

	private IEnumerator ComputeCombo()
	{
		yield return new WaitForSeconds(currentAttack.timeToBlendCombo);
		currentAttackIndex = 0;
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
				transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime * 2f);
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

	protected override void DoDamage(IDamageable dmg)
	{
		dmg.TakeDamage(currentStats.strength * currentAttack.damageMultiplier, DamageType.MeleeAttack);
	}

	#endregion
}