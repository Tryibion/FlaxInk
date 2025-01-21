using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEngine;
using Ink;
using Ink.Runtime;
using PluginManager = FlaxEngine.PluginManager;

namespace FlaxInk;

public struct DialogueLine
{
    public string Text;
    public List<string> Tags;
}

public struct DialogueChoice
{
    public int Index;
    public string Text;
    public List<string> Tags;
}

/// <summary>
/// DialogueRunner Script.
/// </summary>
public class DialogueRunner : Script
{
    private Story _activeStory;
    private bool _isPaused = false;
    private DialogueLine? _currentLine;
    private List<DialogueChoice> _currentChoices = new List<DialogueChoice>();
    
    /// <summary>
    /// Event fired when the dialogue is started.
    /// </summary>
    public event Action DialogueStarted;
    
    /// <summary>
    /// Event fired when the dialogue is restarted.
    /// </summary>
    public event Action DialogueRestarted;
    
    /// <summary>
    /// Event fired when the dialogue is stopped.
    /// </summary>
    public event Action DialogueStopped;
    
    /// <summary>
    /// Event fired when the dialogue is paused.
    /// </summary>
    public event Action DialoguePaused;
    
    /// <summary>
    /// Event fired when the dialogue is resumed.
    /// </summary>
    public event Action DialogueResumed;
    
    /// <summary>
    /// Event fired when there are no more choices or lines to continue to.
    /// </summary>
    public event Action DialogueEnded;
    
    /// <summary>
    /// Event called when a new dialogue line exists.
    /// </summary>
    public event Action<DialogueLine> NewDialogueLine;
    
    /// <summary>
    /// Event called when a new choice is present.
    /// </summary>
    public event Action<List<DialogueChoice>> NewDialogueChoices;
    
    /// <summary>
    /// Whether the story is paused.
    /// </summary>
    public bool IsPaused => _isPaused;
    
    /// <summary>
    /// Whether there is an active story present.
    /// </summary>
    public bool IsStoryActive => _activeStory != null;

    public DialogueLine? CurrentLine => _currentLine;
    
    public List<DialogueChoice> CurrentChoices => _currentChoices;
    
    /// <inheritdoc/>
    public override void OnStart()
    {

    }

    public override void OnDestroy()
    {
        _activeStory = null;
        _isPaused = false;
        DialogueStarted = null;
        DialogueRestarted = null;
        DialogueStopped = null;
        NewDialogueLine = null;
        NewDialogueChoices = null;
        DialoguePaused = null;
        DialogueResumed = null;
        base.OnDestroy();
    }
 
    /// <summary>
    /// Start the dialogue.
    /// </summary>
    /// <param name="inkStory">The ink story in Flax JSON format to start.</param>
    /// <param name="autoStart">Whether to call Continue dialogue immediately.</param>
    public void StartDialogue(JsonAssetReference<InkStory> inkStory, bool autoStart = false)
    {
        var story = FlaxInkUtils.CreateStory(inkStory);
        StartDialogue(story, autoStart);
    }
    
    /// <summary>
    /// Start the dialogue.
    /// </summary>
    /// <param name="jsonStory">The story in JSON format to start.</param>
    /// <param name="autoStart">Whether to call Continue dialogue immediately.</param>
    public void StartDialogue(string jsonStory, bool autoStart = false)
    {
        var story = FlaxInkUtils.CreateStory(jsonStory);
        StartDialogue(story, autoStart);
    }

    /// <summary>
    /// Start the dialogue.
    /// </summary>
    /// <param name="story">The story to start.</param>
    /// <param name="autoStart">Whether to call Continue dialogue immediately.</param>
    public void StartDialogue(Story story, bool autoStart = false)
    {
        if (_activeStory)
        {
            Debug.LogWarning("DialogueRunner already has an active story. Please call StopDialogue() first.");
            return;
        }
        if (story == null)
            return;
        _activeStory = story;
        _currentLine = null;
        _currentChoices.Clear();
        _isPaused = false;
        _activeStory.onError += (message, type) =>
        {
            if (type == ErrorType.Warning)
                Debug.LogWarning(message);
            else
                Debug.LogError(message);
        };
        DialogueStarted?.Invoke();
        if (autoStart)
            ContinueDialogue();
    }

    /// <summary>
    /// Uses the same story, but resets all the data and starts at the beginning.
    /// </summary>
    public void ResetDialogue()
    {
        if (!_activeStory)
            return;
        _activeStory.ResetState();
        _isPaused = false;
        _currentLine = null;
        _currentChoices.Clear();
        DialogueRestarted?.Invoke();
    }

