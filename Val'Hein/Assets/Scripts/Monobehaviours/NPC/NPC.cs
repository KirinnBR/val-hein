using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#pragma warning disable CS0649
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NPC : MonoBehaviour, IDamageable
{
	#region Object Detection Settings
	[Header("Object Detection Settings")]
	[Tooltip("The angle, in degrees, of the vision.")]
	[Range(0, 360)]
	public float visionAngle = 45f;
	[Tooltip("The distance, in meters, of the vision.")]
	public float visionRadius = 10f;
	[Tooltip("The distance, in meters, of the short-distance vision.")]
	public float shortDistanceVisionRadius = 3f;
	[Tooltip("The layer in which the objects to detect are.")]
	[SerializeField]
	private LayerMask detectionLayer;
	[Tooltip("The layer in which the obstacles are.")]
	[SerializeField]
	private LayerMask obstacleObjectsLayer;
	protected List<Transform> visibleObjects;
	#endregion
	[Space]
	#region Combat Settings
	[Header("Combat Settings")]
	[Tooltip("Stats of the NPC.")]
	[SerializeField]
	protected Stats stats;
	[Tooltip("Speed when in battle.")]
	[SerializeField]
	protected float battleSpeed = 10f;
	#endregion
	[Space]
	#region Agent Settings
	protected NavMeshAgent agent;
	public bool IsCloseEnoughToDestination => Vector3.Distance(agent.destination, transform.position) < agent.stoppingDistance;
	protected bool IsCloseEnoughToTarget (Vector3 target) { return Vector3.Distance(transform.position, target) < agent.stoppingDistance; }
	#endregion

	protected Animator anim;
	protected float CurrentHealth { get; set; }
	protected bool attackOnCooldown = false;
	protected float currentCooldown = 0;

	protected virtual void Start()
	{
		visibleObjects = new List<Transform>();
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
		CurrentHealth = stats.baseHealth;
		agent.acceleration = 80;
		agent.angularSpeed = 1200;
		agent.stoppingDistance = 2f;
	}

	protected virtual void Update()
	{
		UpdateCooldown();
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

	protected void SearchObjects()
	{
		visibleObjects.Clear();
		var objectsInVisionRadius = Physics.OverlapSphere(transform.position, visionRadius, detectionLayer);
		if (objectsInVisionRadius.Length > 0)
		{
			for (int i = 0; i < objectsInVisionRadius.Length; i++)
			{
				Vector3 dirToTarget = (objectsInVisionRadius[i].transform.position - transform.position).normalized;
				if (Vector3.Angle(transform.forward, dirToTarget) < visionAngle / 2f)
				{
					if (!Physics.Linecast(transform.position, objectsInVisionRadius[i].transform.position, obstacleObjectsLayer))
					{

						visibleObjects.Add(objectsInVisionRadius[i].transform);
					}
				}
			}
		}
		var objectsInShortVisionRadius = Physics.OverlapSphere(transform.position, shortDistanceVisionRadius, detectionLayer);
		if (objectsInShortVisionRadius.Length > 0)
		{
			foreach (var _object in objectsInShortVisionRadius)
			{
				if (!visibleObjects.Contains(_object.transform))
				{
					visibleObjects.Add(_object.transform);
				}
			}
		}
	}

	public Vector3 DirFromAngle( float angle, bool isGlobal )
	{
		if (!isGlobal)
			angle += transform.eulerAngles.y;
		return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
	}

	protected void Attack(Transform target)
	{
		//Animation of attack.
		//Sound of attack.
		if (target.TryGetComponent(out IDamageable dmg))
		{
			if (Random.Range(0, 100) <= stats.basePrecision)
			{
				Debug.Log($"{gameObject.name} dealt damage to {target.name}.");
				dmg.TakeDamage(stats.baseStrength);
			}
			else
			{
				Debug.Log($"{gameObject.name} missed the attack.");
			}
		}
		else
		{
			Debug.Log($"{target.name} is not damageable.");
		}
		attackOnCooldown = true;
		//TODO: Agility cooldown.
		currentCooldown = 2;
	}

	public void TakeDamage(float ammount)
	{
		CurrentHealth = CurrentHealth - (ammount - stats.baseResistance);
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

	protected virtual void FixedUpdate()
	{
		SearchObjects();
	}

}
