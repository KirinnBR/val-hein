using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Item", menuName = "Item/Weapon Item")]
public class WeaponItemData : ItemData
{
    [SerializeField]
    private WeaponType m_type;
    public WeaponType type => m_type;

    [SerializeField]
    private WeaponHand m_hand;
    public WeaponHand hand => m_hand;

    [SerializeField]
    private AttributeStats m_buff;
    public AttributeStats buff => m_buff;

    public override bool Use()
    {
        return player.inventory.Equip(this);
    }

    public override bool Sell()
    {
        throw new System.NotImplementedException();
    }
}
