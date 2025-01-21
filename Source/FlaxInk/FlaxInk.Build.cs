using System.IO;
using Flax.Build;
using Flax.Build.NativeCpp;

public class FlaxInk : GameModule
{
    public override void Init()
    {
        base.Init();
        
        // C#-only scripting if false
        BuildNativeCode = false;
    }

    /// <inheritdoc />
    public override void Setup(BuildOptions options)
    {
        base.Setup(options);
        options.PublicDependencies.Add(nameof(Ink));
    }
}
