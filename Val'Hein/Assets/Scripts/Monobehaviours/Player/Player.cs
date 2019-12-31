using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerClass))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Player : Singleton<Player>
{
	[Header("General")]
	public CameraBehaviour playerCamera;
	public ArmorStatsIncreaser playerArmor;
	public Stats playerStats;
	public LevelDefinitions playerLevel;
	public PlayerController playerController { get; private set; }
	public PlayerCombat playerCombat { get; private set; }
	public PlayerClass playerClass { get; private set; }
    public Animator anim { get; private set; }

    private void Start()
	{
		playerController = GetComponent<PlayerController>();
		playerCombat = GetComponent<PlayerCombat>();
		playerClass = GetComponent<PlayerClass>();
		anim = GetComponent<Animator>();
	}

}
