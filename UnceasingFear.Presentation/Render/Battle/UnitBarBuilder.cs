using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Managers;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using UnceasingFear.Presentation.Input;
using UnceasingFear.Presentation.Render.Battle;

public class UnitBarBuilder
{
    private const int EntryWidth = 160;
    private const int EntryHeight = 52;
    private const int EntrySpacing = 6;
    private const int FillBarWidth = 140;
    private const int FillBarHeight = 6;
    private const int FillBarSpacing = 4;

    private readonly BattleLayout _layout;

    public UnitBarBuilder(BattleLayout layout)
    {
        _layout = layout;
    }

    public UnitBarHandles[] BuildSide(bool isAlly)
    {
        var panel = CreatePanel(isAlly);
        var bars = new UnitBarHandles[6];
        for (int i = 0; i < 6; i++)
            bars[i] = CreateEntry(panel, slotNumber: i + 1);
        return bars;
    }

    private ColoredRectangleRuntime CreatePanel(bool isAlly)
    {
        int panelHeight = _layout.ScreenHeight / 2;
        int panelY = (_layout.ScreenHeight - panelHeight) / 2;
        int panelX = isAlly ? 0 : _layout.ScreenWidth - EntryWidth - 8;

        var panel = new ColoredRectangleRuntime();
        panel.X = panelX;
        panel.Y = panelY;
        panel.Width = EntryWidth + 8;
        panel.Height = panelHeight;
        panel.Color = Color.Transparent;
        panel.ChildrenLayout = ChildrenLayout.TopToBottomStack;
        panel.StackSpacing = EntrySpacing;
        panel.AddToRoot();
        return panel;
    }

    private UnitBarHandles CreateEntry(GraphicalUiElement parent, int slotNumber)
    {
        var container = new ColoredRectangleRuntime();
        container.Width = EntryWidth;
        container.Height = EntryHeight;
        container.Color = new Color(20, 20, 30, 180);
        container.Visible = false;
        parent.Children.Add(container);

        var name = AddLabel($"[{slotNumber}] —", container, x: 4, y: 2);
        var hp = AddBar(container, yOffset: 20, bg: new Color(60, 0, 0, 200), fill: Color.Red);
        var sp = AddBar(container, yOffset: 20 + FillBarHeight + FillBarSpacing, bg: new Color(0, 0, 60, 200), fill: Color.CornflowerBlue);
        var turn = AddBar(container, yOffset: 20 + (FillBarHeight + FillBarSpacing) * 2, bg: new Color(40, 40, 0, 200), fill: Color.Yellow);

        return new UnitBarHandles(container, name, hp, sp, turn);
    }

    private static Label AddLabel(string text, GraphicalUiElement parent, int x, int y)
    {
        var label = new Label();
        label.Text = text;
        label.Visual.X = x;
        label.Visual.Y = y;
        label.Visual.Width = -8;
        label.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
        parent.Children.Add(label.Visual);
        return label;
    }

    private ColoredRectangleRuntime AddBar(GraphicalUiElement parent, int yOffset, Color bg, Color fill)
    {
        AddRect(parent, yOffset, FillBarWidth, FillBarHeight, bg);
        return AddRect(parent, yOffset, FillBarWidth, FillBarHeight, fill);
    }

    private static ColoredRectangleRuntime AddRect(GraphicalUiElement parent, int y, int w, int h, Color color)
    {
        var rect = new ColoredRectangleRuntime();
        rect.X = 4;
        rect.Y = y;
        rect.Width = w;
        rect.Height = h;
        rect.Color = color;
        parent.Children.Add(rect);
        return rect;
    }
}