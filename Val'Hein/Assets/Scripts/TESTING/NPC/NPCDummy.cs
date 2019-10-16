using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NPCDummy : NPC, IDamageable
{
	#region Agent Settings
	[Header("Agent Settings")]
	[Tooltip("Speed when patrolling.")]
	public float patrollingSpeed = 5f;
	[Tooltip("Speed when chasing enemy.")]
	public float pursuitSpeed = 15f;
	public bool IsCloseEnoughToDestination => Vector3.Distance(agent.destination, transform.position) < agent.stoppingDistance;
	private NavMeshAgent agent;
	#endregion
	[Space]
	#region Combat Settings
	[Header("Combat Settings")]
	[SerializeField]
	[Tooltip("The delay, in seconds, to attack.")]
	private float attackDelay = 1.5f;
	[SerializeField]
	[Tooltip("The max health points the NPC will have.")]
	private float healthPoints = 200f;
	[SerializeField]
	[Tooltip("The damage points it will inflict.")]
	private float damagePoints = 20f;
	#endregion


	public float CurrentHealth { get; private set; }
	private bool attackOnCooldown = false;
	private float currentCooldown = 0;
	private Vector3 inicialPosition;
	private Animator anim;

	protected override void Start()
	{
		base.Start();
		anim = GetComponent<Animator>();
		agent = GetComponent<NavMeshAgent>();
		agent.acceleration = 80;
		agent.angularSpeed = 1200;
		agent.speed = patrollingSpeed;
		agent.stoppingDistance = 2f;
		CurrentHealth = healthPoints;
		inicialPosition = transform.position;
	}

	// Update is called once per frame
	void Update()
    {
		SearchObjects();
		UpdateCooldown();
		//Animate walk.
		if (visibleObjects.Count > 0)
		{
			foreach (var visibleObject in visibleObjects)
			{
				//Condicao para ver se o inimigo se interessa em atacar aquele objeto.
				//Atualmente, ele procura o primeiro objeto que seja um player e ataca.
				if (visibleObject.tag == "Player")
				{
					StartPursuit(visibleObject.transform);
				}
			}
		}
		else
		{
			agent.destination = inicialPosition;
		}
	}

	private void UpdateCooldown()
	{
		if (attackOnCooldown)
		{
			currentCooldown -= Time.deltaTime;
			if (currentCooldown <= 0)
			{
				attackOnCooldown = false;
			}
		}
	}

	private void StartPursuit(Transform target)
	{
		transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
		agent.speed = pursuitSpeed;
		agent.SetDestination(target.position);
		if (IsCloseEnoughToTarget(target.position))
		{
			if (!attackOnCooldown)
			{
				Attack(target);
			}
		}
	}

	private void Attack(Transform target)
	{
		//Animation of attack.
		//Sound of attack.
		attackOnCooldown = true;
		currentCooldown = attackDelay;
		if (target.TryGetComponent(out IDamageable dmg))
		{
			Debug.Log($"{gameObject.name} dealt damage to {target.name}.");
			dmg.TakeDamage(damagePoints);
		}
		else
		{
			Debug.Log($"{target.name} is not damageable.");
		}
	}

	private bool IsCloseEnoughToTarget(Vector3 target)
	{
		return Vector3.Distance(transform.position, target) < agent.stoppingDistance;
	}

	void IDamageable.TakeDamage(float ammount)
	{
		CurrentHealth -= ammount;
		if (CurrentHealth <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		CurrentHealth = 0;
		Debug.Log($"{gameObject.name} has died.");
		Destroy(gameObject);
	}

}
