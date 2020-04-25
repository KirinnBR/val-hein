using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class HostileNPC : CombatObject
{
	#region Combat Settings

	[Tooltip("Defending, normal and attacking move speed, respectively.")]
	[SerializeField]
	private Vector3 m_speed = new Vector3(2f, 5f, 10f);
	public Vector3 speed => m_speed;

	[SerializeField]
	private NPCAttack[] m_attacks;
	public NPCAttack[] attacks => m_attacks;

	[SerializeField]
	[Rename("Randomize Attacks Per Combo?")]
	private bool m_randomizeAttacksPerCombo = false;
	public bool randomizeAttacksPerCombo => m_randomizeAttacksPerCombo;

	[SerializeField]
	private int m_attacksPerCombo = 2;
	public int attacksPerCombo => m_attacksPerCombo;

	private Coroutine resetHitNumberCoroutine = null;

	public int currentAttackIndex { get; set; } = 0;
	public int currentAttacksLimit { get; private set; }
	public int currentAttackCombo { get; set; } = 0;
	public bool canAttack { get; set; } = true;
	public int hitNumber { get; set; } = 0;
	public bool animationReady { get; set; } = true;
	public bool onGuard { get; set; }

	public int currentAttackFrame = 1;
	public int currentHitMarkerIndex = 0;

	#endregion

	#region NPC Settings

	[Header("NPC Settings")]

	[SerializeField]
	[Rename("NPC Name")]
	private string m_npcName = "NPC";
	public string npcName => m_npcName;

	public NavMeshAgent agent { get; private set; }
	public Animator anim { get; private set; }

	#endregion

	#region UI Settings

	[Header("UI Settings")]

	[SerializeField]
	[Rename("NPC Canvas")]
	private GameObject m_canvasObject;
	public GameObject canvasObject => m_canvasObject;

	[SerializeField]
	private Camera m_referenceCamera;
	public Camera referenceCamera => m_referenceCamera;

	public Canvas canvas { get; private set; }
	private Text nameOnCanvas;

	#endregion

	#region Object Detection Settings

	[Header("Object Detection Settings")]

	[Tooltip("The distance, in meters, of the periferic vision.")]
	[SerializeField]
	private float m_perifericVisionRadius = 3f;
	public float perifericVisionRadius
	{
		get
		{
			return m_perifericVisionRadius;
		}
		set
		{
			m_perifericVisionRadius = value;
		}
	}

	[Tooltip("The distance, in meters, of the normal vision.")]
	[SerializeField]
	private float m_normalVisionRadius = 10f;
	public float normalVisionRadius
	{
		get
		{
			return m_normalVisionRadius;
		}
		set
		{
			m_normalVisionRadius = value;
		}
	}

	[Tooltip("The angle, in degrees, of the vision.")]
	[SerializeField]
	[Range(0, 360)]
	private float m_normalVisionAngle = 90f;
	public float normalVisionAngle
	{
		get
		{
			return m_normalVisionAngle;
		}
		set
		{
			m_normalVisionAngle = value;
		}
	}

	[Tooltip("The distance, in meters, of the vision when the target is defined.")]
	[SerializeField]
	private float m_wideDistanceVisionRadius = 20f;
	public float wideDistanceVisionRadius
	{
		get
		{
			return m_wideDistanceVisionRadius;
		}
		set
		{
			m_wideDistanceVisionRadius = value;
		}
	}

	[Tooltip("The layer in which the obstacles are.")]
	[SerializeField]
	private LayerMask m_obstacleObjectsLayer;
	public LayerMask obstacleObjectsLayer => m_obstacleObjectsLayer;

	public List<Transform> visibleTargets { get; } = new List<Transform>();
	public Transform target { get; set; }
	public bool hasTargets => visibleTargets.Count > 0;

	#endregion

	#region Item Drop Settings

	[Header("Item Drop Settings")]

	[SerializeField]
	private float m_dropRange = 2f;
	public float dropRange => m_dropRange;

	[SerializeField]
	private ItemData[] m_itemsToDrop;
	public ItemData[] itemsToDrop => m_itemsToDrop;

	#endregion

	private Slider healthBar;

	private void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();

		canvas = canvasObject.GetComponent<Canvas>();

		healthBar = canvas.GetComponentInChildren<Slider>();
		nameOnCanvas = canvas.GetComponentInChildren<Text>();

		SetAttacksPerCombo();
	}

	protected override void Start()
	{
		base.Start();

		nameOnCanvas.text = npcName;

		if (referenceCamera == null)
		{
			Debug.LogWarning("[NPC] Reference camera not set. Using MainCamera instead.");
			m_referenceCamera = Camera.main;
		}
		canvas.worldCamera = referenceCamera;

		currentStats = stats;

		healthBar.maxValue = currentStats.health;

		InvokeRepeating("SearchObjects", 1f, 0.1f);
	}

	protected virtual void Update()
	{
		canvasObject.transform.LookAt(referenceCamera.transform, Vector3.up);
		if (healthBar.value != currentStats.health)
			healthBar.value = Mathf.Lerp(healthBar.value, currentStats.health, 5f * Time.deltaTime);
	}

	private void SearchObjects()
	{
		visibleTargets.Clear();

		var objectsInShortVisionRadius = Physics.OverlapSphere(transform.position, perifericVisionRadius, combatLayer);
		if (objectsInShortVisionRadius.Length > 0)
		{
			for (int i = 0; i < objectsInShortVisionRadius.Length; i++)
			{
				Transform objTransform = objectsInShortVisionRadius[i].transform;
				if (!visibleTargets.Contains(objTransform))
				{
					visibleTargets.Add(objTransform);
				}
			}
		}

		var objectsInVisionRadius = Physics.OverlapSphere(transform.position, normalVisionRadius, combatLayer);
		if (objectsInVisionRadius.Length > 0)
		{
			for (int i = 0; i < objectsInVisionRadius.Length; i++)
			{
				Transform objTransform = objectsInVisionRadius[i].transform;
				if (Vector3.Angle(transform.forward, (objTransform.position - transform.position).normalized) < normalVisionAngle / 2f)
				{
					if (!Physics.Linecast(transform.position, objTransform.position, obstacleObjectsLayer))
					{
						visibleTargets.Add(objTransform);
					}
				}
			}
		}
	}

	public void SetAttacksPerCombo()
	{
		if (randomizeAttacksPerCombo)
		{
			currentAttacksLimit = Random.Range(1, attacks.Length);
		}
		else
		{
			currentAttacksLimit = attacksPerCombo;
		}
		Debug.Log("Current Attack Limit: " + currentAttacksLimit);
	}

	#region IDamageable Methods

	public override void TakeDamage(int ammount, DamageType type)
	{
		base.TakeDamage(ammount, type);
		currentStats.health -= ammount - stats.resistance;
		if (currentStats.health <= 0)
			Die();
		else
		{
			//anim.SetTrigger("Hurt");
			//FinishAnimation();
		}

		if (type == DamageType.MeleeAttack)
		{
			hitNumber++;
			if (resetHitNumberCoroutine != null)
			{
				StopCoroutine(resetHitNumberCoroutine);
			}
			resetHitNumberCoroutine = StartCoroutine(ResetHitNumber());
		}
	}

	protected override void Die()
	{
		StopAllCoroutines();
		foreach (var item in itemsToDrop)
		{
			Vector3 randomDir = Random.insideUnitSphere;
			Vector3 spawnPos = new Vector3
			{
				x = transform.position.x + randomDir.x * dropRange,
				y = transform.position.y,
				z = transform.position.z + randomDir.z * dropRange
			};
			item.Spawn(spawnPos);
		}
		currentStats.health = 0;
		GetComponent<Collider>().enabled = false;

		//FinishAnimation();
		//anim.SetTrigger("Die");
	}

	private void ResetAttack()
	{
		canAttack = true;
	}

	public void Attack()
	{
		animationReady = false;

		//anim.SetInteger("Attack Index", currentAttackIndex);
		//anim.SetTrigger("Attack");
		Debug.Log("Animation started");

		//controller.MovementBlocked = true;
		//controller.RotationBlocked = true;
		//if (currentAttacksLimit == currentAttackCombo)
		//{
		//	currentAttackCombo = 0;
		//	SetAttacksPerCombo();
		//	Invoke("ResetAttack", 5f);
		//}
		//else
		//{
		//	Invoke("ResetAttack", attacks[currentAttackIndex].timeToRest);
		//}
	}

	public void FinishAnimation()
	{
		currentAttackCombo++;

		animationReady = true;

		Debug.Log("Animation Finished");

		//if (currentAttackIndex == m_attacks.Count - 1)
		//	currentAttackIndex = 0;
		//else
		//{
		//	computeComboCoroutine = StartCoroutine(ComputeCombo());
		//	currentAttackIndex++;
		//}

		currentAttackFrame = 1;
		currentHitMarkerIndex = 0;
	}

	public void UpdateAnimation()
	{
		if (currentHitMarkerIndex < attacks[currentAttackIndex].hitMarkersTime.Length)
		{
			var hitMarkerTime = attacks[currentAttackIndex].hitMarkersTime[currentHitMarkerIndex];

			if (!hitMarkersActive)
			{
				if (hitMarkerTime != null)
				{
					if (currentAttackFrame == hitMarkerTime.x)
					{
						ActivateMarkers();
					}
				}
			}
			else
			{
				if (currentAttackFrame == hitMarkerTime.y)
				{
					DeactivateMarkers();
					currentHitMarkerIndex++;
				}
			}
		}
		currentAttackFrame++;
	}

	private IEnumerator ResetHitNumber()
	{
		yield return new WaitForSeconds(2f);
		hitNumber = 0;
	}

	#endregion

#if UNITY_EDITOR
	private float lastPeriferic;
	private float lastNormal;
	private float lastWide;
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
		if (m_attacksPerCombo > m_attacks.Length)
			m_attacksPerCombo = m_attacks.Length;
	}
#endif

	public Vector3 DirFromAngle(float angle)
	{
		angle += transform.eulerAngles.y;
		return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
	}

	public bool IsCloseEnoughToPoint(Vector3 point)
	{
		return Vector3.Distance(transform.position, point) < agent.stoppingDistance;
	}

	protected override void DoDamage(IDamageable dmg)
	{
		dmg.TakeDamage(10, DamageType.MeleeAttack);
	}

}
