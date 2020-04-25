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
    [SerializeField]
    private LayerMask itemsLayer;

    public float maxWeight => m_maxWeight;
    public float weight { get; private set; } = 0f;

    private PlayerInputSystem input { get => PlayerCenterControl.Instance.input; }
    private PlayerCombatSystem combat { get => PlayerCenterControl.Instance.combat; }
    private PlayerUISystem ui { get => PlayerCenterControl.Instance.ui; }

    public UnityEvent onItemsChangeCallback = new UnityEvent();

    public List<ItemData> storedItems { get; private set; } = new List<ItemData>();

    public Dictionary<EquipmentType, EquipmentItemData> storedEquipment { get; } = new Dictionary<EquipmentType, EquipmentItemData>(4);
    public Dictionary<WeaponHand, WeaponItemData> storedWeapon { get; } = new Dictionary<WeaponHand, WeaponItemData>(2);

    private bool hasItem = false;

    private Collider[] itemCollider = new Collider[1];

    private void Start()
    {
        storedEquipment.Add(EquipmentType.Chest, null);
        storedEquipment.Add(EquipmentType.Gloves, null);
        storedEquipment.Add(EquipmentType.Pants, null);
        storedEquipment.Add(EquipmentType.Boots, null);
        storedWeapon.Add(WeaponHand.LeftHanded, null);
        storedWeapon.Add(WeaponHand.RightHanded, null);
    }

    private void Update()
    {
        if (input.Interact)
        {
            if (hasItem)
            {
                if (itemCollider[0].TryGetComponent(out IInteractable interactable))
                {
                    interactable.Interact();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        var hit = Physics.OverlapBoxNonAlloc(transform.position + transform.TransformDirection(boxOffset), boxSize / 2, itemCollider, Quaternion.LookRotation(transform.forward), itemsLayer);

        hasItem = hit == 1;

        ui.InteractText.enabled = hasItem;
    }

    public bool Equip(EquipmentItemData item)
    {
        //TODO: Instantiate Equipment in world space.

        var oldItem = storedEquipment[item.type];

        if (oldItem != null)
        {
            //Taking off old equipment.
            Store(oldItem);
            combat.Debuff(oldItem.buffStats);

            //Assigning new equipment.
            if (storedItems.Remove(item))
            {
                weight -= item.weight;
                storedEquipment[item.type] = item;
                combat.Buff(item.buffStats);
                onItemsChangeCallback.Invoke();
                return true;
            }
            else
            {
                Debug.LogError($"Could not find {item.name} in items.");
                return false;
            }
        }
        else
        {
            if (storedItems.Remove(item))
            {
                weight -= item.weight;
                storedEquipment[item.type] = item;
                combat.Buff(item.buffStats);
                onItemsChangeCallback.Invoke();
                return true;
            }
            else
            {
                Debug.LogError($"Could not find {item.name} in items.");
                return false;
            }
        }
    }

    public bool Equip(WeaponItemData item)
    {
        if (item.hand == WeaponHand.DoubleHanded)
        {

            if (storedWeapon[WeaponHand.LeftHanded] != null)
            {
                var oldItem = storedWeapon[WeaponHand.LeftHanded];

                Store(oldItem);

                combat.Debuff(oldItem.buff);
            }

            if (storedWeapon[WeaponHand.RightHanded] != null)
            {
                var oldItem = storedWeapon[WeaponHand.RightHanded];

                Store(oldItem);

                combat.Debuff(oldItem.buff);
            }

            if (storedItems.Remove(item))
            {
                storedWeapon[WeaponHand.LeftHanded] = item;
                weight -= item.weight;
                combat.Buff(item.buff);
                onItemsChangeCallback.Invoke();
                return true;
            }
            else
            {
                Debug.LogError($"Could not find {item.name} in items.");
                return false;
            }
        }
        else
        {
            var oldItem = storedWeapon[item.hand];
            if (oldItem != null)
            {
                Store(oldItem);
                combat.Debuff(oldItem.buff);

                if (storedItems.Remove(item))
                {
                    storedWeapon[item.hand] = item;
                    weight -= item.weight;
                    combat.Buff(item.buff);
                    onItemsChangeCallback.Invoke();
                    return true;
                }
                else
                {
                    Debug.LogError($"Could not find {item.name} in items.");
                    return false;
                }
            }
            else
            {
                if (storedItems.Remove(item))
                {
                    storedWeapon[item.hand] = item;
                    weight -= item.weight;
                    combat.Buff(item.buff);
                    onItemsChangeCallback.Invoke();
                    return true;
                }
                else
                {
                    Debug.LogError($"Could not find {item.name} in items.");
                    return false;
                }
            }
        }
    }


    public void Unequip(EquipmentItemData item)
    {
        storedEquipment[item.type] = null;
        combat.Debuff(item.buffStats);

        Store(item);

        onItemsChangeCallback.Invoke();
    }

    public void Unequip(WeaponItemData item)
    {
        if (item.hand == WeaponHand.DoubleHanded)
            storedWeapon[WeaponHand.LeftHanded] = null;
        else
            storedWeapon[item.hand] = null;

        combat.Debuff(item.buff);

        Store(item);

        onItemsChangeCallback.Invoke();
    }

    public void StoreItem(ItemData item)
    {
        Store(item);
        onItemsChangeCallback.Invoke();
    }

    private void Store(ItemData item)
    {
        storedItems.Add(item);
        weight += item.weight;
        if (weight >= maxWeight)
            Debug.Log("The player is too heavy.");
    }

    public void RemoveItem(ItemData item)
    {
        Remove(item);
        onItemsChangeCallback.Invoke();
    }

    private void Remove(ItemData item)
    {
        if (storedItems.Remove(item))
        {
            weight -= item.weight;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position + transform.TransformDirection(boxOffset), boxSize);
    }


}
