using System;
using System.Collections.Generic;
using FlaxEngine;
using Ink.Runtime;

namespace FlaxInk;

/// <summary>
/// FlaxInkUtils class.
/// </summary>
public static class FlaxInkUtils
{
        /// <summary>
        /// Create a story from an JsonAssetReference.
        /// </summary>
        /// <param name="inkStoryReference">The ink story Json asset reference.</param>
        /// <returns>The story.</returns>
        public static Story CreateStory(JsonAssetReference<InkStory> inkStoryReference)
        {
            return new Story(inkStoryReference.Instance.Story);
        }
        
        /// <summary>
        /// Create a story from an InkStory.
        /// </summary>
        /// <param name="inkStoryReference">The ink story.</param>
        /// <returns>The story.</returns>
        public static Story CreateStory(InkStory inkStoryReference)
        {
            return new Story(inkStoryReference.Story);
        }
        
        /// <summary>
        /// Create a story from a json string.
        /// </summary>
        /// <param name="jsonStory">The json story.</param>
        /// <returns>The story.</returns>
        public static Story CreateStory(string jsonStory)
        {
            return new Story(jsonStory);
        }
        
        /// <summary>
        /// Create a story from a JsonAsset.
        /// </summary>
        /// <param name="inkStoryReference">The JsonAsset.</param>
        /// <returns>The story or null if JsonAsset does not contain InkStory.</returns>
        public static Story CreateStory(JsonAsset inkStoryReference)
        {
            if (inkStoryReference != null && inkStoryReference.Instance is InkStory inkStory)
                return new Story(inkStory.Story);
    
            return null;
        }
}
