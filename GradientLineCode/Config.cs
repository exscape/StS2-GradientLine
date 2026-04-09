using System.Reflection;
using BaseLib.Config;
using Godot;

namespace GradientLine.GradientLineCode;

[HoverTipsByDefault]
public class Config : SimpleModConfig
{
    public static GradientUtil.GradientType GradientType { get; set; } = GradientUtil.GradientType.Rainbow;
    public static bool Animate { get; set; } = true;
    public static bool RandomizeStartOffset { get; set; } = true;
    [SliderRange(30, 200, 10)]
    public static double AnimateSpeed { get; set; } = 120f;

    [ConfigSection("Custom color section")]
    [SliderRange(1,5)]
    [ConfigVisibleIf(nameof(GradientType), GradientUtil.GradientType.Custom)]
    public static int NumCustomColors  { get; set; } = 5;

    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor1 { get; set; } = "#0000FF";

    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor2 { get; set; } = "#00FF00";

    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor3 { get; set; } = "#FFFF00";

    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor4 { get; set; } = "#FF0000";

    [ConfigVisibleIf(nameof(ShouldShowCustomColorRow))]
    [ConfigColorPicker(EditAlpha = false)]
    public static string CustomColor5 { get; set; } = "#FFFFFF";

    // Lazy, but easy: no need to touch the rest of the code
    [ConfigHideInUI]
    public static string CustomColors { get; set; } = string.Concat(CustomColor1, CustomColor2, CustomColor3, CustomColor4, CustomColor5);

    [ConfigSection("Random color section")]
    [ConfigVisibleIf(nameof(GradientType), GradientUtil.GradientType.Random)]
    [SliderRange(2, 10)]
    public static double RandomGradientSize { get; set; } = 5;
    [ConfigVisibleIf(nameof(GradientType), GradientUtil.GradientType.Random)]
    public static bool RandomizeEachLine { get; set; } = false;
    
    [ConfigVisibleIf(nameof(GradientType), GradientUtil.GradientType.Random)]
    [SliderRange(0.1, 1, 0.1)]
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
    
    private static Gradient? _savedRandomGradient;
    public static Gradient? GetSavedRandomGradient() => _savedRandomGradient;
    public static void SetSavedRandomGradient(Gradient? gradient) => _savedRandomGradient = gradient;
    
    public override void SetupConfigUI(Control optionContainer)
    {
        ClearUIEventHandlers();
        
        _wasRandomizeEnabled = RandomizeStartOffset;
        _lastGradientType = GradientType;
        _lastCustomColors = CustomColors;
        _lastRandomGradientSize = RandomGradientSize;
        _previewHueOffset = RandomizeStartOffset ? GD.Randf() : 0f;
        _lastRandomness = Randomness;
        
        GenerateOptionsForAllProperties(optionContainer);
        _gradientPreview = AddGradientPreview(optionContainer);
        AddRestoreDefaultsButton(optionContainer);
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
        gradientPreview.CustomMinimumSize = new Vector2(120, 16);
        
        // Save the gradient as a static object for if they use the random option
        _savedRandomGradient = GradientUtil.BuildGradient(GradientType, _previewHueOffset);
        gradientPreview.SetGradient(_savedRandomGradient);
        gradientPreview.SetAnimate(Animate);
        
        EventHandler gradientUpdateHandler = (sender, args) =>
        {
            if (!GodotObject.IsInstanceValid(gradientPreview) || !gradientPreview.IsInsideTree())
                return;

            var colorList = new List<string>(5) { CustomColor1 };
            if (NumCustomColors >= 2) colorList.Add(CustomColor2);
            if (NumCustomColors >= 3) colorList.Add(CustomColor3);
            if (NumCustomColors >= 4) colorList.Add(CustomColor4);
            if (NumCustomColors >= 5) colorList.Add(CustomColor5);
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

            if (shouldRebuildGradient)
            {
                _savedRandomGradient = GradientUtil.BuildGradient(GradientType, _previewHueOffset);
                gradientPreview.SetGradient(_savedRandomGradient);
            }
            
            gradientPreview.SetAnimate(Animate);
        };
        
        ConfigChanged += gradientUpdateHandler;
        
        // Track this handler so it gets cleaned up
        _configChangedHandlers.Add(gradientUpdateHandler);
        
        optionContainer.AddChild(CreateSectionHeader("Preview"));
        optionContainer.AddChild(gradientPreview);
        
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

    private void ClearUIEventHandlers()
    {
        foreach (var handler in _configChangedHandlers)
            ConfigChanged -= handler;
        
        _configChangedHandlers.Clear();
    }

}