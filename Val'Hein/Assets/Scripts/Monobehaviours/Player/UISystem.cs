using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISystem : MonoBehaviour
{
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private GameObject inventoryDisplay;

    private float currentHealth;

    private Coroutine changeHealthCoroutine;

    private bool isInventoryOpened = false;

    private InputSystem input { get { return PlayerCenterControl.Instance.input; } }
    private InventorySystem inventory { get { return PlayerCenterControl.Instance.inventory; } }

    // Start is called before the first frame update
    void Start()
    {
        inventory.onItemsChangeCallback.AddListener(UpdateInventoryUI);
        currentHealth = PlayerCenterControl.Instance.combat.CurrentHealth;
        PlayerCenterControl.Instance.combat.onHealthChange.AddListener(UpdateHealthBar);
        healthBar.value = healthBar.maxValue = currentHealth;
        inventoryDisplay.SetActive(false);
        UpdateInventoryUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (isInventoryOpened)
        {
            
        }
    }

    public void ToggleInventory()
    {
        inventoryDisplay.SetActive(!isInventoryOpened);
        isInventoryOpened = !isInventoryOpened;
        if (isInventoryOpened)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
            GameManager.Instance.ResumeGame();
        }
    }

    public void UpdateHealthBar(float newHealth)
    {
        if (changeHealthCoroutine != null)
            StopCoroutine(changeHealthCoroutine);
        changeHealthCoroutine = StartCoroutine(ChangeHealth(newHealth));
    }

    public void UpdateInventoryUI()
    {
        var slots = inventoryDisplay.GetComponentsInChildren<ItemSlot>();
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.storedItems.Count)
            {
                slots[i].SetSlot(inventory.storedItems[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    private IEnumerator ChangeHealth(float newHealth)
    {
        while (true)
        {
            if (Mathf.Abs(currentHealth - newHealth) < 0.1f)
                currentHealth = newHealth;
            else
                currentHealth = Mathf.Lerp(currentHealth, newHealth, 5f * Time.unscaledDeltaTime);
            healthBar.value = currentHealth;
            yield return null;
        }
    }


}
