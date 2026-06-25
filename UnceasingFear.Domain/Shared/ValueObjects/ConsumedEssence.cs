using System.Xml.Linq;
using UnceasingFear.Domain.Shared.Enums;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using UnceasingFear.Domain.Shared.ValueObjects.Stats;
using static UnceasingFear.Domain.Shared.ValueObjects.ConsumedEssence;

namespace UnceasingFear.Domain.Shared.ValueObjects
{
    public readonly record struct ConsumedEssence(IReadOnlyList<Essence> EssenceList)
    {
        public readonly record struct StatMod(StatType Type, int Value);
        public readonly record struct Essence(string Name, IReadOnlyList<StatMod> Mods, int Value);
        public static ConsumedEssence Empty => new(Array.Empty<Essence>());
        public ConsumedEssence AddEssence(Item item) => 
            this with { EssenceList = [.. EssenceList, FromItem(item)] };
        public ConsumedEssence RemoveLastEssence() =>
            EssenceList.Count > 0 ? this with { EssenceList = [.. EssenceList.ToArray()[..^1]] } : this;
        private Essence FromItem(Item item)
        {
            if (item.Type != "Essence")
                throw new InvalidOperationException($"Cannot consume item of type '{item.Type}' as an essence.");

            return new Essence(item.Name, GetStatMod(item.Name, item.Value), item.Value);
        }

        private IReadOnlyList<StatMod> GetStatMod(string Name, int Value)
        {
            var listMods = new List<StatMod>();
            var statType = Name switch
            {
                "Sanguis" => new StatMod(StatType.MaxHP, 5 * Value),
                "Vigor" => new StatMod(StatType.MaxSP, 5 * Value),
                "Vis" => new StatMod(StatType.Physic, 2 * Value),
                "Tutamen" => new StatMod(StatType.Defense, 2 * Value),
                "Anima" => new StatMod(StatType.Magic, 2 * Value),
                "Celeritas" => new StatMod(StatType.Speed, 2 * Value),

                _ => new StatMod(StatType.None, Value)
            };
            listMods.Add(statType);

            return listMods;
        }
        public int GetBonus(StatType type)
        {
            int total = 0;
            foreach (Essence e in EssenceList)
            {
                foreach (StatMod stat in e.Mods)
                {
                    if (stat.Type == type)
                        total += stat.Value;
                }
            }
            return total;
        }
        public StatDelta CalculateTotalBonus()
        {
            var totalDelta = StatDelta.Zero;
            foreach (Essence e in EssenceList)
            {
                foreach (StatMod stat in e.Mods)
                {
                    totalDelta = stat.Type switch
                    {
                        StatType.Physic => totalDelta with { Physic = totalDelta.Physic + stat.Value },
                        StatType.Defense => totalDelta with { Defense = totalDelta.Defense + stat.Value },
                        StatType.Magic => totalDelta with { Magic = totalDelta.Magic + stat.Value },
                        StatType.Speed => totalDelta with { Speed = totalDelta.Speed + stat.Value },
                        StatType.MaxHP => totalDelta with { MaxHp = totalDelta.MaxHp + stat.Value },
                        StatType.MaxSP => totalDelta with { MaxSp = totalDelta.MaxSp + stat.Value },
                        _ => totalDelta 
                    };
                }
            }
            return totalDelta;
        }
    }
}