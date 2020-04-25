using UnityEngine;
using UnityEngine.Profiling;

#pragma warning disable CS0649

[RequireComponent(typeof(PlayerControllerSystem), typeof(PlayerCombatSystem), typeof(PlayerUISystem))]
[RequireComponent(typeof(PlayerInputSystem), typeof(PlayerInventorySystem), typeof(Animator))]
public class PlayerCenterControl : Singleton<PlayerCenterControl>
{

	#region Camera Reference

	[Header("Camera Reference")]

	[Tooltip("The camera that this player will use.")]
	[SerializeField]
	private CameraBehaviour m_camera;
	public new CameraBehaviour camera => m_camera;

	#endregion

	#region Required Components Instances

	public PlayerControllerSystem controller { get; private set; }
	public PlayerCombatSystem combat { get; private set; }
	public PlayerUISystem ui { get; private set; }
	public PlayerInputSystem input { get; private set; }
	public PlayerInventorySystem inventory { get; private set; }
	public Animator anim { get; private set; }

	#endregion

	protected override void Awake()
	{
		base.Awake();
		controller = GetComponent<PlayerControllerSystem>();
		combat = GetComponent<PlayerCombatSystem>();
		input = GetComponent<PlayerInputSystem>();
		inventory = GetComponent<PlayerInventorySystem>();
		ui = GetComponent<PlayerUISystem>();
		anim = GetComponent<Animator>();
	}

}
