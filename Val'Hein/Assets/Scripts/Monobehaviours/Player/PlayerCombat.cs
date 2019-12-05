using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour, IDamageable
{
	#region Combat Settings

	[Header("Combat Settings")]

	[Tooltip("The stats of the player.")]
	[SerializeField]
	private Stats stats;
	[Tooltip("The armor that the player uses")]
	[SerializeField]
	private ArmorStatsIncreaser armor;
	[Tooltip("The time, in seconds, it takes for the player to stop the combat mode.")]
	[SerializeField]
	private float maxSecondsToEndCombat = 5f;
	[SerializeField]
	[Tooltip("The layer to search for enemies to combat.")]
	private LayerMask combatLayer;
	[Tooltip("The time, in seconds, to validate the combo. PS: It validates after the attack is dealt.")]
	[SerializeField]
	private float comboValidationSeconds = 1f;
	public bool continuousDamage = false;
	[HideInInspector]
	public float continuousDamageInterval = 1f;
	[Space(20f)]
	[SerializeField]
	private List<CollisionMarker> collisionMarkers;
	[SerializeField]
	private Attack[] attacks;
	
	private int CurrentAttackCombo { get; set; } = 0;
	private bool IsAttacking { get; set; } = false;
	private bool ActiveCollisions { get; set; } = false;
	private bool waitingForEndCombat = false;
	private bool canHitAgain = false;
	private bool alreadyHit = false;
	private bool onCombat = false;
	private bool LastHit => attacks.Length == CurrentAttackCombo + 1;
	private Attack CurrentAttack => attacks[CurrentAttackCombo];
	public float CurrentHealth { get; private set; }
	private float currentDelayToEndCombat;
	private float currentIntervalToHitAgain;
	

	#endregion

	#region Input Settings

	[Header("Input Settings")]
	[SerializeField]
	private MouseButtonCode buttonToAttack = MouseButtonCode.LeftButton;
	
	private bool attackInput;

	#endregion

	private Coroutine attackCoroutine;
	private Animator anim;
	private PlayerController controller;

	// Start is called before the first frame update
	private void Start()
    {
		anim = GetComponent<Animator>();
		controller = GetComponent<PlayerController>();
		CurrentHealth = stats.baseHealth;
		currentDelayToEndCombat = maxSecondsToEndCombat;
		currentIntervalToHitAgain = continuousDamageInterval;
    }

	// Update is called once per frame
	private void Update()
    {
		if (GameManager.IsInitialized && GameManager.Instance.CurrentGameState != GameManager.GameState.Running) return;
		
		GetInput();
		ProccessInput();
		TryCombat();
		UpdateAnimationVariables();
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
					foreach (var marker in collisionMarkers)
					{
						if (marker.TryGetDamageable(out IDamageable dmg) && !damageables.Contains(dmg))
						{
							
							dmg.TakeDamage(stats.baseStrength);
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
					foreach (var marker in collisionMarkers)
					{
						if (marker.TryGetDamageable(out IDamageable dmg) && !damageables.Contains(dmg))
						{
							dmg.TakeDamage(stats.baseStrength * CurrentAttack.damageMultiplier);
							damageables.Add(dmg);
							alreadyHit = true;
						}
					}
				}
			}
			damageables.Clear();
		}
	}

	private void GetInput()
	{
		attackInput = Input.GetMouseButtonDown((int)buttonToAttack);
	}

	private void ProccessInput()
	{
		if (attackInput)
		{
			if (!IsAttacking && !controller.IsJumping)
				ProccessAttackAnimation();
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
					onCombat = false;
					anim.SetBool("On Combat", onCombat);
				}
			}
		}
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
		CurrentHealth -= ammount;
		if (CurrentHealth <= 0)
			Die();
	}

	private void Die()
	{
		CurrentHealth = 0;
		Debug.Log($"{gameObject.name} has died.");
		//Call for endgame.
	}

	private IEnumerator AttackComboValidation()
	{
		yield return new WaitForSeconds(comboValidationSeconds);
		CurrentAttackCombo = 0;
		attackCoroutine = null;
	}

	public void ActivateCollisions()
	{
		ActiveCollisions = true;
	}

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
			ComputeCombo();
		}
			
	}

	private void ComputeCombo()
	{
		if (attackCoroutine != null)
			StopCoroutine(attackCoroutine);
		attackCoroutine = StartCoroutine(AttackComboValidation());
	}
}

[System.Serializable]
public class Attack
{
	[Tooltip("The name of the trigger for the attack to be called.")]
	public string triggerName = "Attack";
	[Tooltip("Multiplier damage a certain attack will deal.")]
	[Range(1f, 10f)]
	public float damageMultiplier = 1f;
}
