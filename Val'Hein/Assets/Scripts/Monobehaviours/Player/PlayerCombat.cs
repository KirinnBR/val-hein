using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
	public bool continuousDamage = false;
	public float continuousDamageInterval = 1f;
	[Tooltip("The radius of detection of an enemy.")]
	public float enemyDetectionRadius = 10f;
	public bool hasWeapon = true;
	public List<HitMarker> hitMarkers;
	public Attack[] attacks;
	public Weapon weapon;

	public bool HasTarget { get; private set; }
	public bool IsAttacking { get; private set; } = false;
	private float CurrentHealth { get; set; }
	private int CurrentAttackCombo { get; set; } = 0;
	private bool waitingForEndCombat = false;
	private bool onCombat = false;
	private bool LastHit => attacks.Length == CurrentAttackCombo + 1;
	private Attack CurrentAttack => attacks[CurrentAttackCombo];
	private Transform targetEnemy;
	private Stats Stats { get { return PlayerCenterControl.Instance.playerStats; } }
	private ArmorStatsIncreaser Armor { get { return PlayerCenterControl.Instance.playerArmor; } }
	private List<Transform> focusedEnemies = new List<Transform>();
	private Coroutine computeComboCoroutine = null;
	private Coroutine activateCollisionsCoroutine = null;
	private Coroutine waitForCombatTimeCoroutine = null;

	#endregion

	#region Input Settings

	public MouseButtonCode buttonToAttack = MouseButtonCode.LeftButton;
	public KeyCode keyToTarget = KeyCode.F;
	
	private bool attackInput, targetInput;

	#endregion

	private CameraBehaviour Camera { get { return PlayerCenterControl.Instance.playerCamera; } }
	private PlayerController Controller { get { return PlayerCenterControl.Instance.playerController; } }

	private Animator anim;

	// Start is called before the first frame update
	private void Start()
    {
		anim = GetComponent<Animator>();
		CurrentHealth = Stats.baseHealth;
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
		var enemies = Physics.OverlapSphere(transform.position, enemyDetectionRadius);
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

	private IEnumerator UpdateTarget()
	{
		while (targetEnemy != null)
		{
			if (Vector3.Distance(transform.position, targetEnemy.position) > enemyDetectionRadius)
			{
				focusedEnemies.Clear();
				targetEnemy = null;
				HasTarget = false;
				Camera.Focus = null;
				break;
			}
			var look = Quaternion.LookRotation(new Vector3(targetEnemy.position.x, transform.position.y, targetEnemy.position.z) - transform.position);
			transform.rotation = Quaternion.Lerp(transform.rotation, look, Controller.turnSpeed * Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}
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

	private IEnumerator WaitForCombatTime()
	{
		yield return new WaitForSeconds(maxSecondsToEndCombat);
		waitingForEndCombat = false;
	}

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

	private IEnumerator ComputeCombo()
	{
		yield return new WaitForSeconds(comboValidationSeconds);
		CurrentAttackCombo = 0;
		computeComboCoroutine = null;
	}

	public void ActivateCollisions()
	{
		if (continuousDamage)
			activateCollisionsCoroutine = StartCoroutine(CheckCollisionsContinuous());
		else
			activateCollisionsCoroutine = StartCoroutine(CheckCollisions());
	}

	private IEnumerator CheckCollisions()
	{
		List<IDamageable> nonHitables = new List<IDamageable>();
		while (true)
		{
			foreach (var marker in hitMarkers)
			{
				if (marker.TryGetDamageable(out IDamageable dmg) && !nonHitables.Contains(dmg))
				{
					dmg.TakeDamage(Stats.baseStrength * CurrentAttack.damageMultiplier);
					nonHitables.Add(dmg);
				}
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator CheckCollisionsContinuous()
	{
		List<IDamageable> damageables = new List<IDamageable>();
		bool canHit = true;
		while (true)
		{
			if (canHit)
			{
				foreach (var marker in hitMarkers)
				{
					if (marker.TryGetDamageable(out IDamageable dmg) && !damageables.Contains(dmg))
					{
						dmg.TakeDamage(Stats.baseStrength * CurrentAttack.damageMultiplier);
						damageables.Add(dmg);
						canHit = false;
					}
				}
			}
			else
			{
				yield return new WaitForSeconds(continuousDamageInterval);
				canHit = true;
			}
			damageables.Clear();
			yield return new WaitForEndOfFrame();
		}
	}

	public void DeactivateCollisions(DeactivationType finish)
	{
		StopCoroutine(activateCollisionsCoroutine);
		activateCollisionsCoroutine = null;

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

}

public enum DeactivationType
{
	DoNotFinishAnimation, FinishAnimation
}