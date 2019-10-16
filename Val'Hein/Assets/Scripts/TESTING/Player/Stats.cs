using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stats", menuName = "Entities/Stats")]
public class Stats : ScriptableObject
{
	[Header("Vitals")]
	public float health;
	public float resistance;

	[Header("Physical damage")]
	public float strength;
	[Range(0, 100)]
	public float precision;

	[Header("Magic Damage")]
	public float power;
	public float intelligence;
	public float runicKnowledge;

	[Header("Physical speed")]
	public float agility;
}
