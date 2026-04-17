using Godot;
using GodotPlugins.Game;

namespace GradientLine.GradientLineCode;

public partial class GradientTimer : Node
{
    public override void _Process(double delta)
    {
        if (Config.Animate)
        {
            LineAnimator.Update(delta);
        }
    }
}

public static class LineAnimator
{
    public static Dictionary<Line2D, float> LineElapsedTime = new(); // Time since line was made
    public static Dictionary<Line2D, Gradient?> BaseLineGradients = new(); // Base gradients
    
    public static void Update(double delta)
    {
        foreach (var line in LineElapsedTime.Keys.ToList())
        {
            if (!GodotObject.IsInstanceValid(line))
            {
                LineElapsedTime.Remove(line);
                continue;
            }

            LineElapsedTime[line] += (float)delta;

            float hueOffset = (float)(LineElapsedTime[line] * Config.AnimateSpeed / 100);
            hueOffset %= 1f;
            
            var baseGradient = BaseLineGradients[line]; // Get the original line gradient
            var newGradient = GradientUtil.BuildKeyframeFromGradientColors(baseGradient, hueOffset); // Build the new gradient from that base
            var gradient = line.Gradient;

            // Update line gradient with calculated values
            gradient.Colors = newGradient.Colors;
            gradient.Offsets = newGradient.Offsets;
            gradient.EmitChanged();
        }
    }
    
}
