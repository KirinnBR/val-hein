using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

[RequireComponent(typeof(ControllerSystem), typeof(CombatSystem), typeof(UISystem))]
[RequireComponent(typeof(InputSystem), typeof(InventorySystem), typeof(Animator))]
public class PlayerCenterControl : Singleton<PlayerCenterControl>
{
	#region Scriptable Objects References

	[Header("Scriptable Objects References")]

	[Tooltip("The armor settings the player is using.")]
	[SerializeField]
	private ArmorSet armor;
	public ArmorSet Armor { get { return armor; } }

	[Tooltip("The player's stats.")]
	[SerializeField]
	private Stats stats;
	public Stats Stats { get { return stats; } }

	[Tooltip("The level data for player.")]
	[SerializeField]
	private LevelDefinitions level;
	public LevelDefinitions Level { get { return level; } }

	#endregion

	#region Layer References

	[Header("Layer References")]

	[Tooltip("The LayerMask that the player's physics calculations will use.")]
	[SerializeField]
	private LayerMask physicsCheckLayer;
	public LayerMask PhysicsCheckLayer { get { return physicsCheckLayer; } }

	[Tooltip("The LayerMask that the player's combat physics calculations will use.")]
	[SerializeField]
	private LayerMask combatCheckLayer;
	public LayerMask CombatCheckLayer { get { return combatCheckLayer; } }

	#endregion

	#region Camera Reference

	[Header("Camera Reference")]

	[Tooltip("The camera that this player will use.")]
	[SerializeField]
	private new CameraBehaviour camera;
	public CameraBehaviour Camera { get { return camera; } }

	#endregion

	#region Canvas References

	[Header("Canvas References")]

	[Tooltip("The Canvas of the player. (It should be inside the player GameObject.)")]
	[SerializeField]
	private GameObject canvas;
	public GameObject Canvas { get { return canvas; } }

	[Tooltip("The player's health bar.")]
	[SerializeField]
	private Slider healthBar;
	public Slider HealthBar { get { return healthBar; } }

	#endregion

	#region Required Components Instances

	public ControllerSystem controller { get; private set; }
	public CombatSystem combat { get; private set; }
	public InputSystem input { get; private set; }
	public InventorySystem inventory { get; private set; }
	public UISystem ui { get; private set; }
	public Animator anim { get; private set; }

	#endregion

	protected override void Awake()
	{
		base.Awake();
		controller = GetComponent<ControllerSystem>();
		combat = GetComponent<CombatSystem>();
		input = GetComponent<InputSystem>();
		inventory = GetComponent<InventorySystem>();
		ui = GetComponent<UISystem>();
		anim = GetComponent<Animator>();
		//playerClass = GetComponent<PlayerClass>();
	}
}
