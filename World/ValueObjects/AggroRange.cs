namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct AggroRange(float Value)
    {
        public bool IsInRange(WorldPosition from, WorldPosition to)
            => from.DistanceTo(to) <= Value;
    }
}
