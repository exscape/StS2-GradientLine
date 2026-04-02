using Godot;

namespace GradientLine.GradientLineCode;

public partial class GradientPreviewControl : Control
{
    private Gradient? Gradient;
    private float _thickness = 12f;
    private GradientTexture2D _cachedTexture;

    public override void _Draw()
    {
        if (Gradient == null)
            return;

        DrawTextureRect(_cachedTexture, new Rect2(Vector2.Zero, Size), false);
    }

    public void SetGradient(Gradient g)
    {
        Gradient = g;

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