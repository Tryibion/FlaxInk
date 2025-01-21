using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FlaxEditor;
using FlaxEditor.Content;
using FlaxEditor.Windows;
using FlaxEngine;

namespace FlaxInkEditor;

/// <summary>
/// InkProxy.
/// </summary>
[ContentContextMenu("New/Ink/Ink File")]
public class InkFileProxy : AssetProxy
{
    public override string Name => "Ink File";
    
    protected override bool IsVirtual => true;

    public override bool IsProxyFor(ContentItem item)
    {
        return item is InkFileItem;
    }

    public override bool CanCreate(ContentFolder targetLocation)
    {
        return targetLocation.CanHaveAssets;
    }

    public override string NewItemName => "Ink File";

    public override string FileExtension => "ink";
    
    public override EditorWindow Open(Editor editor, ContentItem item)
    {
        CreateProcessSettings settings = new CreateProcessSettings
        {
            ShellExecute = true,
            FileName = item.Path
        };
        Platform.CreateProcess(ref settings);
        return null;
    }

    public override void Create(string outputPath, object arg)
    {
        GetTemplatePath(out var path);
        string template = File.ReadAllText(path);
        File.WriteAllText(outputPath, template, Encoding.UTF8);
    }

    public virtual void GetTemplatePath(out string templatePath)
    {
        templatePath = StringUtils.NormalizePath(Path.Combine(Globals.ProjectFolder, "Plugins", "FlaxInk", "Content", "Editor", "Templates", "InkDefaultTemplate.txt"));
    }

    public override Color AccentColor { get; }

    public override string TypeName => typeof(InkFileItem).FullName;

    public override AssetItem ConstructItem(string path, string typeName, ref Guid id)
    {
        return new InkFileItem(path, typeName, ref id);
    }
}
