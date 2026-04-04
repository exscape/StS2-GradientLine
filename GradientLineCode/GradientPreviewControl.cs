using Godot;

namespace GradientLine.GradientLineCode;

public partial class GradientPreviewControl : Control
{
    private Gradient? _gradient;
    private float _thickness = 12f;
    private GradientTexture2D _cachedTexture;

    public override void _Draw()
    {
        if (_gradient == null)
            return;

        DrawTextureRect(_cachedTexture, new Rect2(Vector2.Zero, Size), false);
    }

    public void SetGradient(Gradient g)
    {
        _gradient = g;

        _cachedTexture?.Dispose();
        _cachedTexture = new GradientTexture2D
        {
            Gradient = g,
            Width = 256,
            Height = (int)_thickness
        };

        QueueRedraw();
    }
}