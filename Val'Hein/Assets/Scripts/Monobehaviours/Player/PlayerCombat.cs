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
	[Tooltip("The delay, in seconds, it takes to attack again.")]
	[SerializeField]
	private float timeBetweenAttacks = 0.5f;
	[SerializeField]
	[Tooltip("The layer to search for enemies to combat.")]
	private LayerMask combatLayer;
	[Tooltip("The time, in seconds, to validate the combo.")]
	[SerializeField]
	private float comboValidationSeconds = 1f;

	private int CurrentAttackCombo { get; set; } = 0;

	#endregion

	#region Input Settings

	[Header("Input Settings")]

	[SerializeField]
	private MouseButtonCode buttonToAttack = MouseButtonCode.LeftButton;

	private bool attackInput;

	#endregion

	#region Motion Calibration Settings

	[Header("Motion Calibration Settings")]

	[Tooltip("The size of the hitbox.")]
	public Vector3 hitBoxSize = Vector3.one * 0.5f;
	[Tooltip("The position offset that calibrates the hitbox.")]
	public Vector3 hitBoxOffset = new Vector3(0f, 1f, 0.75f);

	#endregion

	public float CurrentHealth { get; private set; }

	private bool CanAttack { get; set; } = true;
	private float currentCooldown = 0;
	private Coroutine attackCoroutine;
	private Animator anim;
	// Start is called before the first frame update
	void Start()
    {
		anim = GetComponent<Animator>();
		CurrentHealth = stats.baseHealth;
    }

    // Update is called once per frame
    void Update()
    {
		if (GameManager.IsInitialized && GameManager.Instance.CurrentGameState != GameManager.GameState.Running) return;
		UpdateCooldown();
		CalculateInput();
		TryCombat();
	}

	private void UpdateCooldown()
	{
		if (!CanAttack)
		{
			currentCooldown -= Time.deltaTime;
			if (currentCooldown <= 0)
			{
				CanAttack = true;
			}
		}
	}

	private void CalculateInput()
	{
		attackInput = Input.GetMouseButtonDown((int)buttonToAttack);
	}

	private void TryCombat()
	{
		if (CanAttack)
		{
			if (attackInput)
			{
				ProccessAttackAnimation();
				ActivateCooldown();
			}
		}
	}

	private void Attack()
	{
		var objects = Physics.OverlapBox(transform.position + hitBoxOffset + transform.forward, hitBoxSize / 2, Quaternion.LookRotation(transform.forward), combatLayer, QueryTriggerInteraction.Ignore);
		bool resetedCombo = false;
		foreach (var obj in objects)
		{
			if (obj.TryGetComponent(out IDamageable dmg))
			{
				//Stops the last combo validation and starts a new one.
				if (!resetedCombo)
				{
					if (attackCoroutine != null)
						StopCoroutine(attackCoroutine);
					attackCoroutine = StartCoroutine(AttackComboValidation());
					resetedCombo = true;
				}

				//Do damage to damageable objects inside the hitbox.
				dmg.TakeDamage(stats.baseStrength);
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
				CurrentAttackCombo = -1;
				break;
		}
	}

	private void ActivateCooldown()
	{
		CanAttack = false;
		currentCooldown = timeBetweenAttacks;
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
		CurrentAttackCombo++;
		yield return new WaitForSeconds(comboValidationSeconds);
		CurrentAttackCombo = 0;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position + hitBoxOffset + transform.forward, hitBoxSize);
	}

}

public enum AttackComboType : int
{
	LightAttack = 0, NormalAttack = 1, HeavyAttack = 3
}
