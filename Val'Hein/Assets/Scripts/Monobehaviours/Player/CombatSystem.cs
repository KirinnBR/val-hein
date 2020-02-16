using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#pragma warning disable CS0649
public class CombatSystem : MonoBehaviour
{
	#region Combat Settings

	[Header("Combat Settings")]
	
	[Tooltip("The radius of detection of an enemy.")]
	[SerializeField]
	private float enemyDetectionRadius = 10f;
	public float EnemyDetectionRadius { get { return enemyDetectionRadius; } }
	[Tooltip("The logic representation of an attack.")]
	[SerializeField]
	private List<PlayerAttack> attacks;
	[Tooltip("Does the player holds some kind of weapon?")]
	[SerializeField]
	private bool hasWeapon = true;
	[SerializeField]
	[ConditionalHide("hasWeapon", false)]
	private CombatSettings combatSettings;
	[Tooltip("The weapon the player is holding")]
	[ConditionalHide("hasWeapon", true)]
	[SerializeField]
	private Weapon weapon;
	public bool HasTarget { get; private set; }
	public bool CanAttack { get; set; } = true;
	public float CurrentHealth { get; private set; }
	private HitMarkerConfigurer hitMarkerConfigurer { get { return combatSettings.hitMarkerManager; } }
	private HitMarker[] hitMarkers { get { return combatSettings.hitMarkers; } }
	private PlayerAttack CurrentAttack => attacks[currentAttackIndex];
	private bool LastHit => currentAttackIndex == attacks.Count - 1;
	private bool animationFinished = false;
	private int currentAttackIndex = 0;
	private Transform targetEnemy;
	private Collider targetEnemyCollider;
	private int targetEnemyIndex;
	private bool isAttacking = false;
	private bool onCombat = false;

	private List<Transform> targetableEnemies = new List<Transform>();
	private Coroutine computeComboCoroutine = null;
	private Coroutine activeMarkersCoroutine = null;
	private Coroutine trackAnimationCoroutine = null;
	private Coroutine updateTargetCoroutine = null;

	#endregion

	#region External Properties

	private Stats stats { get { return PlayerCenterControl.Instance.playerStats; } }
	private LayerMask combatLayer { get { return PlayerCenterControl.Instance.combatCheckLayer; } }
	private CameraBehaviour cam { get { return PlayerCenterControl.Instance.playerCamera; } }
	private ControllerSystem controller { get { return PlayerCenterControl.Instance.controller; } }
	private InputSystem input { get { return PlayerCenterControl.Instance.input; } }
	private UISystem ui { get { return PlayerCenterControl.Instance.ui; } }
	private Animator anim { get { return PlayerCenterControl.Instance.anim; } }

	#endregion

	#region Common Methods

	void Start()
	{
		CurrentHealth = stats.baseHealth;
		if (hasWeapon)
			weapon.MergeStatsWithUser(stats);
		else
			hitMarkerConfigurer.ConfigureMarkers(hitMarkers);
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
		if (isAttacking || !CanAttack) return;

		animationFinished = false;

		anim.SetInteger("Attack Index", currentAttackIndex);
		anim.SetTrigger("Attack");

		isAttacking = true;
		controller.MovementBlocked = true;
		controller.RotationBlocked = true;

		if (trackAnimationCoroutine != null)
			StopCoroutine(trackAnimationCoroutine);
		trackAnimationCoroutine = StartCoroutine(TrackAnimation());
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
		targetEnemyIndex = targetableEnemies.IndexOf(targetEnemy);
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
		if (hasWeapon)
			weapon.ActivateMarkers(CurrentAttack.damageMultiplier);
		else
		{
			if (combatSettings.continuousDamage)
				activeMarkersCoroutine = StartCoroutine(CheckCollisionsContinuous());
			else
				activeMarkersCoroutine = StartCoroutine(CheckCollisions());
		}
	}

	private void DeactivateMarkers()
	{
		if (hasWeapon)
			weapon.DeactivateMarkers();
		else
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

		if (computeComboCoroutine != null)
			StopCoroutine(computeComboCoroutine);

		if (LastHit)
			currentAttackIndex = 0;
		else
		{
			computeComboCoroutine = StartCoroutine(ComputeCombo());
			currentAttackIndex++;
		}
	}

	#endregion

	#region IDamageable Methods

	public void TakeDamage(float ammount)
	{
		FinishAnimation();
		anim.SetTrigger("Hurt");
		CurrentHealth -= ammount;
		//OnTakeDamage.Invoke(CurrentHealth);
		if (CurrentHealth <= 0)
			Die();
	}

	private void Die()
	{
		CurrentHealth = 0;
		//Call for endgame.
		Destroy(gameObject);
	}

	#endregion

	#region Coroutines

	private IEnumerator TrackAnimation()
	{
		int currentFrame = 1;
		bool isHitMarkersActive = false;
		int currentHitMarkerIndex = 0;
		int hitMarkerLength = CurrentAttack.hitMarkersTime.Length;
		while (true)
		{
			if (currentFrame == CurrentAttack.animationLength)
			{
				FinishAnimation();
				break;
			}

			if (currentHitMarkerIndex < hitMarkerLength)
			{
				var hitMarkerTime = CurrentAttack.hitMarkersTime[currentHitMarkerIndex];

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
		yield return new WaitForSeconds(CurrentAttack.timeToBlendCombo);
		Debug.Log("Stopped at index " + aux);
		currentAttackIndex = 0;
	}

	private void DoDamage(IDamageable dmg) => dmg.TakeDamage(stats.baseStrength * CurrentAttack.damageMultiplier);

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
					continue;
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
				transform.rotation = Quaternion.Slerp(transform.rotation, look, controller.turnSpeed * Time.deltaTime * 2f);
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
