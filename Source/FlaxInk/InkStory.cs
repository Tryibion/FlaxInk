using System;
using System.Collections.Generic;
using FlaxEngine;

namespace FlaxInk;

/// <summary>
/// InkStory class.
/// </summary>
public class InkStory
{
    [Serialize]
    private string _story;
    
    /// <summary>
    /// The ink story stored as JSON.
    /// </summary>
    [HideInEditor]
    public string Story => _story;

    public InkStory()
    {
        
    }

    public InkStory(string jsonStory)
    {
        _story = jsonStory;
    }
}
