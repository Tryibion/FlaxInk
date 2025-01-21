using Flax.Build;

public class FlaxInkTarget : GameProjectTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for game
        Modules.Add(nameof(Ink));
        Modules.Add(nameof(FlaxInk));
    }
}
