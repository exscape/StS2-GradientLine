# Map Line Gradients

A mod for **Slay the Spire 2** that replaces the plain-colored freehand map drawing lines with animated color gradients.

## Features

- **built-in gradient presets**
- **Custom palette** - define your own gradient using up to 10 hex color codes
- **Live preview** of the selected gradient in the mod settings UI

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

## How It Works

[GradientLinePatches.cs](GradientLineCode/GradientLinePatches.cs) applies two Harmony postfix patches to the game's `NMapDrawings` class:

- **Line creation** - assigns a gradient with a random starting hue to the new `Line2D` node.
- **Line update** - if animation is enabled, recalculates the gradient on every new point added so the color crawls forward as the line grows.

[GradientUtil.cs](GradientLineCode/GradientUtil.cs) handles gradient construction.

## Contributing New Gradient Presets

Adding a new built-in preset requires changes to three places in [GradientUtil.cs](GradientLineCode/GradientUtil.cs):

### 1. Add the preset to the `GradientType` enum

```csharp
public enum GradientType
{
    Rainbow,
    Fire,
    // ...
    YourPreset,  // add here (before Custom)
    Custom
}
```

### 2. Define the color keyframes

Making the last color identical to the first is recommended because a mismatch between the end and start colors creates a visible hard jump as the hue cycles.

```csharp
static readonly Color[] YourPresetColors =
[
    new Color(r, g, b),
    new Color(r, g, b),
    // ...
    new Color(r, g, b)  // repeat first color to loop
];
```

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

### 4. Add a localization string

Add a display name for the preset in [GradientLine/localization/eng/settings_ui.json](GradientLine/localization/eng/settings_ui.json):

```json
"GRADIENTLINE-GRADIENT_TYPE-YOUR_PRESET.title": "Your Preset Name"
```

The key format is `GRADIENTLINE-GRADIENT_TYPE-<ENUM_NAME_UPPERCASE>.title`.

