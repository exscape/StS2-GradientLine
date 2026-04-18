using BaseLib.Utils;
using Godot;
using GradientLine.GradientLineCode.Networking;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace GradientLine.GradientLineCode;

public class GradientLinePatches
{
    private const double AnimationDivisor = 5000.0;

    [HarmonyPatch(typeof(NMapDrawings), "CreateLineForPlayer")]
    public static class Agogagaba
    {
        [HarmonyPostfix]
        static void Postfix(NMapDrawings __instance, Player player, bool isErasing, ref Line2D __result)
        {
            if (__result == null || isErasing) return;
            
            ulong playerId = player.NetId;
            float startingHue = CalculateStartingHue();
            
            if (MultiplayerManager.IsLocalPlayer(playerId))
            {
                HandleLocalPlayerLine(__result, startingHue);
            }
            else
            {
                HandleRemotePlayerLine(__result, playerId);
            }
        }

        private static float CalculateStartingHue()
        {
            // If we don't want to randomize the starting offset or are using random gradient type, this returns 0f
            bool shouldRandomize = Config.RandomizeStartOffset 
                                && Config.GradientType != GradientUtil.GradientType.Random;
            return shouldRandomize ? GD.Randf() : 0f;
        }

        private static void HandleLocalPlayerLine(Line2D line, float startingHue)
        {
            if (Config.GradientType != GradientUtil.GradientType.None)
            {
                line.Gradient = GradientUtil.BuildGradient(
                    Config.GradientType,
                    startingHue,
                    Config.GetSavedRandomGradient(),
                    Config.RandomizeEachLine
                );

                bool isRandomWithReroll = Config.GradientType == GradientUtil.GradientType.Random
                                          && Config.RandomizeEachLine;

                if (isRandomWithReroll)
                {
                    Config.SetSavedRandomGradient(line.Gradient);
                    MultiplayerManager.BroadcastGradient();
                }
            }

            MultiplayerManager.BroadcastLineStart(startingHue);
        }

        private static void HandleRemotePlayerLine(Line2D line, ulong playerId)
        {
            float remoteHue = MultiplayerManager.GetCurrentLineHue(playerId);
            GradientUtil.GradientType gradientType = MultiplayerManager.GetPlayerGradientType(playerId);

            if(gradientType != GradientUtil.GradientType.None)
                line.Gradient = BuildRemoteGradient(gradientType, remoteHue, playerId);
        }

        private static Gradient BuildRemoteGradient(
            GradientUtil.GradientType gradientType, 
            float hueOffset, 
            ulong playerId)
        {
            if (gradientType == GradientUtil.GradientType.Random)
            {
                return MultiplayerManager.GetPlayerGradient(playerId);
            }
            
            return GradientUtil.BuildGradient(gradientType, hueOffset);
        }
    }

    [HarmonyPatch(typeof(NMapDrawings), "UpdateCurrentLinePosition")]
    public static class Updoop
    {
        [HarmonyPostfix]
        static void Postfix(NMapDrawings __instance, object state, Vector2 position)
        {
            if (!Config.Animate) return;
            
            var lineState = ExtractLineState(state);
            if (!lineState.IsValid) return;
            
            float animatedHueOffset = CalculateAnimatedHueOffset(
                lineState.PlayerId, 
                lineState.Line
            );
            
            if (MultiplayerManager.IsLocalPlayer(lineState.PlayerId))
            {
                UpdateLocalPlayerLine(lineState.Line, animatedHueOffset);
            }
            else
            {
                UpdateRemotePlayerLine(lineState.Line, lineState.PlayerId, animatedHueOffset);
            }
        }

        private static LineState ExtractLineState(object state)
        {
            var traverse = Traverse.Create(state);
            ulong playerId = traverse.Field("playerId").GetValue<ulong>();
            var line = traverse.Field("currentlyDrawingLine").GetValue<Line2D>();
            
            bool isValid = GodotObject.IsInstanceValid(line) && line.Gradient != null;
            
            return new LineState(playerId, line, isValid);
        }

        private static float CalculateAnimatedHueOffset(ulong playerId, Line2D line)
        {
            float currentLineHue = MultiplayerManager.GetCurrentLineHue(playerId);
            float animationProgress = (float)(line.GetPointCount() * Config.AnimateSpeed / AnimationDivisor);
            return (currentLineHue + animationProgress) % 1f;
        }

        private static void UpdateLocalPlayerLine(Line2D line, float hueOffset)
        {
            if (Config.GradientType == GradientUtil.GradientType.None)
                return;
            line.Gradient = GradientUtil.BuildGradient(
                Config.GradientType, 
                hueOffset, 
                Config.GetSavedRandomGradient()
            );
        }

        private static void UpdateRemotePlayerLine(Line2D line, ulong playerId, float hueOffset)
        {
            GradientUtil.GradientType gradientType = MultiplayerManager.GetPlayerGradientType(playerId);

            if (gradientType == GradientUtil.GradientType.None)
                return;
            if (gradientType == GradientUtil.GradientType.Random)
            {
                line.Gradient = GradientUtil.BuildKeyframeFromGradientColors(
                    MultiplayerManager.GetPlayerGradient(playerId), 
                    hueOffset
                );
            }
            else
            {
                line.Gradient = GradientUtil.BuildGradient(gradientType, hueOffset);
            }
        }

        private readonly struct LineState
        {
            public ulong PlayerId { get; }
            public Line2D Line { get; }
            public bool IsValid { get; }

            public LineState(ulong playerId, Line2D line, bool isValid)
            {
                PlayerId = playerId;
                Line = line;
                IsValid = isValid;
            }
        }
    }

    [HarmonyPatch(typeof(NMapDrawings), "StopDrawingLine")]
    public static class WhyAmIDoingItThisWay
    {
        [HarmonyPrefix]
        private static bool StopDrawingLinePatch(NMapDrawings __instance, object state)
        {
            var traverse = Traverse.Create(state);
            var finishedLine = traverse.Field("currentlyDrawingLine").GetValue<Line2D>();

            LineAnimator.LineElapsedTime[finishedLine] = 0f;
            LineAnimator.BaseLineGradients[finishedLine] = finishedLine.Gradient.Duplicate() as Gradient;

            return true;
        }
    }
}