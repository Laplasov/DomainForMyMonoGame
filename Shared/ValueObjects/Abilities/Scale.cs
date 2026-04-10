using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using UnceasingFear.Domain.Combat.Enums;

namespace UnceasingFear.Domain.Shared.ValueObjects.Abilities
{
    public readonly record struct Scale
    {
        public StatType Stat { get; }
        public float Percentage { get; }
        public Scale(StatType stat, float percentage)
        {
            Stat = stat;
            Percentage = percentage;
        }
    }
}
