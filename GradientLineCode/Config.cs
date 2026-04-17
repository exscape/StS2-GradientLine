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
    [ConfigTextInput("^((#[0-9A-Fa-f]{6})){1,10}$", MaxLength = 70)]
    
    [ConfigVisibleWhen(nameof(GradientType), GradientUtil.GradientType.Custom)]
    public static string CustomColors { get; set; } = "";

    [ConfigVisibleWhen(nameof(GradientType), GradientUtil.GradientType.Random)]
    [SliderRange(2, 10)]
    public static double RandomGradientSize { get; set; } = 5;
    [ConfigVisibleWhen(nameof(GradientType), GradientUtil.GradientType.Random)]
    public static bool RandomizeEachLine { get; set; } = false;
    
    [ConfigVisibleWhen(nameof(GradientType), GradientUtil.GradientType.Random)]
    [SliderRange(0.1, 1, 0.1)]
    public static double Randomness { get; set; } = 1f;

    [ConfigVisibleWhen(nameof(GradientType), GradientUtil.GradientType.Random)]
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