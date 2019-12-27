using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCArcher : NPC
{
	#region Archer Combat Settings
	[Header("Archer Combat Settings")]
	[Tooltip("The minimum distance between the target and the NPC to retreat.")]
	public float distanceToRetreat = 5f;
	[Tooltip("The attack range of the NPC.")]
	public float attackRange = 15f;
	#endregion

}
