# Ink for Flax Engine
This integrates [Ink](https://www.inklestudios.com/ink/) into the Flax Game Engine.

Enjoy this plugin and want to donate? Here is a link to donate on [ko-fi](https://ko-fi.com/tryibion).

## Installation
To add this plugin project to your game, follow the instructions in the [Flax Engine documentation](https://docs.flaxengine.com/manual/scripting/plugins/plugin-project.html#automated-git-cloning) for adding a plugin project automatically using git or manually.

## Setup
1. Install the plugin into your project.
2. It is recommended to use the Inky editor for editing Ink files.
3. Create Ink files somewhere in the "Content" folder by right clicking and selecting "Ink->Ink File". This will generate a new Ink file.
4. If Inky is installed and .ink files are registered in the operating system to open ink files, then you can double click on the Ink file in the editor and it will open Inky.
5. Save your Ink file and a `Ink Json` file will be generated. This file is the compiled Ink story and is what should be used in the game.
6. Add the provided `DialogueRunner` script into a scene.
7. Create a another script to interact with the `DialogueRunner`. Call methods and subscribe to events on the `DialogueRunner`.
8. Call `DialogueRunner.StartDialogue()` and pass in the `Ink Json` that contains the compiled Ink story.
9. Follow tutorials online for Ink's story structure to create great experiences to share!
