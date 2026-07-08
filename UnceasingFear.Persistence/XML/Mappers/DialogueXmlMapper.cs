using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnceasingFear.Domain.Shared.ValueObjects;

namespace UnceasingFear.Persistence.Xml.Mappers
{
    public static class DialogueXmlMapper
    {
        public static DialogueTree FromXml(XElement el)
        {
            var id = el.Attribute("id")!.Value;
            var nodes = el.Elements("Node").Select(NodeFromXml).ToList();

            var startNode = nodes.FirstOrDefault(n => n.Id == "start");
            if (startNode.Id == null)
            {
                startNode = nodes.FirstOrDefault();
            }

            return new DialogueTree(id, startNode, nodes);
        }

        private static DialogueNode NodeFromXml(XElement el)
        {
            var id = el.Attribute("id")!.Value;
            var speaker = el.Attribute("speaker")?.Value ?? string.Empty;
            var text = el.Attribute("text")?.Value ?? string.Empty;
            var choices = el.Elements("Choice").Select(ChoiceFromXml).ToList();

            return new DialogueNode(id, speaker, text, choices);
        }

        private static DialogueChoice ChoiceFromXml(XElement el)
        {
            var text = el.Attribute("text")!.Value;
            var actionStr = el.Attribute("action")!.Value;
            var action = Enum.Parse<ChoiceAction>(actionStr);

            var target = new DialogueTarget(el.Attribute("target")?.Value ?? string.Empty);

            var conditions = el.Elements("Condition")
                .Select(c => new DialogueCondition(
                    Enum.Parse<ConditionType>(c.Attribute("type")!.Value),
                    c.Attribute("value")?.Value ?? string.Empty
                ))
                .ToList();

            var receiveItems = el.Element("ReciveItems")?
                .Elements("Item")
                .Select(ItemFromXml)
                .ToList() ?? new List<Item>();

            return new DialogueChoice(text, action, target, receiveItems, conditions);
        }

        private static Item ItemFromXml(XElement el)
        {
            string stackableStr = el.Attribute("IsStackable")?.Value ?? "true";
            bool isStackable = stackableStr == "1" || stackableStr.Equals("true", StringComparison.OrdinalIgnoreCase);

            return new Item(
                Guid.NewGuid(),
                el.Attribute("type")!.Value,
                el.Attribute("name")!.Value,
                int.Parse(el.Attribute("quantity")!.Value),
                int.Parse(el.Attribute("value")!.Value),
                el.Attribute("description")?.Value ?? string.Empty,
               isStackable
            );
        }
    }
}
