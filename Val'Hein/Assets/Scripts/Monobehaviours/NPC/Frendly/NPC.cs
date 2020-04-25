using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Valhein.StateMachine;

#pragma warning disable CS0649
[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(FiniteStateMachine))]
public class NPC : MonoBehaviour
{
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

	protected virtual void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
		canvas = canvasObject.GetComponent<Canvas>();
		nameOnCanvas = canvas.GetComponentInChildren<Text>();
	}

	protected virtual void Start()
	{
		nameOnCanvas.text = npcName;
		if (referenceCamera == null)
		{
			Debug.LogWarning("[NPC] Reference camera not set. Using MainCamera instead.");
			m_referenceCamera = Camera.main;
		}
		canvas.GetComponent<Canvas>().worldCamera = referenceCamera;
	}

	protected virtual void Update()
	{
		canvasObject.transform.LookAt(referenceCamera.transform, Vector3.up);
	}

	public Vector3 DirFromAngle(float angle)
	{
		angle += transform.eulerAngles.y;
		return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
	}

	public bool IsCloseEnoughToPoint(Vector3 point)
	{
		return Vector3.Distance(transform.position, point) < agent.stoppingDistance;
	}

}