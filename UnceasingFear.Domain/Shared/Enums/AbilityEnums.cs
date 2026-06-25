using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.Shared.Enums
{
    public enum DamageType { Physical, Magical, Almighty }
    public enum TargetRange { Melee, Range, Self, Area, RowMelee, RowRange, Piercing, All }
    public enum Target { Enemy, Ally, All }
    public enum CostType { HP, SP, Item, Cooldown }
    public enum StatType { Physic, Defense, Magic, Speed, MaxHP, MaxSP, None }
    public enum StatusEffectType { None, Poison, Stun, Buff }
}
