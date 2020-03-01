using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour, IDamageable
{

    public int health = 100;

    public Stats Stats => new Stats();

    public void TakeDamage(int ammount)
    {
        health -= ammount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
