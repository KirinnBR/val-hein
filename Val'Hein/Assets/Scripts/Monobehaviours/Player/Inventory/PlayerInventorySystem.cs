using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventorySystem : MonoBehaviour
{
    [Header("Item detection settings")]

    [SerializeField]
    private Vector3 boxOffset = Vector3.zero;
    [SerializeField]
    private Vector3 boxSize = Vector3.one;
    [SerializeField]
    private float m_maxWeight = 100f;

    public float maxWeight => m_maxWeight;
    public float weight { get; private set; } = 0f;

    private PlayerInputSystem input { get => PlayerCenterControl.Instance.input; }
    private PlayerCombatSystem combat { get => PlayerCenterControl.Instance.combat; }
    private PlayerUISystem ui { get => PlayerCenterControl.Instance.ui; }
    private LayerMask itemsLayer { get => PlayerCenterControl.Instance.itemsLayer; }

    public UnityEvent onItemsChangeCallback = new UnityEvent();

    public List<ItemData> storedItems { get; private set; } = new List<ItemData>();
    public EquipmentItemData[] storedArmor { get; private set; }

    private bool hasItem = false;
    private Collider itemCollider;

    private void Start()
    {
        storedArmor = new EquipmentItemData[EquipmentItemData.EquipmentTypeLength];
    }

    private void Update()
    {
        if (hasItem)
        {
            if (input.Interact)
            {
                if (itemCollider.TryGetComponent(out IInteractable interactable))
                {
                    interactable.Interact();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        var col = Physics.OverlapBox(transform.position + transform.TransformDirection(boxOffset), boxSize / 2, Quaternion.LookRotation(transform.forward), itemsLayer);

        hasItem = col.Length > 0;

        if (hasItem)
        {
            itemCollider = col[0];
        }

        ui.InteractText.enabled = hasItem;
    }

    public bool Equip(EquipmentItemData item)
    {
        //TODO: Instantiate Equipment in world space.

        var itemIndex = item.Index;
        if (storedArmor[itemIndex] != null)
        {
            var oldItem = storedArmor[itemIndex];
            storedItems.Add(oldItem);
            
            weight += oldItem.weight;
            combat.Debuff(oldItem.buffStats);


            if (storedItems.Remove(item))
            {
                weight -= item.weight;
                storedArmor[itemIndex] = item;
                combat.Buff(item.buffStats);
                onItemsChangeCallback.Invoke();
                return true;
            }
            else
            {
                Debug.Log($"Could not find {item.name} in items.");
            }
        }
        else
        {
            if (storedItems.Remove(item))
            {
                Debug.Log("Equiping item");
                weight -= item.weight;
                storedArmor[itemIndex] = item;
                Debug.Log("Storing armor at " + itemIndex + " index");
                combat.Buff(item.buffStats);

                onItemsChangeCallback.Invoke();
                return true;
            }
            else
            {
                Debug.Log("Could not find equipment in items.");
            }
        }
        return false;
    }

    public bool Unequip(EquipmentItemData item)
    {
        Debug.Log("Unequiping item");
        storedArmor[item.Index] = null;
        storedItems.Add(item);
        weight += item.weight;

        combat.Debuff(item.buffStats);

        onItemsChangeCallback.Invoke();
        return true;
    }

    public bool StoreItem(ItemData item)
    {
        if (weight + item.weight <= maxWeight)
        {
            storedItems.Add(item);
            weight += item.weight;
            onItemsChangeCallback.Invoke();
            return true;
        }
        else
        {
            Debug.Log("The player is too heavy.");
            return false;
        }
    }

    public void RemoveItem(ItemData item)
    {
        if (storedItems.Remove(item))
        {
            weight -= item.weight;
            onItemsChangeCallback.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position + transform.TransformDirection(boxOffset), boxSize);
    }

}
