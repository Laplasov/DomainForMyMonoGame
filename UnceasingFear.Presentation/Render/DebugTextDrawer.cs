// File: UnceasingFear.Presentation/Render/DebugTextDrawer.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class DebugTextDrawer
{
    // 5x7 bitmap font for digits 0-9 (each row is one scanline)
    private static readonly byte[][] DigitBits = new byte[10][]
    {
        new byte[] { 0b01110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110 }, // 0
        new byte[] { 0b00100, 0b01100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110 }, // 1
        new byte[] { 0b01110, 0b10001, 0b00001, 0b00010, 0b00100, 0b01000, 0b11111 }, // 2
        new byte[] { 0b11111, 0b00010, 0b00100, 0b00010, 0b00001, 0b10001, 0b01110 }, // 3
        new byte[] { 0b00010, 0b00110, 0b01010, 0b10010, 0b11111, 0b00010, 0b00010 }, // 4
        new byte[] { 0b11111, 0b10000, 0b11110, 0b00001, 0b00001, 0b10001, 0b01110 }, // 5
        new byte[] { 0b00110, 0b01000, 0b10000, 0b11110, 0b10001, 0b10001, 0b01110 }, // 6
        new byte[] { 0b11111, 0b00001, 0b00010, 0b00100, 0b01000, 0b10000, 0b10000 }, // 7
        new byte[] { 0b01110, 0b10001, 0b10001, 0b01110, 0b10001, 0b10001, 0b01110 }, // 8
        new byte[] { 0b01110, 0b10001, 0b10001, 0b01111, 0b00001, 0b00010, 0b01100 }, // 9
    };

    public static void DrawDigit(SpriteBatch sb, Texture2D pixel, int digit, Vector2 pos, Color color, int scale = 2)
    {
        if (digit < 0 || digit > 9) return;
        var bits = DigitBits[digit];
        for (int row = 0; row < 7; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                if ((bits[row] & (1 << (4 - col))) != 0)
                {
                    sb.Draw(pixel, new Rectangle(
                        (int)pos.X + col * scale,
                        (int)pos.Y + row * scale,
                        scale, scale), color);
                }
            }
        }
    }

    public static void DrawNumber(SpriteBatch sb, Texture2D pixel, int number, Vector2 pos, Color color, int scale = 2)
    {
        string s = number.ToString();
        float x = pos.X;
        foreach (char c in s)
        {
            if (char.IsDigit(c))
                DrawDigit(sb, pixel, c - '0', new Vector2(x, pos.Y), color, scale);
            x += 6 * scale; // 5px width + 1px spacing
        }
    }
}