using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable CS0649
public class CombatSystem : MonoBehaviour
{
	#region Combat Settings

	[Header("Combat Settings")]
	
	[Tooltip("The time, in seconds, it takes for the player to stop the combat mode.")]
	[SerializeField]
	private float maxSecondsToEndCombat = 5f;
	[Tooltip("The radius of detection of an enemy.")]
	[SerializeField]
	private float enemyDetectionRadius = 10f;
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
	public float EnemyDetectionRadius { get { return enemyDetectionRadius; } }
	public bool HasTarget { get; private set; }
	public bool IsAttacking { get; private set; } = false;
	public float CurrentHealth { get; private set; }
	private float CurrentStamina { get; set; }
	private int CurrentAttackIndex { get; set; } = 0;
	private HitMarkerManager hitMarkerManager { get { return combatSettings.hitMarkerManager; } }
	private List<HitMarker> hitMarkers { get { return combatSettings.hitMarkers; } }
	private PlayerAttack CurrentAttack => attacks[CurrentAttackIndex];
	private bool LastHit => CurrentAttackIndex == attacks.Count - 1;
	private bool waitingForEndCombat = false;
	private bool onCombat = false;
	private bool animationFinished = false;
	private Transform targetEnemy;
	private Collider targetCollider;
	private int targetEnemyIndex;

	private List<Transform> focusedEnemies = new List<Transform>();
	private Coroutine computeComboCoroutine = null;
	private Coroutine activeMarkersCoroutine = null;
	private Coroutine waitForCombatTimeCoroutine = null;
	private Coroutine updateTargetCoroutine = null;

	public class OnTakeDamageEvent : UnityEvent<float> { }

	public OnTakeDamageEvent OnTakeDamage = new OnTakeDamageEvent();

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

	// Start is called before the first frame update
	void Start()
	{
		CurrentHealth = stats.baseHealth;
		CurrentStamina = stats.baseStamina;
		if (hasWeapon)
			weapon.MergeStatsWithUser(stats);
		else
			hitMarkerManager.ConfigureMarkers(hitMarkers.ToArray());
		OnTakeDamage.AddListener(ui.UpdateHealthBar);
	}
	// Update is called once per frame
	private void Update()
	{
		if (GameManager.IsInitialized && GameManager.Instance.CurrentGameState != GameManager.GameState.Running) return;
		ProccessInput();
		UpdateAnimationVariables();
	}

	private void DoDamage(IDamageable dmg) => dmg.TakeDamage(stats.baseStrength * CurrentAttack.damageMultiplier);

	private void ProccessInput()
	{
		if (input.Attack)
			if (!IsAttacking && !controller.IsJumping && !controller.IsDodging)
				ProccessAttackAnimation();
		if (input.Target)
		{
			if (!HasTarget)
				SetTarget();
			else
				UnsetTarget();
		}
	}

	private void UpdateAnimationVariables()
	{
		if (IsAttacking)
		{
			if (!onCombat)
			{
				onCombat = true;
				anim.SetBool("On Combat", onCombat);
			}
		}
		else
		{
			if (!waitingForEndCombat)
			{
				if (onCombat)
				{
					if (!HasTarget)
						onCombat = false;
					anim.SetBool("On Combat", onCombat);
				}
			}
		}
	}

	private void SetTarget()
	{
		//Quando setar, pega todos os inimigos e calcula o mais perto. Depois, inicia a co-rotina UpdateTarget().
		var enemies = Physics.OverlapSphere(transform.position, enemyDetectionRadius, combatLayer, QueryTriggerInteraction.UseGlobal);
		float closestDistance = float.MaxValue;
		Transform closestEnemy = null;
		foreach (var enemy in enemies)
		{
			Transform currentEnemy = enemy.transform;

			if (currentEnemy.Equals(transform)) continue;

			float distanceEnemy = Vector3.Distance(transform.position, currentEnemy.position);
			if (distanceEnemy < closestDistance)
			{
				closestDistance = distanceEnemy;
				closestEnemy = currentEnemy;
			}
			focusedEnemies.Add(currentEnemy);
		}

		targetEnemy = closestEnemy;
		targetEnemyIndex = focusedEnemies.IndexOf(targetEnemy);

		HasTarget = targetEnemy != null;

		cam.Focus = targetEnemy;

		if (HasTarget)
		{
			targetCollider = targetEnemy.GetComponent<Collider>();
			onCombat = true;
		}

		if (updateTargetCoroutine != null)
			StopCoroutine(updateTargetCoroutine);
		updateTargetCoroutine = StartCoroutine(UpdateTarget());
	}

	private void UnsetTarget()
	{
		focusedEnemies.Clear();
		targetEnemy = null;
		HasTarget = false;
		cam.Focus = null;
		updateTargetCoroutine = null;
	}

