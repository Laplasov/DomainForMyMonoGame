using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MonoGame_Game_Library.Camera
{
    public interface ITrackable
    {
        public Vector2 Position { get; set; }
        public bool LockPosition { get; set; }
    }
}
