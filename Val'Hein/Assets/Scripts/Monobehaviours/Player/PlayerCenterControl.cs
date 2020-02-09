using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ControllerSystem), typeof(CombatSystem), typeof(UISystem))]
[RequireComponent(typeof(InputSystem), typeof(Animator))]
public class PlayerCenterControl : Singleton<PlayerCenterControl>
{
	[Header("References")]

	[Tooltip("The camera that the player will use.")]
	public CameraBehaviour playerCamera;
	[Tooltip("The armor settings the player is using.")]
	public ArmorStatsIncreaser playerArmor;
	[Tooltip("The player's stats.")]
	public Stats playerStats;
	[Tooltip("The level data for player.")]
	public LevelDefinitions playerLevel;
	[Tooltip("The LayerMask that the player's physics calculations will use.")]
	public LayerMask physicsCheckLayer;
	[Tooltip("The LayerMask that the player's combat physics calculations will use.")]
	public LayerMask combatCheckLayer;
	public ControllerSystem controller { get; private set; }
	public CombatSystem combat { get; private set; }
	public InputSystem input { get; private set; }
	//public PlayerClass playerClass { get; private set; }
	public UISystem ui { get; private set; }
	public Animator anim { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		controller = GetComponent<ControllerSystem>();
		combat = GetComponent<CombatSystem>();
		input = GetComponent<InputSystem>();
		//playerClass = GetComponent<PlayerClass>();
		ui = GetComponent<UISystem>();
		anim = GetComponent<Animator>();
	}
}
