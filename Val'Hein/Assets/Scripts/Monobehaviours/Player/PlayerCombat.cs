using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
#pragma warning disable CS0649
[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour, IDamageable
{
	#region Combat Settings

	[Header("Combat Settings")]
	
	[Tooltip("The time, in seconds, it takes for the player to stop the combat mode.")]
	public float maxSecondsToEndCombat = 5f;
	[Tooltip("The layer to search for enemies to combat.")]
	public LayerMask combatLayer;
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
	public float CurrentHealth { get; private set; }
	private int CurrentAttackCombo { get; set; } = 0;
	public bool IsAttacking { get; private set; } = false;
	private bool ActiveCollisions { get; set; } = false;
	private bool waitingForEndCombat = false;
	private bool canHitAgain = false;
	private bool alreadyHit = false;
	private bool onCombat = false;
	private bool LastHit => attacks.Length == CurrentAttackCombo + 1;
	private Attack CurrentAttack => attacks[CurrentAttackCombo];
	private Transform targetEnemy;
	private float currentDelayToEndCombat;
	private float currentIntervalToHitAgain;
	private Stats Stats { get { return PlayerCenterControl.Instance.playerStats; } }
	private ArmorStatsIncreaser Armor { get { return PlayerCenterControl.Instance.playerArmor; } }
	private Coroutine computeComboCoroutine;

	#endregion

	#region Input Settings

	[Header("Input Settings")]

	[SerializeField]
	private MouseButtonCode buttonToAttack = MouseButtonCode.LeftButton;
	[SerializeField]
	private KeyCode keyToFocus = KeyCode.F;
	
	private bool attackInput, focusInput;

	#endregion

	private CameraBehaviour Camera { get { return PlayerCenterControl.Instance.playerCamera; } }
	private PlayerController Controller { get { return PlayerCenterControl.Instance.playerController; } }

	private Animator anim;
	

	// Start is called before the first frame update
	private void Start()
    {
		anim = GetComponent<Animator>();
		CurrentHealth = Stats.baseHealth;
		currentDelayToEndCombat = maxSecondsToEndCombat;
		currentIntervalToHitAgain = continuousDamageInterval;
    }

	// Update is called once per frame
	private void Update()
    {
		if (GameManager.IsInitialized && GameManager.Instance.CurrentGameState != GameManager.GameState.Running) return;
		GetInput();
		ProccessInput();
		UpdateTarget();
		TryCombat();
		UpdateAnimationVariables();
	}

	private void GetInput()
	{
		attackInput = Input.GetMouseButtonDown((int)buttonToAttack);
		focusInput = Input.GetKeyDown(keyToFocus);
	}

	private void ProccessInput()
	{
		if (attackInput)
			if (!IsAttacking && !Controller.IsJumping)
				ProccessAttackAnimation();
		if (focusInput)
			SetTarget();
	}

	private void UpdateTarget()
	{
		if (targetEnemy != null)
		{
			if (Vector3.Distance(transform.position, targetEnemy.position) > enemyDetectionRadius)
			{
				targetEnemy = null;
				HasTarget = false;
				Camera.Focus = null;
				return;
			}
			var look = Quaternion.LookRotation(new Vector3(targetEnemy.position.x, transform.position.y, targetEnemy.position.z) - transform.position);
			transform.rotation = Quaternion.Lerp(transform.rotation, look, Controller.turnSpeed * Time.deltaTime);
		}
		else
			HasTarget = false;
	}

	private void TryCombat()
	{
		if (ActiveCollisions)
		{
			List<IDamageable> damageables = new List<IDamageable>();
			if (continuousDamage)
			{
				if (canHitAgain)
				{
					foreach (var marker in hitMarkers)
					{
						if (marker.TryGetDamageable(out IDamageable dmg) && !damageables.Contains(dmg))
						{
							
							dmg.TakeDamage(Stats.baseStrength);
							damageables.Add(dmg);
						}
					}
					canHitAgain = false;
					currentIntervalToHitAgain = continuousDamageInterval;
				}
				else
				{
					currentIntervalToHitAgain -= Time.deltaTime;
					if (currentIntervalToHitAgain <= 0)
						canHitAgain = true;
				}
			}
			else
			{
				if (!alreadyHit)
				{
					foreach (var marker in hitMarkers)
					{
						if (marker.TryGetDamageable(out IDamageable dmg) && !damageables.Contains(dmg))
						{
							dmg.TakeDamage(Stats.baseStrength * CurrentAttack.damageMultiplier);
							damageables.Add(dmg);
							alreadyHit = true;
						}
					}
				}
			}
			damageables.Clear();
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
			if (waitingForEndCombat)
			{
				if (currentDelayToEndCombat <= 0)
				{
					waitingForEndCombat = false;
					currentDelayToEndCombat = maxSecondsToEndCombat;
				}
				else
				{
					currentDelayToEndCombat -= Time.deltaTime;
				}
			}
			else
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

			if (currentEnemy.Equals(this.transform)) continue;
			if (targetEnemy != null && targetEnemy.Equals(currentEnemy)) continue;

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

		if (HasTarget)
			onCombat = true;
	}

	private void ProccessAttackAnimation()
	{
		anim.SetTrigger(CurrentAttack.triggerName);
		waitingForEndCombat = true;
		currentDelayToEndCombat = maxSecondsToEndCombat;
		IsAttacking = true;
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
		ActiveCollisions = true;
	}

	//Finish = 0: deactivate collisions, but not finish the animation.
	//Finish = 1: deactivate collisions and finish the animation.
	public void DeactivateCollisions(int finish)
	{
		ActiveCollisions = false;
		alreadyHit = false;
		canHitAgain = true;
		currentIntervalToHitAgain = continuousDamageInterval;

		if (finish == 1)
		{
			if (LastHit)
				CurrentAttackCombo = -1;

			CurrentAttackCombo++;
			IsAttacking = false;

			if (computeComboCoroutine != null)
				StopCoroutine(computeComboCoroutine);
			computeComboCoroutine = StartCoroutine(ComputeCombo());
		}
			
	}

}