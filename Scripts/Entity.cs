using Godot;
using System;

namespace DesktopAssistant.Scripts;

public partial class Entity : CharacterBody2D
{

    /// <summary>最大移动速度</summary>
    [Export]
    public float MaxSpeed { get; set; } = 600.0f;

    /// <summary>重力加速度</summary>
    [Export]
    public float Gravity { get; set; } = 2000.0f;

    [Export]
    public bool AffectedByGravity { get; set; } = true;

    public override void _PhysicsProcess(double delta)
    {
        var dt = (float)delta;

        HandleMove(delta);
        if (AffectedByGravity)
            Velocity = Velocity with { Y = Velocity.Y + Gravity * dt };
        MoveAndSlide();
    }

    protected virtual void HandleMove(double delta)
    {

    }
}
