using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDamageable : MonoBehaviour, IDamageable
{
	public void TakeDamage(float ammount)
	{
		Debug.Log($"{gameObject.name} took {ammount} damage points.");
	}
}
