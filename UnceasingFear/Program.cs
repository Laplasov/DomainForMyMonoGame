namespace UnceasingFear
{
    public static class Program
{
    static void Main(string[] args)
    {
        using var game = new TestImplementation.Game1();
        game.Run();
    }
}
}