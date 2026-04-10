using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Shared.ValueObjects;

namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct MovementSpeed(float Value);
    public readonly record struct Template(string TemplateName, IReadOnlyList<UnitProfile> Profiles);
}
