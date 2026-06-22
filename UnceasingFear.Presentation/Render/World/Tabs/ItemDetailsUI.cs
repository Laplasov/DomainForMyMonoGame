using Gum.Forms.Controls;
using Gum.Managers;
using Gum.Wireframe;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.World;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Presentation.Render;

public class ItemDetailsUI
{
    public Label DetailName = new Label();
    public Label DetailQuantity = new Label();
    public Label DetailDescription = new Label();
    public Label DetailType = new Label();
    public Label DetailValue = new Label();
    public Label DetailOwnership = new Label();
    public Button Button = new Button();

    private PlayerMenu _playerMenu;

    private Item? _currentItem = null;
    // ❌ REMOVED: private UnitProfile? _currentOwner = null;

    private readonly ICommandDispatcher _commandDispatcher;

    public ItemDetailsUI(PlayerMenu playerMenu, ICommandDispatcher commandDispatcher)
    {
        _playerMenu = playerMenu;
        _commandDispatcher = commandDispatcher;
        Button.Click += OnActionButtonClicked;
    }

    // ✅ Look up the CURRENT owner from the live snapshot
    private UnitProfile? FindCurrentOwner()
    {
        if (_currentItem == null || _playerMenu.LastSnapshot == null) return null;

        foreach (var p in _playerMenu.LastSnapshot.Value.PartyProfiles)
        {
            if (p.EquippedItems.Any(eq => eq == _currentItem.Value))
                return p;
        }
        return null;
    }

    private void OnActionButtonClicked(object? sender, EventArgs e)
    {
        if (_currentItem == null) return;

        if (_playerMenu.UnitSelection)
        {
            _playerMenu.UnitSelection = false;
            UpdateDetails(_currentItem.Value);
            return;
        }

        // ✅ Always look up the FRESH owner at click time
        var currentOwner = FindCurrentOwner();

        if (currentOwner.HasValue)
        {
            _commandDispatcher.Dispatch(new UnequipItemCommand(_currentItem.Value, currentOwner.Value));
            ClearDetails();
        }
        else
        {
            _playerMenu.UnitSelection = true;
            Button.Text = "Cancel";
        }
        _playerMenu.UnitsTabDirty = true;
    }

    public void CreateItemDetailsUI(GraphicalUiElement parent)
    {
        parent.ChildrenLayout = ChildrenLayout.TopToBottomStack;
        parent.StackSpacing = 8;

        DetailName.Text = "Select an item";
        parent.Children.Add(DetailName.Visual);
        DetailQuantity.Text = ""; parent.Children.Add(DetailQuantity.Visual);
        DetailType.Text = ""; parent.Children.Add(DetailType.Visual);
        DetailValue.Text = ""; parent.Children.Add(DetailValue.Visual);
        DetailDescription.Text = ""; parent.Children.Add(DetailDescription.Visual);
        DetailOwnership.Text = ""; parent.Children.Add(DetailOwnership.Visual);
        parent.Children.Add(Button.Visual);
        Button.Text = "";
        Button.IsVisible = false;
    }

    public void SendCommand(UnitProfile profile)
    {
        if (_currentItem.HasValue)
        {
            _commandDispatcher.Dispatch(new EquipItemCommand(_currentItem.Value, profile));
            _playerMenu.UnitSelection = false;
            ClearDetails();

            // ✅ ADD THIS: Force the items list to rebuild so closures get fresh data
            _playerMenu.UnitsTabDirty = true;
        }
    }

    // ✅ No longer takes an owner parameter - it computes it fresh
    public void UpdateDetails(Item item)
    {
        if (_playerMenu.UnitSelection)
            _playerMenu.UnitSelection = false;

        _currentItem = item;

        if (string.IsNullOrEmpty(item.Name))
        {
            ClearDetails();
            return;
        }

        // ✅ Always look up FRESH ownership
        var currentOwner = FindCurrentOwner();

        string ownerText = currentOwner.HasValue
            ? $" [Equipped: {currentOwner.Value.Name}]"
            : " [In Stash]";

        DetailName.Text = $"--- {item.Name} ---";
        DetailQuantity.Text = $"Quantity: {item.Quantity}";
        DetailType.Text = $"Type: {item.Type}";
        DetailValue.Text = $"Value: {item.Value}";
        DetailDescription.Text = $"Desc: {item.Description}";
        DetailOwnership.Text = $"Equipped: {ownerText}";

        if (item.Type != "Fragment" && item.Type != "Money")
            Button.IsVisible = true;
        else
            Button.IsVisible = false;

        Button.Text = currentOwner.HasValue ? "Unequip" : "Equip";
    }

    public void ClearDetails()
    {
        _currentItem = null;
        DetailName.Text = "Select an item";
        DetailQuantity.Text = "";
        DetailDescription.Text = "";
        DetailType.Text = "";
        DetailValue.Text = "";
        DetailOwnership.Text = "";
        Button.IsVisible = false;
    }
}