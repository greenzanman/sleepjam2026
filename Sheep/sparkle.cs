using Godot;
using System;


// Simplistic sparkle particle
public class sparkle : Node2D
{
    Sprite spriteFront;
    Sprite spriteBack;
    const int frames = 6;
    const float lifetime = 0.35f;
    float age = 0;
    public override void _Ready()
    {
        spriteFront = GetNode<Sprite>("SpriteFront");
        spriteBack = GetNode<Sprite>("SpriteBack");
        spriteBack.Modulate = GameSettings.colorDark;
        spriteFront.Modulate = GameSettings.colorLight;
    }

    public override void _Process(float delta)
    {
        delta *= GameManager.GetTimeDilation();
        age += delta;
        if (age >= lifetime)
            QueueFree();
        else
        {
            int frame = (int) (age / lifetime * frames);
            spriteBack.Frame = frame;
            spriteFront.Frame = frame;
        }
    }


}
