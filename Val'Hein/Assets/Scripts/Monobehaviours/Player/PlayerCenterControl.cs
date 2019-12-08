using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerClass))]
public class PlayerCenterControl : Singleton<PlayerCenterControl>
{
	[Header("General")]
	public CameraBehaviour playerCamera;
	public ArmorStatsIncreaser playerArmor;
	public Stats playerStats;
	public LevelDefinitions playerLevel;
	[HideInInspector]
	public PlayerController playerController;
	[HideInInspector]
	public PlayerCombat playerCombat;
	[HideInInspector]
	public PlayerClass playerClass;

	private void Start()
	{
		playerController = GetComponent<PlayerController>();
		playerCombat = GetComponent<PlayerCombat>();
		playerClass = GetComponent<PlayerClass>();
	}

}
