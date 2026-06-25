namespace UnceasingFear.Domain.Shared.ValueObjects.Stats
{
    public readonly record struct StatDelta(
        int MaxHp,
        int MaxSp,
        int Physic,
        int Defense,
        int Magic,
        int Speed)
    {
        public static StatDelta Zero => new(0, 0, 0, 0, 0, 0);

        public static StatDelta operator +(StatDelta a, StatDelta b) => new(
            a.MaxHp + b.MaxHp,
            a.MaxSp + b.MaxSp,
            a.Physic + b.Physic,
            a.Defense + b.Defense,
            a.Magic + b.Magic,
            a.Speed + b.Speed);
        public static StatDelta operator -(StatDelta a, StatDelta b) => new(
            a.MaxHp - b.MaxHp, 
            a.MaxSp - b.MaxSp,
            a.Physic - b.Physic, 
            a.Defense - b.Defense,
            a.Magic - b.Magic,
            a.Speed - b.Speed);
    }

}