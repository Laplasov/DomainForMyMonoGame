using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.ValueObjects;
using System.Xml.Serialization;

namespace UnceasingFear.Persistence
{
    public class XmlSceneLoader
    {
        public Scene Load(SceneId id)
        {
            var serializer = new XmlSerializer(typeof(Scene));
            var xml = $"Content/DB/Scenes.xml";

            using (var stream = File.OpenRead(xml))
            {
                Scene? data = serializer.Deserialize(stream) as Scene;

                if (data == null) throw new ArgumentNullException();
                else return data;
            }
        }
    }
}
