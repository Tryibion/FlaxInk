using System;
using System.Collections.Generic;
using System.IO;
using FlaxEditor;
using FlaxEditor.Content;
using FlaxEditor.GUI.ContextMenu;
using FlaxEditor.Options;
using FlaxEngine;

namespace FlaxInkEditor;

/// <summary>
/// InkItem.
/// </summary>
public class InkFileItem : AssetItem
{
    private string savedPath;
    public InkFileItem(string path, string typeName, ref Guid id) : base(path, typeName, ref id)
    {
        ShowFileExtension = false;
        savedPath = path;
        PluginManager.GetPlugin<InkEditorPlugin>().RegisterInkItem(this);
    }

    public override void OnContextMenu(ContextMenu menu)
    {
        base.OnContextMenu(menu);
        menu.AddButton("Compile Ink File", () => PluginManager.GetPlugin<InkEditorPlugin>().CompileInkAndSave(Path));
    }

    public override void OnPathChanged()
    {
        var editor = Editor.Instance;
        var oldCompiledPath = $"{System.IO.Path.Combine(System.IO.Path.GetDirectoryName(savedPath), System.IO.Path.GetFileNameWithoutExtension(savedPath))}.json";
        var oldItem = editor.ContentDatabase.Find(oldCompiledPath);
        if (oldItem != null)
        {
            var newCompiledPath = $"{System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), System.IO.Path.GetFileNameWithoutExtension(Path))}.json";
            Editor.Instance.ContentDatabase.Move(oldItem, newCompiledPath);
        }
        
        savedPath = Path;
        base.OnPathChanged();
    }

    public override void OnDelete()
    {
        var editor = Editor.Instance;
        var compiledPath = $"{System.IO.Path.Combine(System.IO.Path.GetDirectoryName(savedPath), System.IO.Path.GetFileNameWithoutExtension(savedPath))}.json";
        var item = editor.ContentDatabase.Find(compiledPath);
        if (item != null)
        {
            editor.ContentDatabase.Delete(item);
        }
        PluginManager.GetPlugin<InkEditorPlugin>().UnregisterInkItem(this);
        base.OnDelete();
    }

    public override SpriteHandle DefaultThumbnail
    {
        get
        {
            // Try to load already loaded asset first.
            var atlas = PluginManager.GetPlugin<InkEditorPlugin>().InkFileAtlas;
            if (atlas == null)
            {
                atlas = Content.Load<SpriteAtlas>("Plugins/FlaxInk/Content/Editor/Icons/InkFileIcon-Large.flax");
                atlas.WaitForLoaded();
            }
            return atlas.FindSprite("Default");
        }
    }
    
    public override ContentItemType ItemType => ContentItemType.Other;
    public override ContentItemSearchFilter SearchFilter => ContentItemSearchFilter.Other;
    public override string TypeDescription => "Ink File";
}
