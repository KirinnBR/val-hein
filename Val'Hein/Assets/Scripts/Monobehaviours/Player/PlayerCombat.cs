using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649

public class PlayerCombat : MonoBehaviour, IDamageable
{
	#region Combat Settings
	
	[Header("Combat Settings")]

	[Tooltip("The layer to search for enemies to combat.")]
	[SerializeField]
	private LayerMask combatLayer;
	[Tooltip("The time, in seconds, it takes for the player to stop the combat mode.")]
	[SerializeField]
	private float maxSecondsToEndCombat = 5f;
	[Tooltip("The radius of detection of an enemy.")]
	[SerializeField]
	private float enemyDetectionRadius = 10f;
	[Tooltip("The logic representation of an attack.")]
	[SerializeField]
	private List<ComboAttack> attacks;
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
	private ComboAttack CurrentAttack => attacks[CurrentAttackIndex];
	private bool LastHit => CurrentAttackIndex == attacks.Count - 1;
	private bool waitingForEndCombat = false;
	private bool onCombat = false;
	private bool animationFinished = false;
	private Transform targetEnemy;
	private int targetEnemyIndex;
	
	private List<Transform> focusedEnemies = new List<Transform>();
	private Coroutine computeComboCoroutine = null;
	private Coroutine activeMarkersCoroutine = null;
	private Coroutine waitForCombatTimeCoroutine = null;
	private Coroutine updateTargetCoroutine = null;

	#endregion

	#region Input Settings

	[Header("Input Settings")]

	public MouseButtonCode buttonToAttack = MouseButtonCode.LeftButton;
	public KeyCode keyToTarget = KeyCode.F;
	//[SerializeField]
	//private MouseButtonCode buttonToRaiseGuard = MouseButtonCode.RightButton;
	//[SerializeField]
	//private bool holdButton = false;
	
	private bool attackInput, targetInput;
	private float mouseScrollInput;

	#endregion

	#region External Properties

	private Stats stats { get { return Player.Instance.playerStats; } }
	private CameraBehaviour cam { get { return Player.Instance.playerCamera; } }
	private PlayerController controller { get { return Player.Instance.playerController; } }
	private Animator anim { get { return Player.Instance.anim; } }

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
	}
	// Update is called once per frame
	private void Update()
	{
		if (GameManager.IsInitialized && GameManager.Instance.CurrentGameState != GameManager.GameState.Running) return;
		GetInput();
		ProccessInput();
		UpdateAnimationVariables();
	}

	private void DoDamage(IDamageable dmg) => dmg.TakeDamage(stats.baseStrength * CurrentAttack.damageMultiplier);

	private void GetInput()
	{
		attackInput = Input.GetMouseButtonDown((int)buttonToAttack);
		targetInput = Input.GetKeyDown(keyToTarget);
		//raiseGuardInput = holdButton ? Input.GetMouseButton((int)buttonToRaiseGuard) : Input.GetMouseButtonDown((int)buttonToRaiseGuard);
		mouseScrollInput = -Input.GetAxisRaw("Mouse ScrollWheel");
	}

	private void ProccessInput()
	{
		if (attackInput)
			if (!IsAttacking && !controller.IsJumping && !controller.IsDodging)
				ProccessAttackAnimation();
		if (targetInput)
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
		//anim.SetBool("On Guard", IsGuard);
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

		cam.SetFocus(targetEnemy);

		if (HasTarget)
			onCombat = true;

		if (updateTargetCoroutine != null)
			StopCoroutine(updateTargetCoroutine);
		updateTargetCoroutine = StartCoroutine(UpdateTarget());
	}

	private void UnsetTarget()
	{
		focusedEnemies.Clear();
		targetEnemy = null;
		HasTarget = false;
		cam.SetFocus(null);
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

		Coroutine checkForTargetsCoroutine = StartCoroutine(CheckForTargets());

		while (targetEnemy != null)
		{
			if (Vector3.Distance(transform.position, targetEnemy.position) > enemyDetectionRadius + 0.2f) { break; }

			if (mouseScrollInput > 0)
			{
				targetEnemyIndex++;
				if (targetEnemyIndex == focusedEnemies.Count) targetEnemyIndex = 0;
				
				targetEnemy = focusedEnemies[targetEnemyIndex];
				cam.SetFocus(targetEnemy);
			}

			if (mouseScrollInput < 0)
			{
				targetEnemyIndex--;
				if (targetEnemyIndex < 0) targetEnemyIndex = focusedEnemies.Count - 1;

				targetEnemy = focusedEnemies[targetEnemyIndex];
				cam.SetFocus(targetEnemy);
			}

			//var look = Quaternion.LookRotation(new Vector3(targetEnemy.position.x, transform.position.y, targetEnemy.position.z) - transform.position);
			//transform.rotation = Quaternion.Lerp(transform.rotation, look, Controller.turnSpeed * Time.deltaTime);

			transform.LookAt(new Vector3(targetEnemy.position.x, transform.position.y, targetEnemy.position.z));
			
			yield return null;
		}

		StopCoroutine(checkForTargetsCoroutine);
		UnsetTarget();
		yield break;
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

		IsAttacking = false;
		animationFinished = true;

		if (LastHit)
			CurrentAttackIndex = 0;
		else
			CurrentAttackIndex++;
	}

	#endregion
}