using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnceasingFear.Application.Collision
{
    public interface ICollisionChecker
    {
        bool IsWalkable(float x, float y);
    }
}
