using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCArcher : NPC
{
	[Header("Agent Settings")]
	[Tooltip("Speed when in battle.")]
	public float battleSpeed = 15f;
	public bool IsCloseEnoughToDestination => Vector3.Distance(agent.destination, transform.position) < agent.stoppingDistance;
	private NavMeshAgent agent;
	[Space]
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
	[SerializeField]
	[Tooltip("The minimum distance between the target and the NPC to retreat.")]
	private float distanceToRetreat = 5f;
	[SerializeField]
	[Tooltip("The attack range of the NPC.")]
	private float attackRange = 15f;

	public float CurrentHealth { get; private set; }
	private bool attackOnCooldown = false;
	private float currentCooldown = 0;
	private Vector3 inicialPosition;
	private Quaternion inicialRotation;

    // Start is called before the first frame update
    protected override void Start()
    {
		base.Start();
		agent = GetComponent<NavMeshAgent>();
		CurrentHealth = healthPoints;
		agent.stoppingDistance = attackRange - 1;
		inicialPosition = transform.position;
		inicialRotation = transform.rotation;
	}

	// Update is called once per frame
	void Update()
    {
		UpdateCooldown();
		SearchObjects();
		if (visibleObjects.Count > 0)
		{
			foreach (var visibleObject in visibleObjects)
			{
				if (visibleObject.tag == "Player")
				{
					StartPursuit(visibleObject);
				}
			}
		}
		else
		{
			agent.destination = inicialPosition;
			transform.rotation = inicialRotation;
		}
	}

	private bool IsCloseEnoughToTarget(Vector3 target)
	{
		return Vector3.Distance(transform.position, target) < agent.stoppingDistance;
	}

	private void StartPursuit(Transform target)
	{
		agent.speed = battleSpeed;
		transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
		if (Vector3.Distance(transform.position, target.position) <= distanceToRetreat)
		{
			agent.stoppingDistance = 0;
			agent.destination = -transform.forward.normalized * distanceToRetreat;
		}
		else
		{
			agent.stoppingDistance = attackRange - 2;
			agent.SetDestination(target.position);
		}
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
		if (target.TryGetComponent(out IDamageable dmg))
		{
			dmg.TakeDamage(damagePoints);
			Debug.Log("Attack");
		}
		else
		{
			Debug.Log("Couldn't attack");
		}
		attackOnCooldown = true;
		currentCooldown = attackDelay;
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

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, distanceToRetreat);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, attackRange - 1);
	}

}
