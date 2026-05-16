using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame_Game_Library.Graphics
{
    public class BillboardSprite
    {
        private BasicEffect _effect;
        private Mesh3D _mesh;
        private Vector3 _position;

        public Vector3 Position { get => _position; set => _position = value; }
        public Vector2 Size { get; set; }

        public BillboardSprite(GraphicsDevice device, Vector3 position, float width = 1f, float height = 1f)
        {
            _position = position;
            _mesh = MeshBuilder.CreateQuad(width, height, centered: true);
            Size = new Vector2(width, height);

            _effect = new BasicEffect(device)
            {
                TextureEnabled = true,
                VertexColorEnabled = false,
                LightingEnabled = false,
                Alpha = 1.0f
            };
        }

        public void UpdateTexture(Texture2D texture, Rectangle sourceRect)
        {
            _effect.Texture = texture;
            MeshBuilder.UpdateQuadUVs(_mesh, sourceRect, texture.Width, texture.Height);
        }

        public void Draw(CameraMatrix3D camera, GraphicsDevice device)
        {
            if (_effect.Texture == null) return;

            Matrix rotation = GetBillboardRotation(camera);
            Matrix world = rotation * Matrix.CreateTranslation(_position);

            _effect.World = world;
            _effect.View = camera.View;
            _effect.Projection = camera.Projection;
            _effect.Alpha = 1.0f;

            var oldRasterizer = device.RasterizerState;
            var oldDepthStencil = device.DepthStencilState;


            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.DepthRead;
            device.BlendState = BlendState.AlphaBlend;
            device.SamplerStates[0] = SamplerState.PointClamp;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _mesh.Draw(device);
            }

            device.RasterizerState = oldRasterizer;
            device.DepthStencilState = oldDepthStencil;
        }

        private Matrix GetBillboardRotation(CameraMatrix3D camera)
        {
            Vector3 toCamera = Vector3.Normalize(camera.Position - _position);
            Vector3 right = Vector3.Normalize(Vector3.Cross(Vector3.Up, toCamera));
            Vector3 up = Vector3.Normalize(Vector3.Cross(toCamera, right));

            return new Matrix(
                right.X, right.Y, right.Z, 0,
                up.X, up.Y, up.Z, 0,
                toCamera.X, toCamera.Y, toCamera.Z, 0,
                0, 0, 0, 1);
        }
    }
}