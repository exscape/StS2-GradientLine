using System.Reflection;
using BaseLib.Config;
using Godot;
using MegaCrit.Sts2.Core.Localization;

namespace GradientLine.GradientLineCode;

[ConfigHoverTipsByDefault]
public class Config : SimpleModConfig
{
    public static GradientUtil.GradientType GradientType { get; set; } = GradientUtil.GradientType.Rainbow;
    public static bool Animate { get; set; } = true;
    public static bool RandomizeStartOffset { get; set; } = true;
    [ConfigSlider(30, 200, 10)]
    public static double AnimateSpeed { get; set; } = 120f;

    [ConfigSection("Custom color section")]
    [ConfigSlider(1,8)]
    [ConfigVisibleIf(nameof(GradientType), GradientUtil.GradientType.Custom)]
    public static int NumCustomColors  { get; set; } = 5;

    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor1 { get; set; } = "#0000FF"; // Blue

    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor2 { get; set; } = "#00FF00"; // Lime

    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor3 { get; set; } = "#FFFF00"; // Yellow

    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor4 { get; set; } = "#FF0000"; // Red

    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor5 { get; set; } = "#FFFFFF"; // White
    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor6 { get; set; } = "#FFA500"; // Orange
    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor7 { get; set; } = "#800080"; // Purple
    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor8 { get; set; } = "#000000"; // Black

    // Lazy, but easy: no need to touch the rest of the code
    [ConfigHideInUI]
    public static string CustomColors { get; set; } = string.Concat(CustomColor1, CustomColor2, CustomColor3, CustomColor4, CustomColor5);

    [ConfigSection("Random color section")]
    [ConfigVisibleIf(nameof(GradientType), GradientUtil.GradientType.Random)]
    [ConfigSlider(2, 10)]
    public static double RandomGradientSize { get; set; } = 5;
    [ConfigVisibleIf(nameof(GradientType), GradientUtil.GradientType.Random)]
    public static bool RandomizeEachLine { get; set; } = false;
    
    [ConfigVisibleIf(nameof(GradientType), GradientUtil.GradientType.Random)]
    [ConfigSlider(0.1, 1, 0.1)]
    public static double Randomness { get; set; } = 1f;

    [ConfigVisibleIf(nameof(GradientType), GradientUtil.GradientType.Random)]
    [ConfigButton("RerollRandom")]
    public void RerollRandom()
    {
        _savedRandomGradient = GradientUtil.BuildGradient(GradientType, _previewHueOffset);
        _gradientPreview.SetGradient(_savedRandomGradient);
    }
    
    private readonly List<EventHandler> _configChangedHandlers = [];
    private static float _previewHueOffset;
    private bool _wasRandomizeEnabled;
    private double _lastRandomGradientSize;
    private GradientUtil.GradientType _lastGradientType;
    private GradientPreviewControl _gradientPreview;
    private string _lastCustomColors;
    private double _lastRandomness;

    private MarginContainer _previewHeader;
    
    private static Gradient? _savedRandomGradient;
    public static Gradient? GetSavedRandomGradient() => _savedRandomGradient;
    public static void SetSavedRandomGradient(Gradient? gradient) => _savedRandomGradient = gradient;
    
    public override void SetupConfigUI(Control optionContainer)
    {
        ClearUIEventHandlers();
        _gradientPreview = AddGradientPreview(optionContainer);

        base.SetupConfigUI(optionContainer);

        _wasRandomizeEnabled = RandomizeStartOffset;
        _lastGradientType = GradientType;
        _lastCustomColors = CustomColors;
        _lastRandomGradientSize = RandomGradientSize;
        _previewHueOffset = RandomizeStartOffset ? GD.Randf() : 0f;
        _lastRandomness = Randomness;
        
        SetupFocusNeighbors(optionContainer);
    }

    private static bool ShouldShowCustomColorRow(PropertyInfo prop)
    {
        var numStr = prop.Name.Replace("CustomColor", "");
        if (!int.TryParse(numStr, out var num)) return true;
        return GradientType == GradientUtil.GradientType.Custom && NumCustomColors >= num;
    }

