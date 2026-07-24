using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnceasingFear.Domain.Alchemy.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects;

namespace UnceasingFear.Persistence.XML
{
    public class XmlRecipeRepository
    {
        private readonly List<Recipe> _recipes;

        public XmlRecipeRepository(string filePath)
        {
            var doc = XDocument.Load(filePath);
            _recipes = doc.Root!.Elements("Recipe").Select(ParseRecipe).ToList();
        }

        public IReadOnlyList<Recipe> GetRecipes() => _recipes;

        private static Recipe ParseRecipe(XElement el) =>
            new(ParseContainer(el.Element("ingredients")!), ParseContainer(el.Element("output")!));

        private static Recipe.RecipeContainer ParseContainer(XElement el) => new(
            el.Elements("Identity").Select(ParseOutputIdentity).ToList(),
            el.Elements("Item").Select(ParseItem).ToList());

        // Recipe XML uses `value` where Ability XML uses `tier` for the same "Any"/int concept —
        // kept as a separate method rather than reusing AbilityXmlMapper.ParseIdentity so the two
        // schemas can drift independently; consider unifying the attribute name in the XML later.
        private static Identity ParseOutputIdentity(XElement el) => new()
        {
            Element = el.Attribute("element")!.Value == "Any" ? Element.None : Enum.Parse<Element>(el.Attribute("element")!.Value),
            Type = el.Attribute("type")!.Value == "Any" ? UnitType.None : Enum.Parse<UnitType>(el.Attribute("type")!.Value),
            Tier = el.Attribute("value")!.Value == "Any" ? 0 : int.Parse(el.Attribute("value")!.Value)
        };

        private static Item ParseItem(XElement el) =>
           new Item(Guid.Empty, el.Attribute("type")!.Value, el.Attribute("name")!.Value, 1, int.Parse(el.Attribute("value")?.Value ?? "1"), string.Empty, false);
    }
}
