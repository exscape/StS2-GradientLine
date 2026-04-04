# Map Line Gradients

A mod for **Slay the Spire 2** that replaces the plain-colored freehand map drawing lines with animated color gradients.

## Features

- **Built-in gradient presets**
- **Custom palette** - define your own gradient using up to 10 hex color codes
- **Live preview** of the selected gradient in the mod settings UI
- **Multiplayer support** - each player's gradient preferences are synced across clients

## Requirements

- Slay the Spire 2 (Early Access)
- [BaseLib](https://github.com/alchyr/BaseLib) mod installed

## Installation

1. Install BaseLib if you haven't already.
2. Copy the `GradientLine/` folder (containing `GradientLine.dll`, `GradientLine.pck`, and `GradientLine.json`) into your StS2 mods directory.

## Configuration

All settings are available in-game through the mod settings menu:

| Setting | Default | Description |
|---|---|---|
| Gradient | Rainbow | Which gradient preset to use |
| Animate the color while drawing | On | Whether the gradient shifts as you draw |
| Animation Speed | 120 | Controls how fast the hue shifts (higher = slower); range 30–200 |
| Randomize Start Offset | On | Whether each line starts with a random hue |
| Custom Gradient Colors | _(empty)_ | Up to 10 hex color codes, each prefixed with `#` (e.g. `#FF0000#00FF00#0000FF`) |

A live gradient preview strip updates in real time as you change settings.

## Building from Source

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Godot 4.5.1 (the version bundled with StS2 / "Megadot") - needed only for exporting the `.pck`
- Slay the Spire 2 installed via Steam

The project auto-detects the Steam library location on Windows, Linux, and macOS. If it can't find the game, the build will error with a helpful message.

### Build (DLL only)

```sh
dotnet build
```

This compiles the mod and copies `GradientLine.dll`, `GradientLine.json`, and required BaseLib files directly into the StS2 mods directory.

### Publish (DLL + PCK)

```sh
dotnet publish
```

This additionally exports the Godot `.pck` asset bundle using Godot in headless mode.


## Contributing New Gradient Presets

Adding a new built-in preset requires changes to two files:

### 1. Add the preset to `GradientType` enum in [GradientUtil.cs](GradientLineCode/GradientUtil.cs)

```csharp
public enum GradientType : ushort
{
    Rainbow,
    Fire,
    // ...
    YourPreset, // add here (before Custom)
    Random,  
    Custom
}
```

### 2. Define color keyframes in [GradientsPresets.cs](GradientLineCode/GradientsPresets.cs)

Making the last color identical to the first is recommended because a mismatch between the end and start colors creates a visible hard

### 3. Wire it up in `BuildGradient`

Add a case to the switch expression:

```csharp
public static Gradient BuildGradient(float hueOffset)
{
    return Config.GradientType switch
    {
        // ...
        GradientType.YourPreset => BuildKeyframeGradient(hueOffset, YourPresetColors),
        // ...
    };
}
```

## TODO
- Color picker for custom colors
- Hide custom color row when not selecting custom color
- Make erasing better
- Allow gradient to repeat
- Random gradient for each individual line setting
