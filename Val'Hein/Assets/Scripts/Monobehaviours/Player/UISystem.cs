using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISystem : MonoBehaviour
{


    private float currentHealth;

    private Coroutine changeHealthCoroutine;

    private GameObject canvas { get { return PlayerCenterControl.Instance.Canvas; } }
    private Slider healthBar { get { return PlayerCenterControl.Instance.HealthBar; } }

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = PlayerCenterControl.Instance.combat.CurrentHealth;
        healthBar.value = healthBar.maxValue = currentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHealthBar(float newHealth)
    {
        if (changeHealthCoroutine != null)
            StopCoroutine(changeHealthCoroutine);
        changeHealthCoroutine = StartCoroutine(ChangeHealth(newHealth));
    }

    private IEnumerator ChangeHealth(float newHealth)
    {
        while (true)
        {
            if (currentHealth > newHealth)
            {
                if (currentHealth <= newHealth + 0.1f)
                {
                    currentHealth = newHealth;
                    break;
                }
            }
            else
            {
                if (currentHealth >= newHealth - 0.1f)
                {
                    currentHealth = newHealth;
                    break;
                }
            }
            currentHealth = Mathf.Lerp(currentHealth, newHealth, 5f * Time.deltaTime);
            yield return null;
        }
        changeHealthCoroutine = null;
        yield break;
    }


}
