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
	[Tooltip("The time, in seconds, to validate the combo.")]
	[SerializeField]
	private float comboValidationSeconds = 1f;
	public bool continuousDamage = false;
	[HideInInspector]
	public float continuousDamageInterval = 1f;
	[Space(20f)]
	[SerializeField]
	private List<CollisionMarker> collisionMarkers;

	public float CurrentHealth { get; private set; }
	private int CurrentAttackCombo { get; set; } = 0;
	private bool IsAttacking { get; set; } = false;
	private bool ActiveCollisions { get; set; } = false;
	private bool waitingForEndCombat = false;
	private float currentDelayToEndCombat;
	private bool onCombat = false;
	private float currentIntervalToHitAgain;
	private bool canHitAgain = false;
	private bool alreadyHit = false;

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
		//TryCooldown();
		GetInput();
		ProccessInput();
		UpdateAnimationVariables();
		CombatSystem();
	}

	private void CombatSystem()
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
							dmg.TakeDamage(stats.baseStrength);
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
			{
				ProccessAttackAnimation();
				//Attack();
			}
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
		switch (CurrentAttackCombo)
		{
			case (int)AttackComboType.LightAttack:
				anim.SetTrigger("Light Attack");
				break;
			case (int)AttackComboType.NormalAttack:
				anim.SetTrigger("Normal Attack");
				break;
			case (int)AttackComboType.HeavyAttack:
				anim.SetTrigger("Heavy Attack");
				break;
		}
		waitingForEndCombat = true;
		currentDelayToEndCombat = maxSecondsToEndCombat;
		IsAttacking = true;
		ComputeCombo();
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
		if (CurrentAttackCombo == (int)AttackComboType.HeavyAttack) CurrentAttackCombo = -1;

		CurrentAttackCombo++;

		yield return new WaitForSeconds(comboValidationSeconds);

		CurrentAttackCombo = 0;
		attackCoroutine = null;
	}

	public void InvalidateAnimation()
	{
		IsAttacking = false;
	}

	public void ActivateCollisions()
	{
		ActiveCollisions = true;
	}

	public void DeactivateCollisions()
	{
		ActiveCollisions = false;
		alreadyHit = false;
		canHitAgain = true;
		currentIntervalToHitAgain = continuousDamageInterval;
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
	[SerializeField]
	private AnimationClip animation;
	[SerializeField]
	private AttackComboType type;
	[Rename("Animator Name")]
	public string name;
}

public enum AttackComboType : int
{
	LightAttack = 0, NormalAttack = 1, HeavyAttack = 2
}
