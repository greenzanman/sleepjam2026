using Godot;
using System;

public static class GameSettings
{
    public static int ScreenHeight = 960;
    public static int ScreenWidth = 1280;

    public static Color colorLight = Color.Color8(2, 5, 36, 255);
    public static Color colorDark = Color.Color8(140, 140, 170, 255);
    public static Color colorInvisible = new Color(0, 0, 0, 0);

    public static int PenLeft = 425;
    public static int PenRight = 855;
    public static int PenTop = 345;
    public static int PenBottom = 615;
    public static int FenceX = 640;
    public static int FlashRate = 10;
}
