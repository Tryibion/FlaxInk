using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FlaxEditor;
using FlaxEditor.Content;
using FlaxEditor.GUI.ContextMenu;
using FlaxEngine;
using FlaxInk;
using Ink;
using Ink.Runtime;
using Path = System.IO.Path;

namespace FlaxInkEditor;

// Utility class for the ink compiler, used to work out how to find include files and their contents
public class FlaxInkFileHandler : IFileHandler
{
    private readonly string _rootDirectory;

    public FlaxInkFileHandler(string rootDirectory)
    {
        _rootDirectory = rootDirectory;
    }
    
    public string ResolveInkFilename(string includeName)
    {
        // Convert to Unix style, and then use FileInfo.FullName to parse any ..\
        return new FileInfo(Path.Combine(_rootDirectory, includeName).Replace('\\', '/')).FullName;
    }

    public string LoadInkFileContents(string fullFilename)
    {
        return File.ReadAllText(fullFilename);
    }
}

/// <summary>
/// Ink Editor Plugin.
/// </summary>
public class InkEditorPlugin : EditorPlugin
{
    internal SpriteAtlas InkFileAtlas;
    internal SpriteAtlas InkJSONFileAtlas;
    internal FileSystemWatcher InkFileWatcher;

    private ContextMenuButton _compileInkButton;
    
    private List<InkFileItem> _inkFileItems;
    
    /// <inheritdoc/>
    public override void InitializeEditor()
    {
        _inkFileItems = new List<InkFileItem>();
        InkFileAtlas = Content.Load<SpriteAtlas>("Plugins/FlaxInk/Content/Editor/Icons/InkFileIcon-Large.flax");
        InkFileAtlas.WaitForLoaded();
        
        InkJSONFileAtlas = Content.Load<SpriteAtlas>("Plugins/FlaxInk/Content/Editor/Icons/InkJSONFileIcon-Large.flax");
        InkJSONFileAtlas.WaitForLoaded();
        var inkJsonSprite = InkJSONFileAtlas.FindSprite("Default");

        base.InitializeEditor();

        Editor.ContentDatabase.AddProxy(new SpawnableJsonAssetProxy<InkStory>(inkJsonSprite));
        Editor.ContentDatabase.AddProxy(new InkFileProxy());
        Editor.ContentDatabase.Rebuild(true);

        // File watcher to capture changes to .ink files
        InkFileWatcher = new FileSystemWatcher(Globals.ProjectFolder, "*.ink")
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        };
        InkFileWatcher.Changed += InkFileWatcherOnChanged;

        var toolsButton = Editor.UI.MainMenu.GetButton("Plugins") ?? Editor.UI.MainMenu.AddButton("Plugins");
        var contextMenu = toolsButton.ContextMenu.GetOrAddChildMenu("Ink").ContextMenu;
        _compileInkButton = contextMenu.AddButton("Compile All Ink Files");
        _compileInkButton.Clicked += () =>
        {
            foreach (var item in _inkFileItems)
            {
                CompileInkAndSave(item.Path);
            }
        };

        Editor.ContentDatabase.ItemAdded += ContentDatabaseOnItemAdded;
        Editor.ContentDatabase.ItemRemoved += ContentDatabaseOnItemRemoved;
    }
    
    /// <summary>
    /// Register for editor plugin to keep track of.
    /// </summary>
    /// <param name="item">The ink file item.</param>
    public void RegisterInkItem(InkFileItem item)
    {
        if (!_inkFileItems.Contains(item))
            _inkFileItems.Add(item);
    }

    /// <summary>
    /// Unregister for editor plugin to not keep track of.
    /// </summary>
    /// <param name="item">The ink file item.</param>
    public void UnregisterInkItem(InkFileItem item)
    {
        if (_inkFileItems.Contains(item))
            _inkFileItems.Remove(item);
    }

    private void ContentDatabaseOnItemAdded(ContentItem item)
    {
        if (item is InkFileItem inkItem)
        {
            CompileInkAndSave(item.Path);
            RegisterInkItem(inkItem);
        }
    }

    private void ContentDatabaseOnItemRemoved(ContentItem item)
    {
        if (item is InkFileItem inkItem)
        {
            UnregisterInkItem(inkItem);
        }
    }

    private void InkFileWatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        CompileInkAndSave(e.FullPath);
    }

    /// <summary>
    /// Compiles and saves an Ink file to a InkStory.
    /// </summary>
    /// <param name="path">The path of the Ink file.</param>
    public void CompileInkAndSave(string path)
    {
        var jsonStory = CompileInkToJsonStory(path);
        if (jsonStory == null)
            return;
        var compiledPath = $"{Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path))}.json";
        Editor.SaveJsonAsset(compiledPath, new InkStory(jsonStory));
    }

    private string CompileInkToJsonStory(string path)
    {
        if (path == null)
            return null;
        if (!Path.GetExtension(path).Contains(".ink", StringComparison.OrdinalIgnoreCase))
            return null;

        string inkFileContents = string.Empty;
        using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var sr = new StreamReader(fs, Encoding.Default))
        {
            inkFileContents = sr.ReadToEnd();
        }
        Editor.Log($"Compiling ink file: {path}");
        //var inkFileContents = File.ReadAllText(e.FullPath);
        var compilerOptions = new Compiler.Options
        {
            countAllVisits = true,
            fileHandler = new FlaxInkFileHandler(Path.GetDirectoryName(path)),
        };
        var compiler = new Ink.Compiler(inkFileContents, compilerOptions);
        Story story = null;
        try
        {
            story = compiler.Compile();
        }
        catch (Exception e)
        {
            Editor.LogError($"Failed to compile story: {e.Message}");
        }

        if (story == null)
            return null;
        
        return story.ToJson();
    }

    /// <inheritdoc/>
    public override void DeinitializeEditor()
    {
        if (InkFileAtlas != null)
            Content.UnloadAsset(InkFileAtlas);
        InkFileAtlas = null;
        
        if (InkJSONFileAtlas != null)
            Content.UnloadAsset(InkJSONFileAtlas);
        InkJSONFileAtlas = null;
        
        InkFileWatcher.Changed -= InkFileWatcherOnChanged;
        InkFileWatcher.EnableRaisingEvents = false;
        InkFileWatcher.Dispose();
        InkFileWatcher = null;
        
        _compileInkButton.Dispose();
        _inkFileItems.Clear();
        
        Editor.ContentDatabase.ItemAdded -= ContentDatabaseOnItemAdded;
        Editor.ContentDatabase.ItemRemoved -= ContentDatabaseOnItemRemoved;

        base.DeinitializeEditor();
    }
}