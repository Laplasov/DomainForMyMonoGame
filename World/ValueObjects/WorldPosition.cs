namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct WorldPosition(float X, float Y)
    {
        public static WorldPosition Zero => new(0, 0);
        public float DistanceTo(WorldPosition other)
            => MathF.Sqrt(MathF.Pow(X - other.X, 2) + MathF.Pow(Y - other.Y, 2));
    }
}
