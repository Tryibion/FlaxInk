using Flax.Build;
using Flax.Build.NativeCpp;

public class FlaxInkEditor : GameEditorModule
{
    /// <inheritdoc />
    public override void Setup(BuildOptions options)
    {
        base.Setup(options);

        // Reference game source module to access game code types
        options.PublicDependencies.Add(nameof(Ink));
        options.PublicDependencies.Add(nameof(FlaxInk));
        
        BuildNativeCode = false;
    }
}
