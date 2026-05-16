using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoGame_Game_Library.Graphics
{
    /// <summary>
    /// Helper class for building and rendering 3D meshes with textures.
    /// </summary>
    public class Mesh3D
    {
        public VertexPositionTexture[] Vertices { get; private set; }
        public short[] Indices { get; private set; }
        public int TriangleCount => Indices.Length / 3;

        public Mesh3D(VertexPositionTexture[] vertices, short[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public void Draw(GraphicsDevice device)
        {
            device.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                Vertices, 0, Vertices.Length,
                Indices, 0, TriangleCount
            );
        }
    }

    /// <summary>
    /// Builder for creating simple 3D meshes like quads and grids.
    /// </summary>
    public static class MeshBuilder
    {
        /// <summary>
        /// Creates a simple quad (4 vertices, 2 triangles).
        /// </summary>
        /// <param name="width">Width of the quad</param>
        /// <param name="height">Height of the quad</param>
        /// <param name="centered">If true, quad is centered at origin. If false, starts at origin.</param>
        public static Mesh3D CreateQuad(float width, float height, bool centered = true)
        {
            var verts = new VertexPositionTexture[4];
            var indices = new short[] { 0, 1, 2, 0, 2, 3 };

            float xOffset = centered ? -width / 2 : 0;
            float yOffset = 0;

            // Bottom-left, Bottom-right, Top-right, Top-left
            verts[0] = new VertexPositionTexture(new Vector3(xOffset, yOffset, 0), new Vector2(0, 1));
            verts[1] = new VertexPositionTexture(new Vector3(xOffset + width, yOffset, 0), new Vector2(1, 1));
            verts[2] = new VertexPositionTexture(new Vector3(xOffset + width, yOffset + height, 0), new Vector2(1, 0));
            verts[3] = new VertexPositionTexture(new Vector3(xOffset, yOffset + height, 0), new Vector2(0, 0));

            return new Mesh3D(verts, indices);
        }

        /// <summary>
        /// Updates a quad's UV coordinates for a specific texture region.
        /// </summary>
        public static void UpdateQuadUVs(Mesh3D mesh, Rectangle sourceRect, int textureWidth, int textureHeight)
        {
            float u1 = (float)sourceRect.X / textureWidth;
            float v1 = (float)sourceRect.Y / textureHeight;
            float u2 = (float)(sourceRect.X + sourceRect.Width) / textureWidth;
            float v2 = (float)(sourceRect.Y + sourceRect.Height) / textureHeight;

            // Update UVs while keeping positions
            mesh.Vertices[0].TextureCoordinate = new Vector2(u1, v2); // Bottom-left
            mesh.Vertices[1].TextureCoordinate = new Vector2(u2, v2); // Bottom-right
            mesh.Vertices[2].TextureCoordinate = new Vector2(u2, v1); // Top-right
            mesh.Vertices[3].TextureCoordinate = new Vector2(u1, v1); // Top-left
        }

        /// <summary>
        /// Creates a grid of quads on the XZ plane (horizontal).
        /// </summary>
        public static Mesh3D CreateGrid(int columns, int rows, float tileWidth, float tileHeight)
        {
            int vertexCount = columns * rows * 4;
            int indexCount = columns * rows * 6;

            var verts = new VertexPositionTexture[vertexCount];
            var indices = new short[indexCount];

            int vi = 0, ii = 0;

            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < columns; x++)
                {
                    float worldX = x * tileWidth;
                    float worldZ = z * tileHeight;

                    // Create 4 vertices for this tile
                    verts[vi + 0] = new VertexPositionTexture(
                        new Vector3(worldX, 0, worldZ), new Vector2(0, 0));
                    verts[vi + 1] = new VertexPositionTexture(
                        new Vector3(worldX + tileWidth, 0, worldZ), new Vector2(1, 0));
                    verts[vi + 2] = new VertexPositionTexture(
                        new Vector3(worldX + tileWidth, 0, worldZ + tileHeight), new Vector2(1, 1));
                    verts[vi + 3] = new VertexPositionTexture(
                        new Vector3(worldX, 0, worldZ + tileHeight), new Vector2(0, 1));

                    // Create 2 triangles (6 indices)
                    indices[ii++] = (short)(vi + 0);
                    indices[ii++] = (short)(vi + 1);
                    indices[ii++] = (short)(vi + 2);
                    indices[ii++] = (short)(vi + 0);
                    indices[ii++] = (short)(vi + 2);
                    indices[ii++] = (short)(vi + 3);

                    vi += 4;
                }
            }

            return new Mesh3D(verts, indices);
        }

        /// <summary>
        /// Updates grid UVs for each tile based on tile IDs and tileset layout.
        /// </summary>
        public static void UpdateGridTileUVs(Mesh3D mesh, int[,] tileData, int tilesetCols, int tilesetRows)
        {
            int rows = tileData.GetLength(0);
            int cols = tileData.GetLength(1);
            int vi = 0;

            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < cols; x++)
                {
                    int tileId = tileData[z, x];
                    Vector2[] uvs = GetTileUVs(tileId, tilesetCols, tilesetRows);

                    mesh.Vertices[vi + 0].TextureCoordinate = uvs[0];
                    mesh.Vertices[vi + 1].TextureCoordinate = uvs[1];
                    mesh.Vertices[vi + 2].TextureCoordinate = uvs[2];
                    mesh.Vertices[vi + 3].TextureCoordinate = uvs[3];

                    vi += 4;
                }
            }
        }

        private static Vector2[] GetTileUVs(int tileId, int tilesetCols, int tilesetRows)
        {
            if (tileId == 0)
            {
                return new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1f / tilesetCols, 0),
                    new Vector2(1f / tilesetCols, 1f / tilesetRows),
                    new Vector2(0, 1f / tilesetRows)
                };
            }

            int tileIndex = tileId - 1;
            int tileX = tileIndex % tilesetCols;
            int tileY = tileIndex / tilesetCols;

            float u = (float)tileX / tilesetCols;
            float v = (float)tileY / tilesetRows;
            float uW = 1f / tilesetCols;
            float vH = 1f / tilesetRows;

            return new Vector2[]
            {
                new Vector2(u, v),
                new Vector2(u + uW, v),
                new Vector2(u + uW, v + vH),
                new Vector2(u, v + vH)
            };
        }
    }
}