	private void ProccessAttackAnimation()
	{
		if (LastHit)
			StopCoroutine(computeComboCoroutine);
		else
		{
			if (computeComboCoroutine != null)
				StopCoroutine(computeComboCoroutine);
			computeComboCoroutine = StartCoroutine(ComputeCombo());
		}

		animationFinished = false;

		anim.SetInteger("Attack Index", CurrentAttackIndex);
		anim.SetTrigger("Attack");

		IsAttacking = true;
		controller.MovementBlocked = true;
		controller.RotationBlocked = true;
		waitingForEndCombat = true;

		if (waitForCombatTimeCoroutine != null)
			StopCoroutine(waitForCombatTimeCoroutine);
		waitForCombatTimeCoroutine = StartCoroutine(WaitForCombatTime());
	}

	#endregion

	#region IDamageable Methods

	public void TakeDamage(float ammount)
	{
		FinishAnimation();
		anim.SetTrigger("Hurt");
		CurrentHealth -= ammount;
		OnTakeDamage.Invoke(CurrentHealth);
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

	private IEnumerator ComputeCombo()
	{
		int aux = CurrentAttackIndex;
		yield return new WaitForSeconds(CurrentAttack.timeToBlendCombo);
		Debug.Log("Stopped at index " + aux);
		CurrentAttackIndex = 0;
		computeComboCoroutine = null;
		yield break;
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

	private IEnumerator WaitForCombatTime()
	{
		yield return new WaitForSeconds(maxSecondsToEndCombat);
		waitingForEndCombat = false;
		IsAttacking = false;
	}

	private IEnumerator UpdateTarget()
	{

		IEnumerator CheckForTargets()
		{
			while (true)
			{
				Collider[] enemies = Physics.OverlapSphere(transform.position, enemyDetectionRadius, combatLayer, QueryTriggerInteraction.UseGlobal);
				focusedEnemies.Clear();
				foreach (var enemy in enemies)
				{
					var enemyT = enemy.transform;
					if (transform.Equals(enemyT)) continue;
					focusedEnemies.Add(enemyT);
				}
				yield return new WaitForSeconds(0.1f);
			}
		}

		bool completeTwirl = false;

		void SetNewFocus()
		{
			targetEnemy = focusedEnemies[targetEnemyIndex];
			cam.Focus = targetEnemy;
			targetCollider = targetEnemy.GetComponent<Collider>();
			completeTwirl = false;
		}

		Coroutine checkForTargetsCoroutine = StartCoroutine(CheckForTargets());
		while (targetEnemy != null)
		{
			if (Vector3.Distance(transform.position, targetEnemy.position) > enemyDetectionRadius + 0.2f) break;

			if (targetCollider.enabled == false)
			{
				if (focusedEnemies.Count == 0)
					break;
				targetEnemyIndex = focusedEnemies.FindIndex(enemy => enemy.transform != null);
				SetNewFocus();
			}

			if (input.MouseScrollWheel > 0)
			{
				targetEnemyIndex++;
				if (targetEnemyIndex == focusedEnemies.Count) targetEnemyIndex = 0;
				SetNewFocus();
			}

			if (input.MouseScrollWheel < 0)
			{
				targetEnemyIndex--;
				if (targetEnemyIndex < 0) targetEnemyIndex = focusedEnemies.Count - 1;
				SetNewFocus();
			}

			if (completeTwirl)
			{
				transform.LookAt(new Vector3(targetEnemy.position.x, transform.position.y, targetEnemy.position.z));
			}
			else
			{
				var look = Quaternion.LookRotation(new Vector3(targetEnemy.position.x, transform.position.y, targetEnemy.position.z) - transform.position);
				transform.rotation = Quaternion.Lerp(transform.rotation, look, controller.turnSpeed * Time.deltaTime);
				if (Quaternion.Angle(transform.rotation, look) < 5f)
					completeTwirl = true;
			}
			yield return null;
		}

		StopCoroutine(checkForTargetsCoroutine);
		UnsetTarget();
	}

	private IEnumerator UpdateStamina()
	{
		CurrentStamina -= 10f;
		yield return new WaitForSeconds(2.5f);
		while (CurrentStamina < stats.baseStamina)
		{
			CurrentStamina += 0.5f;
			yield return null;
		}
		CurrentStamina = stats.baseStamina;
	}

	#endregion

	#region Animation Event Methods

	public void ActivateMarkers()
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

	public void DeactivateMarkers(DeactivationType finish)
	{
		if (hasWeapon)
			weapon.DeactivateMarkers();
		else
			StopCoroutine(activeMarkersCoroutine);

		if (finish == DeactivationType.FinishAnimation)
			FinishAnimation();
	}

	public void FinishAnimation()
	{
		if (animationFinished) return;

		controller.MovementBlocked = false;
		controller.RotationBlocked = false;
		IsAttacking = false;
		animationFinished = true;

		if (LastHit)
			CurrentAttackIndex = 0;
		else
			CurrentAttackIndex++;
	}

	#endregion
}