    private GradientPreviewControl AddGradientPreview(Control optionContainer)
    {
        GradientPreviewControl gradientPreview = new GradientPreviewControl();
        _previewHeader = new MarginContainer();
        gradientPreview.CustomMinimumSize = new Vector2(120, 16);
        bool shouldShowGradient = GradientType != GradientUtil.GradientType.None;

        if (shouldShowGradient)
        {
            // Save the gradient as a static object for if they use the random option
            _savedRandomGradient = GradientUtil.BuildGradient(GradientType, _previewHueOffset);
            gradientPreview.SetGradient(_savedRandomGradient);
            gradientPreview.SetAnimate(Animate);
        }

        EventHandler gradientUpdateHandler = (sender, args) =>
        {
            if (!GodotObject.IsInstanceValid(gradientPreview) || !gradientPreview.IsInsideTree())
                return;

            var colorList = new List<string>(5) { CustomColor1 };
            if (NumCustomColors >= 2) colorList.Add(CustomColor2);
            if (NumCustomColors >= 3) colorList.Add(CustomColor3);
            if (NumCustomColors >= 4) colorList.Add(CustomColor4);
            if (NumCustomColors >= 5) colorList.Add(CustomColor5);
            if (NumCustomColors >= 6) colorList.Add(CustomColor6);
            if (NumCustomColors >= 7) colorList.Add(CustomColor7);
            if (NumCustomColors >= 8) colorList.Add(CustomColor8);

            CustomColors = string.Join(string.Empty, colorList);

            bool gradientChanged = GradientType != _lastGradientType;
            bool randomSizeChanged = RandomGradientSize != _lastRandomGradientSize;
            bool customColorsChanged = CustomColors != _lastCustomColors;
            bool randomnessChanged =  Randomness != _lastRandomness;
            bool shouldRebuildGradient = randomnessChanged || customColorsChanged || gradientChanged || randomSizeChanged || _wasRandomizeEnabled != RandomizeStartOffset;

            UpdatePreviewOffset(gradientChanged);

            // Update tracking state
            _wasRandomizeEnabled = RandomizeStartOffset;
            _lastGradientType = GradientType;
            _lastRandomGradientSize = RandomGradientSize;
            _lastCustomColors = CustomColors;
            _lastRandomness = Randomness;

            if (GradientType == GradientUtil.GradientType.None)
            {
                gradientPreview.Visible = false;
                _previewHeader.Visible = false;
            }
            else if (shouldRebuildGradient)
            {
                gradientPreview.Visible = true;
                _previewHeader.Visible = true;
                _savedRandomGradient = GradientUtil.BuildGradient(GradientType, _previewHueOffset);
                gradientPreview.SetGradient(_savedRandomGradient);
            }
            
            gradientPreview.SetAnimate(Animate);
        };
        
        ConfigChanged += gradientUpdateHandler;
        
        // Track this handler so it gets cleaned up
        _configChangedHandlers.Add(gradientUpdateHandler);
        
        // Preview text kinda looks weird when at the top
        // optionContainer.AddChild(CreateSectionHeader(new LocString("settings_ui", "PREVIEW.title").GetFormattedText()));
        optionContainer.AddChild(gradientPreview);
        optionContainer.AddChild(CreateBlankSpace()); // Makes a gap between the preview and next thing
        
        return gradientPreview;
    }

    private void UpdatePreviewOffset(bool gradientChanged)
    {
        bool randomizeJustEnabled = RandomizeStartOffset && !_wasRandomizeEnabled;
        bool randomizeJustDisabled = !RandomizeStartOffset && _wasRandomizeEnabled;
        
        if (randomizeJustDisabled)
        {
            _previewHueOffset = 0f;
        }
        else if (RandomizeStartOffset && (randomizeJustEnabled || gradientChanged))
        {
            _previewHueOffset = GD.Randf();
        }
    }

    public static float GetPreviewHueOffset()
    {
        return _previewHueOffset;
    }
    
    private MarginContainer CreateBlankSpace()
    {
        var header = new MarginContainer();
        header.AddThemeConstantOverride("margin_left", 24);
        header.AddThemeConstantOverride("margin_right", 24);

        // Just a spacer label with the same height as a normal header
        var spacer = new Label
        {
            CustomMinimumSize = new Vector2(0, 64),
            Text = " " // a single space so it renders height
        };

        header.AddChild(spacer);
        return header;
    }

    private void ClearUIEventHandlers()
    {
        foreach (var handler in _configChangedHandlers)
            ConfigChanged -= handler;
        
        _configChangedHandlers.Clear();
    }

}