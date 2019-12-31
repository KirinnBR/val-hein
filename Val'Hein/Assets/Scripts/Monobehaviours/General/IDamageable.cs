using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
	void TakeDamage(float ammount);
	float CurrentHealth { get; }
}
