using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Application.Repository
{
    public interface ISceneProvider
    {
        public Scene? GetById(SceneId id);
        public void Save(Scene scene);
    }
}
