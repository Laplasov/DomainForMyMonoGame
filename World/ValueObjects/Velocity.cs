using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct Velocity(float X, float Y)
    {
        public static Velocity Zero => new(0, 0);

        public static Velocity Toward(WorldPosition from, WorldPosition to, MovementSpeed speed)
        {
            var dx = to.X - from.X;
            var dy = to.Y - from.Y;
            var dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist < 0.001f) return Zero;
            return new Velocity(dx / dist * speed.Value, dy / dist * speed.Value);
        }

        public static Velocity FromInput(float inputX, float inputY, MovementSpeed speed)
        {
            var len = MathF.Sqrt(inputX * inputX + inputY * inputY);
            if (len < 0.001f) return Zero;
            return new Velocity(inputX / len * speed.Value, inputY / len * speed.Value);
        }

        public WorldPosition Apply(WorldPosition position, float deltaTime)
            => new WorldPosition(
                position.X + X * deltaTime,
                position.Y + Y * deltaTime);

        public bool IsZero => X == 0 && Y == 0;
    }
}
