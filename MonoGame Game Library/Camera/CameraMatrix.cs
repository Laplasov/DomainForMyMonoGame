using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGame_Game_Library.Camera
{
    public class CameraMatrix
    {
        public float CameraAxeX { get; set; }
        public float CameraAxeY { get; set; }
        public float Zoom { get; set; }
        public float ScrollPower { get; set; }
        public int MovementSpeed { get; set; }
        public float TrackSmoothness { get; set; } = 0.2f;
        public bool FreeCamera { get; set; } = true;

        GraphicsDeviceManager _graphicsDeviceManager;
        int _previousScrollValue;
        float _zoomAccumulator;

        private ITrackable _trackedObject;

        public CameraMatrix(GraphicsDeviceManager graphicsDeviceManager, int movementSpeed = 10, float scrollPower = 0.001f)
        {
            _graphicsDeviceManager = graphicsDeviceManager;
            _previousScrollValue = Mouse.GetState().ScrollWheelValue;
            _zoomAccumulator = 1f;

            MovementSpeed = movementSpeed;
            ScrollPower = scrollPower;
            Zoom = 1f;
        }
        public void ToggleTracking()
        {
            if(_trackedObject == null) 
                return;
            FreeCamera = !FreeCamera;
            _trackedObject.LockPosition = FreeCamera;
        }
        public void TrackTarget(ITrackable trackable)
        {
            _trackedObject = trackable;
            FreeCamera = false;
        }

        public Matrix GetMatrix()
        {
            Vector2 viewCenter = new Vector2(
                _graphicsDeviceManager.PreferredBackBufferWidth * 0.5f + CameraAxeX, 
                _graphicsDeviceManager.PreferredBackBufferHeight * 0.5f + CameraAxeY);

            return Matrix.CreateTranslation(-viewCenter.X, -viewCenter.Y, 0) *
                         Matrix.CreateScale(Zoom) *
                         Matrix.CreateTranslation(viewCenter.X, viewCenter.Y, 0) *
                         Matrix.CreateTranslation(-CameraAxeX, -CameraAxeY, 0);
        }

        public void Update()
        {

            if (Core.Input.Keyboard.WasKeyJustPressed(Keys.F))
                ToggleTracking();

            if (!FreeCamera)
            {
                Tracker();
                Zoom = 1f;
                return;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
                CameraAxeX -= MovementSpeed / Zoom;

            if (Keyboard.GetState().IsKeyDown(Keys.W))
                CameraAxeY -= MovementSpeed / Zoom;

            if (Keyboard.GetState().IsKeyDown(Keys.D))
                CameraAxeX += MovementSpeed / Zoom;

            if (Keyboard.GetState().IsKeyDown(Keys.S))
                CameraAxeY += MovementSpeed / Zoom;

            int currentScroll = Mouse.GetState().ScrollWheelValue;
            int scrollDelta = currentScroll - _previousScrollValue;
            _previousScrollValue = currentScroll;

            if (scrollDelta != 0)
            {
                _zoomAccumulator += scrollDelta * ScrollPower;
                _zoomAccumulator = MathHelper.Clamp(_zoomAccumulator, 0.1f, 10f);
                Zoom = _zoomAccumulator;
            }
        }
        void Tracker()
        {
            Vector2 targetPos = _trackedObject.Position;

            float targetCameraX = targetPos.X - (_graphicsDeviceManager.PreferredBackBufferWidth * 0.5f);
            float targetCameraY = targetPos.Y - (_graphicsDeviceManager.PreferredBackBufferHeight * 0.5f);

            CameraAxeX = MathHelper.Lerp(CameraAxeX, targetCameraX, TrackSmoothness);
            CameraAxeY = MathHelper.Lerp(CameraAxeY, targetCameraY, TrackSmoothness);
        }
    }
}
