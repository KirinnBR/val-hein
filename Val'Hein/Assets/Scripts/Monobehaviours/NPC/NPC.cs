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
	protected List<Transform> visibleObjects = new List<Transform>();

	#endregion
	[Space]

	#region Combat Settings
	[Header("Combat Settings")]

	[Tooltip("Stats of the NPC.")]
	[SerializeField]
	protected Stats stats;
	[SerializeField]
	protected List<Attack> attacks;
	[SerializeField]
	[Rename("Has Weapon?")]
	protected bool hasWeapon = false;
	[SerializeField]
	[ConditionalHide("hasWeapon", true)]
	protected Weapon weapon;
	[SerializeField]
	[ConditionalHide("hasWeapon", false)]
	protected CombatSettings combatSettings;
	[Tooltip("Speed when in battle.")]
	[SerializeField]
	protected float battleSpeed = 10f;

	protected int CurrentAttackIndex { get; set; } = 0;
	protected List<HitMarker> hitMarkers { get { return combatSettings.hitMarkers; } }
	protected Attack CurrentAttack => attacks[CurrentAttackIndex];
	protected bool IsAttacking { get; set; } = false;
	protected bool animationFinished = false;
	protected Coroutine activeMarkersCoroutine = null;

	#endregion
	[Space]
	#region Agent Settings

	[Header("Agent Settings")]

	[SerializeField]
	protected float agentAcceleration = 80f;
	[SerializeField]
	protected float agentAngularSpeed = 1200f;
	[SerializeField]
	protected float agentStoppingDistance = 2f;

	protected NavMeshAgent agent;
	protected bool IsCloseEnoughToDestination => IsCloseEnoughToTarget(agent.destination);
	protected bool IsCloseEnoughToTarget (Vector3 target) { return Vector3.Distance(transform.position, target) < agent.stoppingDistance; }

	#endregion

	public float CurrentHealth { get; protected set; }
	protected Animator anim;

	protected virtual void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
		CurrentHealth = stats.baseHealth;
		if (hasWeapon)
			weapon.MergeStatsWithUser(stats);
		else
			combatSettings.hitMarkerManager.ConfigureMarkers(hitMarkers.ToArray());
		agent.acceleration = agentAcceleration;
		agent.angularSpeed = agentAngularSpeed;
		agent.stoppingDistance = agentStoppingDistance;
	}

	protected virtual void FixedUpdate()
	{
		SearchObjects();
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
			foreach (var obj in objectsInShortVisionRadius)
			{
				if (!visibleObjects.Contains(obj.transform))
				{
					visibleObjects.Add(obj.transform);
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

	protected void ProccessAttackAnimation()
	{
		animationFinished = false;

		CurrentAttackIndex = Random.Range(0, attacks.Count - 1);

		anim.SetInteger("Attack Index", CurrentAttackIndex);
		anim.SetTrigger("Attack");

		IsAttacking = true;
	}

	#region Coroutines

	protected IEnumerator CheckCollisions()
	{
		List<IDamageable> cannotHit = new List<IDamageable>();
		while (true)
		{
			foreach (var marker in hitMarkers)
			{
				if (marker.TryGetDamageable(out IDamageable dmg) && !cannotHit.Contains(dmg))
				{
					DoDamage(dmg);
					cannotHit.Add(dmg);
				}
			}
			yield return null;
		}
	}

	protected IEnumerator CheckCollisionsContinuous()
	{
		List<IDamageable> cannotHit = new List<IDamageable>();
		while (true)
		{
			foreach (var marker in hitMarkers)
			{
				if (marker.TryGetDamageable(out IDamageable dmg) && !cannotHit.Contains(dmg))
				{
					DoDamage(dmg);
					cannotHit.Add(dmg);
				}
			}
			yield return new WaitForSeconds(combatSettings.continuousDamageInterval);
			cannotHit.Clear();
			yield return null;
		}
	}

	protected void DoDamage(IDamageable dmg) => dmg.TakeDamage(stats.baseStrength * CurrentAttack.damageMultiplier);

	#endregion

	#region Animation Event Methods

	public void EnableMarkers()
	{
		if (hasWeapon)
			weapon.ActivateMarkers(CurrentAttack.damageMultiplier);
		else
		{
			if (combatSettings.continuousDamage)
				activeMarkersCoroutine = StartCoroutine(CheckCollisionsContinuous());
			else
				activeMarkersCoroutine = StartCoroutine(CheckCollisions());
		}
	}

	public void DisableMarkers(DeactivationType finish)
	{
		if (hasWeapon)
			weapon.DeactivateMarkers();
		else
			StopCoroutine(activeMarkersCoroutine);

		if (finish == DeactivationType.FinishAnimation)
			FinishAnimation();
	}

	public void FinishAnimation()
	{
		if (animationFinished) return;

		IsAttacking = false;

		animationFinished = true;
	}

	#endregion

	#region IDamageable Methods

	public virtual void TakeDamage(float ammount)
	{
		CurrentHealth = CurrentHealth - (ammount - stats.baseResistance);
		if (CurrentHealth <= 0)
		{
			Die();
		}
	}

	protected virtual void Die()
	{
		CurrentHealth = 0;
		Debug.Log($"{gameObject.name} has died.");
		Destroy(gameObject);
	}

	#endregion

}