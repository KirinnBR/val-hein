using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#pragma warning disable CS0649
[RequireComponent(typeof(NavMeshAgent))]
public class NPCPatroller : NPC, IDamageable
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
	#region Patrolling Settings
	[Header("Patrolling Settings")]
	[Tooltip("The points to patrol.")]
	public Transform[] patrolPoints;
	[Tooltip("The time to wait between patrol points.")]
	public int patrollingWaitTime = 5;
	private int currentPatrolPoint = 0;
	private bool patrolling = false;
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

	// Start is called before the first frame update
	protected override void Start()
    {
		base.Start();
		agent = GetComponent<NavMeshAgent>();
		agent.acceleration = 80;
		agent.angularSpeed = 1200;
		agent.speed = patrollingSpeed;
		agent.stoppingDistance = 2f;
		CurrentHealth = healthPoints;
    }

	// Update is called once per frame
	private void Update()
    {
		UpdateCooldown();
		SearchObjects();
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
			if (!patrolling)
			{
				Debug.Log("Starting to patrol");
				StartCoroutine("Patrol");
			}
		}
    }

	private void StartPursuit(Transform target)
	{
		StopPatrolCoroutine();
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

	private bool IsCloseEnoughToTarget(Vector3 target)
	{
		return Vector3.Distance(transform.position, target) < agent.stoppingDistance;
	}

	private void StopPatrolCoroutine()
	{
		agent.speed = patrollingSpeed;
		StopCoroutine("Patrol");
		patrolling = false;
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

	private IEnumerator Patrol()
	{
		agent.speed = patrollingSpeed;
		patrolling = true;
		if (patrolPoints.Length == 0) yield break;
		yield return new WaitUntil(() => IsCloseEnoughToDestination);
		yield return new WaitForSeconds(patrollingWaitTime);
		while (true)
		{
			agent.SetDestination(patrolPoints[currentPatrolPoint].position);
			yield return new WaitUntil(() => IsCloseEnoughToDestination);
			yield return new WaitForSeconds(patrollingWaitTime);
			currentPatrolPoint = currentPatrolPoint == patrolPoints.Length - 1 ? 0 : currentPatrolPoint + 1;
		}
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
