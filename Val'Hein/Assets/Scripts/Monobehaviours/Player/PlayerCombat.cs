using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
#pragma warning disable CS0649
[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour, IDamageable
{
	#region Combat Settings
	
	[Tooltip("The layer to search for enemies to combat.")]
	public LayerMask combatLayer;
	[Tooltip("The time, in seconds, it takes for the player to stop the combat mode.")]
	public float maxSecondsToEndCombat = 5f;
	[Tooltip("The time, in seconds, to validate the combo. PS: It validates after the attack is dealt.")]
	public float comboValidationSeconds = 1f;
	[Tooltip("Is the hit continous?")]
	public bool continuousDamage = false;
	[Tooltip("The interval, in seconds, it takes to detect another hit when the damage is continuous.")]
	public float continuousDamageInterval = 1f;
	[Tooltip("The radius of detection of an enemy.")]
	public float enemyDetectionRadius = 10f;
	[Tooltip("Does the player holds some kind of weapon?")]
	public bool hasWeapon = true;
	[Tooltip("The hit boxex of the attacks.")]
	public List<HitMarker> hitMarkers;
	[Tooltip("Utility class that helps the configuration of the referenced markers.")]
	public HitMarkerManager hitMarkersManager;
	[Tooltip("All of the attacks that the player can do.")]
	public List<Attack> attacks;
	[Tooltip("The weapon the player is holding")]
	public Weapon weapon;

	public bool HasTarget { get; private set; }
	public bool IsAttacking { get; private set; } = false;
	private float CurrentHealth { get; set; }
	private int CurrentAttackCombo { get; set; } = 0;
	private bool waitingForEndCombat = false;
	private bool onCombat = false;
	private bool LastHit => attacks.Count == CurrentAttackCombo + 1;
	private Attack CurrentAttack => attacks[CurrentAttackCombo];
	private Transform targetEnemy;
	private Stats Stats { get { return PlayerCenterControl.Instance.playerStats; } }
	private ArmorStatsIncreaser Armor { get { return PlayerCenterControl.Instance.playerArmor; } }
	private List<Transform> focusedEnemies = new List<Transform>();
	private Coroutine computeComboCoroutine = null;
	private Coroutine activeMarkersCoroutine = null;
	private Coroutine waitForCombatTimeCoroutine = null;

	#endregion

	#region Input Settings

	public MouseButtonCode buttonToAttack = MouseButtonCode.LeftButton;
	public KeyCode keyToTarget = KeyCode.F;
	
	private bool attackInput, targetInput;

	#endregion

	#region External Properties

	private CameraBehaviour Camera { get { return PlayerCenterControl.Instance.playerCamera; } }
	private PlayerController Controller { get { return PlayerCenterControl.Instance.playerController; } }

	private Animator anim;

	#endregion

	#region Common Methods

	// Start is called before the first frame update
	private void Start()
	{
		anim = GetComponent<Animator>();
		CurrentHealth = Stats.baseHealth;
		if (hasWeapon)
			weapon.MergeStatsWithUser(Stats);
		else
			hitMarkersManager.ConfigureMarkers(hitMarkers.ToArray());
	}

	// Update is called once per frame
	private void Update()
	{
		if (GameManager.IsInitialized && GameManager.Instance.CurrentGameState != GameManager.GameState.Running) return;
		GetInput();
		ProccessInput();
		UpdateAnimationVariables();
	}

	private void GetInput()
	{
		attackInput = Input.GetMouseButtonDown((int)buttonToAttack);
		targetInput = Input.GetKeyDown(keyToTarget);
	}

	private void ProccessInput()
	{
		if (attackInput)
			if (!IsAttacking && !Controller.IsJumping)
				ProccessAttackAnimation();
		if (targetInput)
			SetTarget();
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
		var enemies = Physics.OverlapSphere(transform.position, enemyDetectionRadius, combatLayer, QueryTriggerInteraction.Ignore);
		float closestDistance = float.MaxValue;
		Transform closestEnemy = null;
		foreach (var enemy in enemies)
		{
			Transform currentEnemy = enemy.transform;

			if (currentEnemy.Equals(transform)) continue;
			if (targetEnemy != null && targetEnemy.Equals(currentEnemy)) continue;
			if (focusedEnemies.Contains(currentEnemy)) continue;

			float distanceEnemy = Vector3.Distance(transform.position, currentEnemy.position);
			if (distanceEnemy < closestDistance)
			{
				closestDistance = distanceEnemy;
				closestEnemy = currentEnemy;
			}
		}

		if (closestEnemy != null && closestEnemy.Equals(targetEnemy))
			targetEnemy = null;
		else
			targetEnemy = closestEnemy;

		HasTarget = targetEnemy != null;

		Camera.Focus = targetEnemy;

		if (targetEnemy != null)
			focusedEnemies.Add(targetEnemy);

		if (HasTarget)
			onCombat = true;

		StartCoroutine(UpdateTarget());
	}

	private void ProccessAttackAnimation()
	{
		anim.SetTrigger(CurrentAttack.triggerName);
		waitingForEndCombat = true;
		IsAttacking = true;
		if (waitForCombatTimeCoroutine != null)
			StopCoroutine(waitForCombatTimeCoroutine);
		waitForCombatTimeCoroutine = StartCoroutine(WaitForCombatTime());
	}

	#endregion

	#region IDamageable Methods

	public void TakeDamage(float ammount)
	{
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
		yield return new WaitForSeconds(comboValidationSeconds);
		CurrentAttackCombo = 0;
		computeComboCoroutine = null;
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
					dmg.TakeDamage(Stats.baseStrength * CurrentAttack.damageMultiplier);
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
						dmg.TakeDamage(Stats.baseStrength * CurrentAttack.damageMultiplier);
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

	private IEnumerator WaitForCombatTime()
	{
		yield return new WaitForSeconds(maxSecondsToEndCombat);
		waitingForEndCombat = false;
	}

	private IEnumerator UpdateTarget()
	{
		while (targetEnemy != null)
		{
			if (Vector3.Distance(transform.position, targetEnemy.position) > enemyDetectionRadius)
			{
				focusedEnemies.Clear();
				targetEnemy = null;
				HasTarget = false;
				Camera.Defocus();
				break;
			}
			var look = Quaternion.LookRotation(new Vector3(targetEnemy.position.x, transform.position.y, targetEnemy.position.z) - transform.position);
			transform.rotation = Quaternion.Lerp(transform.rotation, look, Controller.turnSpeed * Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}
		focusedEnemies.Clear();
		HasTarget = false;
		Camera.Defocus();
	}

    #endregion

    #region Animation Event Methods

    public void ActivateMarkers()
	{
		if (hasWeapon)
			weapon.ActivateMarkers();
		else
		{
			if (continuousDamage)
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
		{
			if (LastHit)
				CurrentAttackCombo = 0;
			else
				CurrentAttackCombo++;

			IsAttacking = false;

			if (computeComboCoroutine != null)
				StopCoroutine(computeComboCoroutine);
			computeComboCoroutine = StartCoroutine(ComputeCombo());
		}
	}

	#endregion

}

public enum DeactivationType
{
	DoNotFinishAnimation, FinishAnimation
}