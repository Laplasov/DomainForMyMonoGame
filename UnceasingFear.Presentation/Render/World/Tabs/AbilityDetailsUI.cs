using Gum.Forms.Controls;
using Gum.Managers;
using Gum.Wireframe;
using System.Linq;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;

namespace UnceasingFear.Presentation.Render.World.Tabs
{
    internal class AbilityDetailsUI
    {
        public Label DetailName = new Label();
        public Label DetailDescription = new Label();
        public Label DetailRange = new Label();
        public Label DetailTarget = new Label();
        public Label DetailCosts = new Label();
        public Label DetailScales = new Label();
        public Label DetailStatus = new Label();

        public void CreateAbilityDetailsUI(GraphicalUiElement parent)
        {
            parent.ChildrenLayout = ChildrenLayout.TopToBottomStack;
            parent.StackSpacing = 8;

            DetailName.Text = "Select an ability";
            parent.Children.Add(DetailName.Visual);

            DetailDescription.Text = "";
            parent.Children.Add(DetailDescription.Visual);

            DetailRange.Text = "";
            parent.Children.Add(DetailRange.Visual);

            DetailTarget.Text = "";
            parent.Children.Add(DetailTarget.Visual);

            DetailCosts.Text = "";
            parent.Children.Add(DetailCosts.Visual);

            DetailScales.Text = "";
            parent.Children.Add(DetailScales.Visual);

            DetailStatus.Text = "";
            parent.Children.Add(DetailStatus.Visual);
        }

        public void UpdateDetails(Ability ability)
        {
            if (string.IsNullOrEmpty(ability.Name))
            {
                ClearDetails();
                return;
            }

            DetailName.Text = $"--- {ability.Name} ---";
            DetailDescription.Text = ability.Description;
            DetailRange.Text = $"Range: {ability.Range}";
            DetailTarget.Text = $"Target: {ability.Target}";

            // Format Costs (e.g., "Cost: SP: 2")
            DetailCosts.Text = ability.Costs.Any()
                ? $"Cost: {string.Join(", ", ability.Costs.Select(c => $"{c.Stat}: {c.Value}"))}"
                : "Cost: None";

            // Format Scales as percentages (e.g., "Scales: Physic: 100%")
            DetailScales.Text = ability.Scales.Any()
                ? $"Scales: {string.Join(", ", ability.Scales.Select(s => $"{s.Stat}: {s.Percentage * 100}%"))}"
                : "Scales: None";

            // Format Status Effects (e.g., "Status: Poison: 5")
            DetailStatus.Text = ability.StatusEffects.Any()
                ? $"Status: {string.Join(", ", ability.StatusEffects.Select(s => $"{s.Stat}: {s.Value}"))}"
                : "Status: None";
        }

        public void ClearDetails()
        {
            if (DetailName == null) return;

            DetailName.Text = "Select an ability";
            DetailDescription.Text = "";
            DetailRange.Text = "";
            DetailTarget.Text = "";
            DetailCosts.Text = "";
            DetailScales.Text = "";
            DetailStatus.Text = "";
        }
    }
}