    /// <summary>
    /// Keeps existing story state and data but moves the dialogue back to the start.
    /// </summary>
    public void GoToStartOfStory()
    {
        if (!_activeStory)
            return;
        _currentLine = null;
        _currentChoices.Clear();
        _activeStory.state.GoToStart();
    }

    /// <summary>
    /// Pauses the dialogue and does not let it continue until resumed.
    /// </summary>
    public void PauseDialogue()
    {
        if (!_activeStory || _isPaused)
            return;
        _isPaused = true;
        DialoguePaused?.Invoke();
    }

    /// <summary>
    /// Resumes the dialogue if paused.
    /// </summary>
    public void ResumeDialogue()
    {
        if (!_activeStory || !_isPaused)
            return;
        _isPaused = false;
        DialogueResumed?.Invoke();
    }

    /// <summary>
    /// Stop the dialogue and clear the active story.
    /// </summary>
    public void StopDialogue()
    {
        if (!_activeStory)
            return;
        DialogueStopped?.Invoke();
        _isPaused = false;
        _activeStory = null;
        _currentLine = null;
        _currentChoices.Clear();
    }

    /// <summary>
    /// Call to continue the dialogue to the next line. Call this to move to the choices as well.
    /// </summary>
    public void ContinueDialogue()
    {
        if (_isPaused)
            return;

        if (!_activeStory)
        {
            Debug.LogWarning("DialogueRunner does not have an active story to continue.");
            return;
        }

        if (_activeStory.canContinue)
        {
            _activeStory.Continue();
            DialogueLine line = new DialogueLine
            {
                Text = _activeStory.currentText,
                Tags = _activeStory.currentTags,
            };
            _currentLine = line;
            _currentChoices.Clear();
            NewDialogueLine?.Invoke(line);
        }
        else if (_activeStory.currentChoices.Count > 0)
        {
            List<DialogueChoice> choices = new List<DialogueChoice>();
            foreach (var currentChoice in _activeStory.currentChoices)
            {
                DialogueChoice choice = new DialogueChoice
                {
                    Index = currentChoice.index,
                    Text = currentChoice.text,
                    Tags = currentChoice.tags,
                };
                choices.Add(choice);
            }
            _currentLine = null;
            _currentChoices = choices;
            NewDialogueChoices?.Invoke(choices);
        }
        else
        {
            _currentLine = null;
            _currentChoices.Clear();
            DialogueEnded?.Invoke();
        }
    }

    /// <summary>
    /// Call this to choose a choice.
    /// </summary>
    /// <param name="index">The choice index.</param>
    /// <param name="autoContinue">Whether to automatically call Continue once choice is chosen.</param>
    public void ChooseChoice(int index, bool autoContinue = false)
    {
        if (_isPaused)
            return;

        if (!_activeStory)
        {
            Debug.LogWarning("DialogueRunner does not have an active story to choose a choice.");
            return;
        }

        if (_activeStory.currentChoices.Count == 0)
        {
            Debug.LogWarning("DialogueRunner does not have any active choices to make.");
            return;
        }

        if (_activeStory.currentChoices.Count < index)
        {
            Debug.LogWarning($"DialogueRunner only has `{_activeStory.currentChoices.Count}` many choices and is zero based, index `{index}` is out of range.");
            return;
        }

        _activeStory.ChooseChoiceIndex(index);
        if (autoContinue)
            ContinueDialogue();
    }

    /// <summary>
    /// Saves the current state of the active story.
    /// </summary>
    /// <returns>Returns the state of the story in JSON format.</returns>
    public string GetActiveDialogueStateSave()
    {
        if (!_activeStory)
        {
            Debug.LogWarning("No active story found. Please call StartDialogue() first before trying to get dialogue save state.");
            return null;
        }

        return _activeStory.state.ToJson();
    }

    /// <summary>
    /// Loads a save state of the active story.
    /// </summary>
    /// <param name="jsonStoryState">The saved story state in JSON format.</param>
    public void LoadActiveDialogueStateSave(string jsonStoryState)
    {
        if (!_activeStory)
        {
            Debug.LogWarning("No active story found. Please call StartDialogue() first before trying to load dialogue save state.");
            return;
        }

        _activeStory.state.LoadJson(jsonStoryState);
    }

    public void SetVariable(string name, int value)
    {
        SetVariable(name, (object)value);
    }
    
    public void SetVariable(string name, float value)
    {
        SetVariable(name, (object)value);
    }
    
    public void SetVariable(string name, string value)
    {
        SetVariable(name, (object)value);
    }
    
    public void SetVariable(string name, bool value)
    {
        SetVariable(name, (object)value);
    }

    private void SetVariable(string name, object value)
    {
        if (!_activeStory)
            return;
        if (!_activeStory.variablesState.Contains(name))
        {
            Debug.LogWarning($"The active story does not contain the variable `{name}` to set.");
            return;
        }
        _activeStory.variablesState[name] = value;
    }

