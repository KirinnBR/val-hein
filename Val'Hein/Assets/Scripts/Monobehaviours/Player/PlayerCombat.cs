using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour, IDamageable
{
	#region Combat Settings

	[Header("Combat Settings")]

	[SerializeField]
	private LevelUpableStats stats;
	[SerializeField]
	private ArmorStatsIncreaser armor;
	[SerializeField]
	private float timeBetweenAttacks = 0.5f;
	[SerializeField]
	private LayerMask combatLayer;

	#endregion

	#region Input Settings

	[Header("Input Settings")]

	[SerializeField]
	private MouseButtonCode buttonToAttack = MouseButtonCode.LeftButton;

	private bool lightAttackInput;

	#endregion

	#region Motion Calibration Settings

	[Header("Motion Calibration Settings")]

	[SerializeField]
	private Vector3 hitBoxOffset = new Vector3(0f, 1f, 0.75f);
	[SerializeField]
	private Vector3 hitBoxSize = Vector3.one * 0.5f;

	#endregion

	public float CurrentHealth { get; private set; }

	private bool canAttack = false;
	private float currentCooldown = 0;
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
		CalculateCooldown();
		CalculateInput();
		TryCombat();
	}

	private void CalculateCooldown()
	{
		if (!canAttack)
		{
			currentCooldown -= Time.deltaTime;
			if (currentCooldown <= 0)
			{
				canAttack = true;
			}
		}
	}

	private void CalculateInput()
	{
		lightAttackInput = Input.GetMouseButtonDown((int)buttonToAttack);
	}

	private void TryCombat()
	{
		if (canAttack)
		{
			if (lightAttackInput)
			{
				Attack();
				ActivateCooldown();
			}
		}
	}

	private void Attack()
	{
		anim.SetTrigger("Light Attack");
		var obj = Physics.BoxCast(transform.position + hitBoxOffset, hitBoxSize, transform.forward, out RaycastHit hit, Quaternion.identity, 1f, combatLayer, QueryTriggerInteraction.Ignore);
		if (obj && hit.transform.TryGetComponent(out IDamageable dmg))
		{
			dmg.TakeDamage(stats.baseStrength);
		}
	}

	private void ActivateCooldown()
	{
		canAttack = false;
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

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position + hitBoxOffset, hitBoxSize);
	}

}
