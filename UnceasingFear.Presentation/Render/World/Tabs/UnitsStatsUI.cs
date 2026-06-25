using Gum.Forms.Controls;
using Gum.Managers;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using MonoGameGum.GueDeriving;
using UnceasingFear.Domain.Shared.ValueObjects;

namespace UnceasingFear.Presentation.Render.World.Tabs
{
    internal class UnitsStatsUI
    {
        public Label DetailName = new Label();
        public Label DetailHp = new Label();
        public Label DetailSp = new Label();
        public Label DetailPhysic = new Label();
        public Label DetailDefense = new Label();
        public Label DetailMagic = new Label();
        public Label DetailSpeed = new Label();

        public void CreatStatsUI(GraphicalUiElement rightParent)
        {
            rightParent.ChildrenLayout = ChildrenLayout.TopToBottomStack;
            rightParent.StackSpacing = 8;

            DetailName.Text = "Hover over a unit"; rightParent.Children.Add(DetailName.Visual);
            DetailHp.Text = ""; rightParent.Children.Add(DetailHp.Visual);
            DetailSp.Text = ""; rightParent.Children.Add(DetailSp.Visual);
            DetailPhysic.Text = ""; rightParent.Children.Add(DetailPhysic.Visual);
            DetailDefense.Text = ""; rightParent.Children.Add(DetailDefense.Visual);
            DetailMagic.Text = ""; rightParent.Children.Add(DetailMagic.Visual);
            DetailSpeed.Text = ""; rightParent.Children.Add(DetailSpeed.Visual);
        }

        public void UpdateStats(UnitProfile profile)
        {
            // UnitProfile is a struct, so FirstOrDefault returns a default struct with empty names if not found
            if (string.IsNullOrEmpty(profile.Name))
            {
                ClearStats();
                return;
            }

            DetailName.Text = $"--- {profile.Name} ---";
            DetailHp.Text = $"HP:  {profile.Stats.Health.Current} / {profile.Stats.MaxHp}";
            DetailSp.Text = $"SP:  {profile.Stats.SpellPoints.Current} / {profile.Stats.MaxSp}";
            DetailPhysic.Text = $"PHY: {profile.Stats.Physic}";
            DetailDefense.Text = $"DEF: {profile.Stats.Defense}";
            DetailMagic.Text = $"MAG: {profile.Stats.Magic}";
            DetailSpeed.Text = $"SPD: {profile.Stats.Speed}";
        }

        public void ClearStats()
        {
            if (DetailName == null) return;
            DetailName.Text = "Hover over a unit";
            DetailHp.Text = "";
            DetailSp.Text = "";
            DetailPhysic.Text = "";
            DetailDefense.Text = "";
            DetailMagic.Text = "";
            DetailSpeed.Text = "";
        }
    }
}