using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGame_Game_Library.TileLogic
{
    public class TileLayer
    {
        public string Name { get; set; }
        public int[,] TileData { get; set; }
        public bool Visible { get; set; } = true;
        public Texture2D Tileset { get; set; }
        public int TilesetColumns { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
    }
}
