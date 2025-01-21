using System.IO;
using Flax.Build;
using Flax.Build.NativeCpp;

public class Ink : ThirdPartyModule
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();
        
        LicenseType = LicenseTypes.MIT;
        LicenseFilePath = "InkLicense.txt";

        // C#-only scripting if false
        BuildNativeCode = false;
        BuildCSharp = true;
    }

    /// <inheritdoc />
    public override void Setup(BuildOptions options)
    {
        base.Setup(options);

        options.ScriptingAPI.IgnoreMissingDocumentationWarnings = true;
        options.PublicIncludePaths.Add(FolderPath);
    }
}