using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventorySystem : MonoBehaviour
{
    [SerializeField]
    private float interactSphereRadius = 5f;
    [SerializeField]
    private float m_maxWeight = 100f;

    public float maxWeight => m_maxWeight;
    public float weight { get; private set; } = 0f;

    private PlayerInputSystem input { get => PlayerCenterControl.Instance.input; }
    private PlayerCombatSystem combat { get => PlayerCenterControl.Instance.combat; }
    private LayerMask itemsLayer { get => PlayerCenterControl.Instance.itemsLayer; }

    public UnityEvent onItemsChangeCallback = new UnityEvent();

    public List<ItemData> storedItems { get; private set; } = new List<ItemData>();
    public EquipmentItemData[] storedEquipment { get; private set; }


    private bool hasItem = false;

    private void Start()
    {
        storedEquipment = new EquipmentItemData[EquipmentItemData.EquipmentTypeLength];
    }

    private void Update()
    {
        if (hasItem)
        {
            if (input.Interact)
            {
                Collider[] itemCollider = new Collider[1];
                Physics.OverlapSphereNonAlloc(transform.position, interactSphereRadius, itemCollider, itemsLayer);
                if (itemCollider[0].TryGetComponent(out IInteractable interactable))
                {
                    interactable.Interact();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        hasItem = Physics.CheckSphere(transform.position, interactSphereRadius, itemsLayer);
    }

    public bool Equip(EquipmentItemData item)
    {
        var itemIndex = item.Index;
        if (storedEquipment[itemIndex] != null)
        {
            var oldItem = storedEquipment[itemIndex];
            Debug.Log($"Switching {oldItem.name} to {item.name}");
            storedItems.Add(oldItem);
            
            weight += oldItem.weight;
            combat.Debuff(oldItem.buffStats);
            storedEquipment[itemIndex] = null;


            if (storedItems.Remove(item))
            {
                weight -= item.weight;
                storedEquipment[itemIndex] = item;

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
                storedEquipment[itemIndex] = item;

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
        storedEquipment[item.Index] = null;
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
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, transform.forward, 360, interactSphereRadius);
        //Gizmos.color = Color.white;
        //Gizmos.DrawWireSphere(transform.position, interactSphereRadius);
    }

}