    public void GetVariable(string name, out int value)
    {
        GetVariable(name, out object val);
        if (val is int i)
        {
            value = i;
            return;
        }
        Debug.LogWarning("Value in GetVariable is not of type int.");
        value = 0;
    }
    
    public void GetVariable(string name, out float value)
    {
        GetVariable(name, out object val);
        if (val is float i)
        {
            value = i;
            return;
        }
        Debug.LogWarning("Value in GetVariable is not of type float.");
        value = 0;
    }
    
    public void GetVariable(string name, out string value)
    {
        GetVariable(name, out object val);
        if (val is string i)
        {
            value = i;
            return;
        }
        Debug.LogWarning("Value in GetVariable is not of type string.");
        value = null;
    }
    
    public void GetVariable(string name, out bool value)
    {
        GetVariable(name, out object val);
        if (val is bool i)
        {
            value = i;
            return;
        }
        Debug.LogWarning("Value in GetVariable is not of type bool.");
        value = false;
    }

    private void GetVariable(string name, out object value)
    {
        if (!_activeStory)
        {
            value = null;
            return;
        }

        if (!_activeStory.variablesState.Contains(name))
        {
            Debug.LogWarning($"The active story does not contain the variable `{name}` to get.");
            value = null;
            return;
        }

        value = _activeStory.variablesState[name];
    }

    public void BindExternalFunction(string functionName, Action function, bool lookAheadSafe = false)
    {
        if (!_activeStory)
            return;

        _activeStory.BindExternalFunction(functionName, function, lookAheadSafe);
    }
    
    public void BindExternalFunction<T>(string functionName, Action<T> function, bool lookAheadSafe = false)
    {
        if (!_activeStory)
            return;
        
        _activeStory.BindExternalFunction(functionName, function, lookAheadSafe);
    }
    
    public void BindExternalFunction<T1, T2>(string functionName, Action<T1, T2> function, bool lookAheadSafe = false)
    {
        if (!_activeStory)
            return;
        
        _activeStory.BindExternalFunction(functionName, function, lookAheadSafe);
    }
    
    public void BindExternalFunction<T1, T2, T3>(string functionName, Action<T1, T2, T3> function, bool lookAheadSafe = false)
    {
        if (!_activeStory)
            return;
        
        _activeStory.BindExternalFunction(functionName, function, lookAheadSafe);
    }
    
    public void BindExternalFunction<T1, T2, T3, T4>(string functionName, Action<T1, T2, T3, T4> function, bool lookAheadSafe = false)
    {
        if (!_activeStory)
            return;
        
        _activeStory.BindExternalFunction(functionName, function, lookAheadSafe);
    }
    
    public void BindExternalFunction(string functionName, Func<object> function, bool lookAheadSafe = false)
    {
        if (!_activeStory)
            return;
        
        _activeStory.BindExternalFunction(functionName, function, lookAheadSafe);
    }
    
    public void BindExternalFunction<T>(string functionName, Func<T, object> function, bool lookAheadSafe = false)
    {
        if (!_activeStory)
            return;
        
        _activeStory.BindExternalFunction(functionName, function, lookAheadSafe);
    }
    
    public void BindExternalFunction<T1, T2>(string functionName, Func<T1, T2, object> function, bool lookAheadSafe = false)
    {
        if (!_activeStory)
            return;
        
        _activeStory.BindExternalFunction(functionName, function, lookAheadSafe);
    }
    
    public void BindExternalFunction<T1, T2, T3>(string functionName, Func<T1, T2, T3, object> function, bool lookAheadSafe = false)
    {
        if (!_activeStory)
            return;
        
        _activeStory.BindExternalFunction(functionName, function, lookAheadSafe);
    }
    
    public void BindExternalFunction<T1, T2, T3, T4>(string functionName, Func<T1, T2, T3, T4, object> function, bool lookAheadSafe = false)
    {
        if (!_activeStory)
            return;
        
        _activeStory.BindExternalFunction(functionName, function, lookAheadSafe);
    }
    
    public void ObserveVariable(string variableName, Action<string, object> function)
    {
        if (!_activeStory)
            return;
        if (!_activeStory.variablesState.Contains(variableName))
        {
            Debug.LogWarning($"The Active story can not observer the variable `{variableName}` because it doesn't exist.");
            return;
        }

        _activeStory.ObserveVariable(variableName, (string varName, object newValue) => function(varName, newValue));
    }
    
    public void ObserveVariables(List<string> variableNames, Action<string, object> function)
    {
        if (!_activeStory)
            return;

        _activeStory.ObserveVariables(variableNames, (string varName, object newValue) => function(varName, newValue));
    }
}
