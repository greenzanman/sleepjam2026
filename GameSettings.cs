using Godot;
using System;

public static class GameSettings
{
    public static int ScreenHeight = 720;
    public static int ScreenWidth = 1280;

    public static Color colorDark = Color.Color8(2, 5, 36, 255);
    public static Color colorLight = Color.Color8(140, 140, 170, 255);
    public static Color colorInvisible = new Color(0, 0, 0, 0);

    public static int PenLeft = 440;
    public static int PenRight = 840;
    public static int PenTop = 235;
    public static int PenBottom = 485;
    public static int PenDivider = 640;
}