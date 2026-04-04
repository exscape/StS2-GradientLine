using Godot;

namespace GradientLine.GradientLineCode;

public partial class GradientPreviewControl : Control
{
    private Gradient? _gradient;
    private float _thickness = 12f;
    private GradientTexture2D _cachedTexture;
    private float _animationOffset = 0f;
    private bool _shouldAnimate = false;

    public override void _Process(double delta)
    {
        if (_shouldAnimate && _gradient != null)
        {
            _animationOffset += (float)(delta * Config.AnimateSpeed / 100.0);
            _animationOffset %= 1f;
        
            var newGradient = GradientUtil.BuildKeyframeFromGradientColors(_gradient, _animationOffset);
            UpdateCachedTexture(newGradient);
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (_gradient == null)
            return;

        DrawTextureRect(_cachedTexture, new Rect2(Vector2.Zero, Size), false);
    }

    public void SetGradient(Gradient g)
    {
        _gradient = g;
        UpdateCachedTexture(g);
        QueueRedraw();
    }

    public void SetAnimate(bool animate)
    {
        _shouldAnimate = animate;
        SetProcess(animate);
    }

    private void UpdateCachedTexture(Gradient g)
    {
        _cachedTexture?.Dispose();
        _cachedTexture = new GradientTexture2D
        {
            Gradient = g,
            Width = 256,
            Height = (int)_thickness
        };
    }
}