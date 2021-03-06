﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUISystem : MonoBehaviour
{
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private GameObject inventoryDisplay;
    [SerializeField]
    private Text weightText;
    [SerializeField]
    private Text interactText;

    public Text InteractText => interactText;

    private Coroutine changeHealthCoroutine;

    private float currentHealth;
    private float maxHealth;

    private PlayerInputSystem input { get { return PlayerCenterControl.Instance.input; } }
    private PlayerInventorySystem inventory { get { return PlayerCenterControl.Instance.inventory; } }

    public bool IsUIActive { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        maxHealth = PlayerCenterControl.Instance.combat.stats.health;
        weightText.text = $"Weight: {inventory.weight}/{inventory.maxWeight}";
        healthBar.value = healthBar.maxValue = maxHealth;
        currentHealth = maxHealth;
        inventoryDisplay.SetActive(false);
        UpdateInventoryUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (input.OpenInventory)
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        var active = inventoryDisplay.activeSelf;
        inventoryDisplay.SetActive(!active);
        IsUIActive = active;
        if (!active)
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
        var itemSlots = inventoryDisplay.GetComponentsInChildren<ItemSlot>();
        var equipmentSlots = inventoryDisplay.GetComponentsInChildren<EquipmentSlot>().OrderBy(x => x.equipmentType).ToArray();
        var weaponSlots = inventoryDisplay.GetComponentsInChildren<WeaponSlot>();

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < inventory.storedItems.Count)
                itemSlots[i].SetSlot(inventory.storedItems[i]);
            else
                itemSlots[i].ClearSlot();
        }

        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            var equipment = inventory.storedEquipment[(EquipmentType)i];
            if (equipment != null)
                equipmentSlots[i].SetSlot(equipment);
            else
                equipmentSlots[i].ClearSlot();
        }

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            var weapon = inventory.storedWeapon[(WeaponHand)i];
            if (weapon != null)
                weaponSlots[i].SetSlot(weapon);
            else
                weaponSlots[i].ClearSlot();
        }

        weightText.text = $"Weight: {inventory.weight}/{inventory.maxWeight}";
    }

    private IEnumerator ChangeHealth(float newHealth)
    {
        var newMaxHealth = PlayerCenterControl.Instance.combat.stats.health;
        while ((maxHealth != newMaxHealth) || (currentHealth != newHealth))
        {
            if (Mathf.Abs(maxHealth - newMaxHealth) < 0.1f)
                maxHealth = newMaxHealth;
            else
                maxHealth = Mathf.Lerp(maxHealth, newMaxHealth, 5f * Time.unscaledDeltaTime);
            healthBar.maxValue = maxHealth;

            if (Mathf.Abs(currentHealth - newHealth) < 0.1f)
                currentHealth = newHealth;
            else
                currentHealth = Mathf.Lerp(currentHealth, newHealth, 5f * Time.unscaledDeltaTime);
            healthBar.value = currentHealth;
            yield return null;
        }
    }


}
