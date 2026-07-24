using UnceasingFear.Application.Combat.Snapshots;
using UnceasingFear.Domain.Combat.Events;
using UnceasingFear.Domain.Shared.Events;

namespace UnceasingFear.Presentation.Render.Battle
{
    /// <summary>
    /// Reads BattleSnapshot and writes values into BattleHudHandles each frame.
    /// No widget construction, no drawing — only data binding.
    /// </summary>
    public class BattleHudUpdater
    {
        private readonly BattleHudHandles _handles;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Queue<string> _lines = new();
        private const int MaxLines = 6;

        public BattleHudUpdater(BattleHudHandles handles, IEventDispatcher eventDispatcher)
        {
            _handles = handles;
            _eventDispatcher = eventDispatcher;

            _eventDispatcher.Subscribe<CombatEvents.AbilitySucceededEvent>(e =>
                PushLog($"{e.ActorName} used {e.AbilityName} for {e.Power} dmg"));

            _eventDispatcher.Subscribe<CombatEvents.AbilityFailedEvent>(e =>
                PushLog($"{e.ActorName}: {e.Reason}"));

            _eventDispatcher.Subscribe<CombatEvents.UnitDamagedEvent>(e =>
                PushLog($"{e.Name} took {e.Amount} damage"));
        }
        private void PushLog(string line)
        {
            _lines.Enqueue(line);
            if (_lines.Count > MaxLines)
                _lines.Dequeue();
            _handles.LogPlaceholder.Text = string.Join("\n", _lines);
        }

        public void Update(BattleSnapshot snapshot)
        {
            UpdateStatsPanel(snapshot);
            UpdateAbilityButtons(snapshot);
            UpdateUnitBars(snapshot);
            UpdateActionButtons(snapshot);
        }

        // ── Action buttons ──────────────────────────────────────────────────────
        private void UpdateActionButtons(BattleSnapshot snapshot)
        {
            // Enable Pass/Run/Items/Talk only when waiting for player input
            bool isEnabled = snapshot.IsWaitingForPlayerInput;

            foreach (var btn in _handles.ActionButtons)
            {
                btn.IsEnabled = isEnabled;
            }
        }

        // ── Stats panel ──────────────────────────────────────────────────────

        private void UpdateStatsPanel(BattleSnapshot snapshot)
        {
            if (snapshot.CurrentActorId == null || !snapshot.IsWaitingForPlayerInput)
            {
                ClearStatsPanel();
                return;
            }

            var actor = snapshot.Units.FirstOrDefault(u => u.Id == snapshot.CurrentActorId);
            if (actor == default) return;

            _handles.ActiveUnitName.Text = actor.Name;
            _handles.Identity.Text = $"{actor.Identity.Type} {actor.Identity.Element} {actor.Identity.Tier}";
            _handles.StatHp.Text = $"HP:  {actor.CurrentHp} / {actor.MaxHp}";
            _handles.StatSp.Text = $"SP:  {actor.CurrentSp} / {actor.MaxSp}";
            _handles.StatAtk.Text = $"PHY: {actor.Physic}";
            _handles.StatDef.Text = $"DEF: {actor.Defense}";
            _handles.StatMag.Text = $"MAG: {actor.Magic}";
            _handles.StatSpd.Text = $"SPD: {actor.Speed}";
        }

        private void ClearStatsPanel()
        {
            _handles.ActiveUnitName.Text = "—";
            _handles.Identity.Text = "—";
            _handles.StatHp.Text = "HP:  —";
            _handles.StatSp.Text = "SP:  —";
            _handles.StatAtk.Text = "PHY: —";
            _handles.StatDef.Text = "DEF: —";
            _handles.StatMag.Text = "MAG: —";
            _handles.StatSpd.Text = "SPD: —";
        }

        // ── Ability buttons ──────────────────────────────────────────────────

        private void UpdateAbilityButtons(BattleSnapshot snapshot)
        {
            if (!snapshot.IsWaitingForPlayerInput || snapshot.CurrentActorId == null)
            {
                DisableAllAbilities();
                return;
            }

            var actor = snapshot.Units.FirstOrDefault(u => u.Id == snapshot.CurrentActorId);
            if (actor == default) { DisableAllAbilities(); return; }

            for (int i = 0; i < _handles.AbilityButtons.Length; i++)
            {
                var btn = _handles.AbilityButtons[i];

                if (i >= actor.Abilities.Count)
                {
                    btn.Text = "—";
                    btn.IsEnabled = false;
                    continue;
                }

                var ability = actor.Abilities[i];
                bool canAfford = actor.CurrentSp >= ability.SpCost;

                btn.Text = $"{ability.Name}\n{ability.SpCost} SP";
                btn.IsEnabled = canAfford;
            }
        }

        private void DisableAllAbilities()
        {
            foreach (var btn in _handles.AbilityButtons)
            {
                btn.Text = "—";
                btn.IsEnabled = false;
            }
        }

        private void UpdateUnitBars(BattleSnapshot snapshot)
        {
            UpdateUnitBarSide(_handles.AllyBars, snapshot.Units.Where(u => u.IsAlly));
            UpdateUnitBarSide(_handles.EnemyBars, snapshot.Units.Where(u => !u.IsAlly));
        }
        private void UpdateUnitBarSide(UnitBarHandles[] bars, IEnumerable<UnitSnapshot> units)
        {
            foreach (var bar in bars)
                bar.Container.Visible = false;

            foreach (var unit in units)
            {
                int i = unit.SlotIndex - 1;
                if (i < 0 || i > 5) continue;

                var bar = bars[i];
                bar.Container.Visible = unit.IsAlive;
                if (!unit.IsAlive) continue;

                bar.NameLabel.Text = $"[{unit.SlotIndex}] {unit.Name}";

                float hp = unit.MaxHp > 0 ? (float)unit.CurrentHp / unit.MaxHp : 0f;
                float sp = unit.MaxSp > 0 ? (float)unit.CurrentSp / unit.MaxSp : 0f;
                float turn = Math.Clamp(unit.TurnProgress / 100f, 0f, 1f);

                bar.HpBar.Width = (int)(140 * hp);
                bar.SpBar.Width = (int)(140 * sp);
                bar.TurnBar.Width = (int)(140 * turn);
            }
        }
    }
}