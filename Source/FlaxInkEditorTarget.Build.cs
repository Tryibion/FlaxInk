using Flax.Build;

public class FlaxInkEditorTarget : GameProjectEditorTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for editor
        Modules.Add(nameof(Ink));
        Modules.Add(nameof(FlaxInk));
        Modules.Add(nameof(FlaxInkEditor));
    }
}
