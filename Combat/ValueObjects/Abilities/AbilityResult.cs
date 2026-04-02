using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.Combat.ValueObjects.Abilities
{
    public abstract record AbilityResult
    {

        public sealed record Success(AbilityEffect Effect) : AbilityResult;
        public sealed record Failure(string Reason) : AbilityResult;
    }

}
