using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour, IDamageable
{

    public float health = 100f;

    public void TakeDamage(float ammount)
    {
        health -= ammount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
