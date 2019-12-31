using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

#pragma warning disable CS0649
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NPC : MonoBehaviour, IDamageable
{
	#region NPC Settings

	[Header("NPC Settings")]

	[SerializeField]
	[Rename("NPC Name")]
	protected string npcName = "NPC";

	#endregion

	#region UI Settings

	[Header("UI Settings")]

	[SerializeField]
	[Rename("NPC Canvas")]
	private GameObject canvas;
	[SerializeField]
	private Camera referenceCamera;
	protected Canvas npcCanvas;
	protected Slider npcHealthBar;
	protected Text npcTextName;

	#endregion

	#region Object Detection Settings

	[Header("Object Detection Settings")]

	[Tooltip("The distance, in meters, of the periferic vision.")]
	public float perifericVisionRadius = 3f;
	[Tooltip("The distance, in meters, of the normal vision.")]
	public float normalVisionRadius = 10f;
	[Tooltip("The angle, in degrees, of the vision.")]
	[Range(0, 360)]
	public float normalVisionAngle = 90f;
	[Tooltip("The distance, in meters, of the vision when the target is defined.")]
	public float wideDistanceVisionRadius = 20f;
	[Tooltip("The layer in which the objects to detect are.")]
	[SerializeField]
	private LayerMask detectionLayer;
	[Tooltip("The layer in which the obstacles are.")]
	[SerializeField]
	private LayerMask obstacleObjectsLayer;

	protected List<Transform> visibleObjects = new List<Transform>();

	#endregion

	#region Combat Settings

	[Header("Combat Settings")]

	[Tooltip("Stats of the NPC.")]
	[SerializeField]
	protected Stats stats;
	[SerializeField]
	protected List<Attack> attacks;
	[SerializeField]
	[Rename("Randomize Attacks Per Combo?")]
	protected bool randomizeAttacksPerCombo = false;
	[SerializeField]
	[ConditionalHide("randomizeAttacksPerCombo", false)]
	protected int attacksPerCombo = 2;
	[SerializeField]
	[Rename("NPC Type")]
	protected NPCType npcType = NPCType.Human;
	[SerializeField]
	[Rename("Has Weapon?")]
	protected bool hasWeapon = false;
	[SerializeField]
	[ConditionalHide("hasWeapon", true)]
	protected Weapon weapon;
	[SerializeField]
	[ConditionalHide("hasWeapon", false)]
	protected CombatSettings combatSettings;
	[Tooltip("Defending, normal and attacking move speed, respectively.")]
	[SerializeField]
	protected Vector3 speed = new Vector3(2f, 5f, 10f);

	public float CurrentHealth { get; private set; }
	protected float CurrentStamina { get; set; }
	protected int CurrentAttackIndex { get; set; } = 0;
	protected int CurrentAttackCombo { get; set; } = 0;
	protected List<HitMarker> HitMarkers { get { return combatSettings.hitMarkers; } }
	protected Attack CurrentAttack => attacks[CurrentAttackIndex];
	protected bool IsAttacking { get; set; } = false;
	protected bool IsDefending { get; set; } = false;
	protected bool canAttack = true;
	protected bool animationFinished = true;
	protected Coroutine activeMarkersCoroutine = null;
	protected Coroutine setAttackCooldownCoroutine = null;
	public bool ShowCombatGUI { get; set; } = false;
	protected bool onCombat = false;

	#endregion

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

	protected Animator anim;
#if UNITY_EDITOR
	/*Editor utility.*/
	public Vector3 StartPos { get; set; }
#endif
	protected virtual void Start()
	{
#if UNITY_EDITOR
		StartPos = transform.position;
#endif
		CurrentHealth = stats.baseHealth;
		CurrentStamina = stats.baseStamina;
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
		npcCanvas = canvas.GetComponent<Canvas>();
		npcHealthBar = npcCanvas.GetComponentInChildren<Slider>();
		npcTextName = npcCanvas.GetComponentInChildren<Text>();
		npcTextName.text = npcName;
		npcHealthBar.maxValue = CurrentHealth;
		npcHealthBar.value = CurrentHealth;

		if (randomizeAttacksPerCombo)
			attacksPerCombo = Random.Range(1, attacks.Count);

		if (hasWeapon)
			weapon.MergeStatsWithUser(stats);
		else
			combatSettings.hitMarkerManager.ConfigureMarkers(HitMarkers.ToArray());

		agent.acceleration = agentAcceleration;
		agent.angularSpeed = agentAngularSpeed;
		agent.stoppingDistance = agentStoppingDistance;
	}

	protected virtual void FixedUpdate()
	{
		SearchObjects();
		UpdateUIElements();
		if (referenceCamera != null)
			canvas.transform.LookAt(referenceCamera.transform, Vector3.up);
	}

	protected void UpdateUIElements()
	{
		npcHealthBar.value = Mathf.Lerp(npcHealthBar.value, CurrentHealth, 0.2f);
	}

	protected void SearchObjects()
	{
		visibleObjects.Clear();
		var objectsInVisionRadius = Physics.OverlapSphere(transform.position, normalVisionRadius, detectionLayer);
		if (objectsInVisionRadius.Length > 0)
		{
			for (int i = 0; i < objectsInVisionRadius.Length; i++)
			{
				Vector3 dirToTarget = (objectsInVisionRadius[i].transform.position - transform.position).normalized;
				if (Vector3.Angle(transform.forward, dirToTarget) < normalVisionAngle / 2f)
				{
					if (!Physics.Linecast(transform.position, objectsInVisionRadius[i].transform.position, obstacleObjectsLayer))
					{
						visibleObjects.Add(objectsInVisionRadius[i].transform);
					}
				}
			}
		}
		var objectsInShortVisionRadius = Physics.OverlapSphere(transform.position, perifericVisionRadius, detectionLayer);
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
		if (!animationFinished) { return; }

		animationFinished = false;

		CurrentAttackIndex = Random.Range(0, attacks.Count - 1);

		CurrentAttackCombo++;

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
			foreach (var marker in HitMarkers)
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
			foreach (var marker in HitMarkers)
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

	protected IEnumerator SetAttackCooldown()
	{
		yield return new WaitForSeconds(5f);
		canAttack = true;
	}

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
		if (animationFinished) { return; }

		IsAttacking = false;

		animationFinished = true;

		if (CurrentAttackCombo == attacksPerCombo)
		{
			canAttack = false;
			CurrentAttackCombo = 0;
			if (randomizeAttacksPerCombo)
				attacksPerCombo = Random.Range(1, attacks.Count);
		}

		if (setAttackCooldownCoroutine != null)
			StopCoroutine(setAttackCooldownCoroutine);
		setAttackCooldownCoroutine = StartCoroutine(SetAttackCooldown());
	}

	#endregion

	#region IDamageable Methods

	public virtual void TakeDamage(float ammount)
	{
		if (IsDefending)
			CurrentHealth -= ammount - stats.baseResistance * 2;
		else
			CurrentHealth -= ammount - stats.baseResistance;
		if (CurrentHealth <= 0)
		{
			Die();
		}
		else
		{
			anim.SetTrigger("Hurt");
			FinishAnimation();
		}
	}

	protected virtual void Die()
	{
		CurrentHealth = 0;
		anim.SetTrigger("Die");
		FinishAnimation();
		Debug.Log($"{gameObject.name} has died.");
		Destroy(gameObject);
	}

	#endregion

	//Auxiliary variables.
	float lastPeriferic { get; set; }
	float lastNormal { get; set; }float lastWide { get; set; }
	private void OnValidate()
	{
		if (perifericVisionRadius != lastPeriferic)
		{
			if (perifericVisionRadius <= 0f)
				perifericVisionRadius = 0f;
			else if (perifericVisionRadius >= normalVisionRadius)
				perifericVisionRadius = normalVisionRadius;
			lastPeriferic = perifericVisionRadius;
		}
		if (normalVisionRadius != lastNormal)
		{
			if (normalVisionRadius <= perifericVisionRadius)
				normalVisionRadius = perifericVisionRadius;
			else if (normalVisionRadius >= wideDistanceVisionRadius)
				normalVisionRadius = wideDistanceVisionRadius;
			lastNormal = normalVisionRadius;
		}
		if (wideDistanceVisionRadius != lastWide)
		{
			if (wideDistanceVisionRadius <= normalVisionRadius)
				wideDistanceVisionRadius = normalVisionRadius;
			lastWide = wideDistanceVisionRadius;
		}
		if (attacksPerCombo > attacks.Count)
			attacksPerCombo = attacks.Count;
	}

}

public enum NPCType
{
	Human, Beast, Dummy
}