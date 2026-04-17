using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace GradientLine.GradientLineCode.Networking;

[HarmonyPatch]
public static class NetworkEventPatches
{
    [HarmonyPatch(typeof(NCharacterSelectScreen), "PlayerConnected")]
    [HarmonyPostfix]
    static void OnPlayerConnected(LobbyPlayer player)
    {
        if (player.id == MultiplayerManager.LocalPlayerId)
            return;
        
        MultiplayerManager.BroadcastGradientType();
        if (Config.GradientType == GradientUtil.GradientType.Random)
        {
            MultiplayerManager.BroadcastGradient();
        }
    }

    [HarmonyPatch(typeof(NMapScreen), "Initialize")]
    [HarmonyPostfix]
    static void OnMapScreenInitialize(RunState runState)
    {
        MultiplayerManager.BroadcastGradientType();
        if (Config.GradientType == GradientUtil.GradientType.Random)
        {
            MultiplayerManager.BroadcastGradient();
        }
    }


    [HarmonyPatch(typeof(StartRunLobby), MethodType.Constructor, typeof(GameMode), typeof(INetGameService),
        typeof(IStartRunLobbyListener), typeof(int))]
    [HarmonyPostfix]
    static void OnStartRunLobbyConstructed(StartRunLobby __instance)
    {
        MultiplayerManager.Initialize(__instance.NetService);
    }

    [HarmonyPatch(typeof(LoadRunLobby), MethodType.Constructor, typeof(INetGameService),
        typeof(ILoadRunLobbyListener), typeof(SerializableRun))]
    [HarmonyPostfix]
    static void OnLoadRunLobbyConstructed(LoadRunLobby __instance)
    {
        MultiplayerManager.Initialize(__instance.NetService);
    }
}