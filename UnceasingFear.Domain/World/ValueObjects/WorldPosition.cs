namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct WorldPosition(float X, float Y)
    {
        public static WorldPosition Zero => new(0, 0);
        public float DistanceTo(WorldPosition other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            return MathF.Sqrt((dx * dx) + (dy * dy));
        }
    }
}
