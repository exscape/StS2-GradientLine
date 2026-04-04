using Godot;
using GodotPlugins.Game;
using GradientLine.GradientLineCode.Networking;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace GradientLine.GradientLineCode;

public class GradientLinePatches
{
    [HarmonyPatch(typeof(NMapDrawings), "CreateLineForPlayer")]
    public static class SetGradientPatch
    {
        [HarmonyPostfix]
        static void Postfix(NMapDrawings __instance, Player player, bool isErasing, ref Line2D __result)
        {
            if (__result == null || isErasing) return;
            
            ulong playerId = player.NetId;
            
            // If we don't want to randomize the starting offset or are using random gradient type, this returns 0f
            float startingHue =
                (Config.RandomizeStartOffset && Config.GradientType != GradientUtil.GradientType.Random)
                    ? GD.Randf()
                    : 0f;            
            
            if (MultiplayerManager.IsLocalPlayer(playerId))
            {
                if (Config.GradientType == GradientUtil.GradientType.Random)
                    __result.Gradient = Config.GetSavedRandomGradient();
                
                else
                     __result.Gradient = GradientUtil.BuildGradient(Config.GradientType, startingHue);
                
                MultiplayerManager.BroadcastLineStart(startingHue);
            }
            else
            {
                float remoteHue = MultiplayerManager.GetCurrentLineHue(playerId);
                GradientUtil.GradientType gradientType = MultiplayerManager.GetPlayerGradientType(playerId);

                if (gradientType == GradientUtil.GradientType.Random)
                {
                    __result.Gradient = MultiplayerManager.GetPlayerGradient(playerId);
                }
                else
                {
                    __result.Gradient = GradientUtil.BuildGradient(gradientType, remoteHue);
                }
            }
        }
    }

    [HarmonyPatch(typeof(NMapDrawings), "UpdateCurrentLinePosition")]
    public static class UpdateGradientPatch
    {
        [HarmonyPostfix]
        static void Postfix(NMapDrawings __instance, object state, Vector2 position)
        {
            if (!Config.Animate)
                return;
            
            ulong netId = Traverse.Create(state).Field("playerId").GetValue<ulong>();
            var line = Traverse.Create(state).Field("currentlyDrawingLine").GetValue<Line2D>();
            if (!GodotObject.IsInstanceValid(line) || line.Gradient == null) return;
            
            float currentLineHue = MultiplayerManager.GetCurrentLineHue(netId);
            float hueOffset = currentLineHue + (float)(line.GetPointCount() * Config.AnimateSpeed / 5000.0) % 1f;
            
            if (MultiplayerManager.IsLocalPlayer(netId))
            {
                line.Gradient = GradientUtil.BuildGradient(Config.GradientType, hueOffset, Config.GetSavedRandomGradient());
            }
            else
            {
                GradientUtil.GradientType gradientType = MultiplayerManager.GetPlayerGradientType(netId);
                if (gradientType == GradientUtil.GradientType.Random)
                {
                    line.Gradient = GradientUtil.BuildKeyframeFromGradientColors(MultiplayerManager.GetPlayerGradient(netId), hueOffset);
                }
                else
                {
                    line.Gradient = GradientUtil.BuildGradient(gradientType, hueOffset);
                }
            }
        }
    }
}