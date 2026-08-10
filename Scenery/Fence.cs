using System.Collections.Generic;
using Godot;

public class Fence : SleepRect
{
    public static HashSet<Fence> Fences = new HashSet<Fence>();

    public override void _Ready()
    {
        // Don't need to be too safe since fences are permenant
        Fences.Add(this);
    }

    // If given point is within the test bounds
    public bool IsOver(Vector2 testPoint)
    {
        return testPoint.x >= RectPosition.x &&
            testPoint.x <= RectPosition.x + RectSize.x &&
            testPoint.y >= RectPosition.y &&
            testPoint.y <= RectPosition.y + RectSize.y;
    }
}
