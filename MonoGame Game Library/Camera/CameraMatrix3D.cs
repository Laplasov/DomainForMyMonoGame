using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoGame_Game_Library.Graphics
{
    public class CameraMatrix3D
    {
        public Matrix World { get; private set; } = Matrix.Identity;
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public Vector3 Position { get; private set; }
        public Vector3 Target { get; private set; }
        public Vector3 Up { get; private set; }

        public float FieldOfView { get; set; } = MathHelper.PiOver4; // 45°
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 100f;

        private GraphicsDevice _graphicsDevice;

        public CameraMatrix3D(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            Up = Vector3.Up;
            UpdateProjection();
        }

        public void SetLookAt(Vector3 position, Vector3 target, Vector3 up)
        {
            Position = position;
            Target = target;
            Up = up;
            UpdateView();
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;
            UpdateView();
        }

        public void SetTarget(Vector3 target)
        {
            Target = target;
            UpdateView();
        }

        public void OrbitAround(Vector3 center, float radius, float angle, float height)
        {
            Position = new Vector3(
                center.X + (float)Math.Cos(angle) * radius,
                center.Y + height,
                center.Z + (float)Math.Sin(angle) * radius
            );
            Target = center;
            UpdateView();
        }
        private void UpdateView() => View = Matrix.CreateLookAt(Position, Target, Up);

        private void UpdateProjection()
        {
            float aspectRatio = _graphicsDevice.Viewport.AspectRatio;
            Projection = Matrix.CreatePerspectiveFieldOfView(
                FieldOfView, aspectRatio, NearPlane, FarPlane);
        }

        public void OnViewportChanged() => UpdateProjection();

        // Utility methods
        public Vector3 ScreenToWorld(Vector2 screenPosition, float depth)
        {
            Vector3 source = new Vector3(screenPosition.X, screenPosition.Y, depth);
            Viewport viewport = _graphicsDevice.Viewport;
            return viewport.Unproject(source, Projection, View, World);
        }

        public Vector2 WorldToScreen(Vector3 worldPosition)
        {
            Viewport viewport = _graphicsDevice.Viewport;
            Vector3 screenPos = viewport.Project(worldPosition, Projection, View, World);
            return new Vector2(screenPos.X, screenPos.Y);
        }
    }
}