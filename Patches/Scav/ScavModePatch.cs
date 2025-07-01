using EFT;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;
using System.Collections.Generic;
using System.Reflection;

namespace Paulov.Tarkov.Local.Patches.Scav
{
    /// <summary>
    /// This patch simply allows you to click scav and press ready but doesn't fix the fact the game still loads as PMC
    /// </summary>
    public class ScavModePatch : NullPaulovHarmonyPatch
    {
        public override MethodBase GetMethodToPatch()
        {
            Plugin.Logger.LogDebug($"{nameof(ScavModePatch)}.GetMethodToPatch");

            return AccessTools.Method(typeof(MainMenuControllerClass), nameof(MainMenuControllerClass.method_24));
        }

        public static void PrefixOverrideMethod(ref RaidSettings ___raidSettings_0, ref RaidSettings ___raidSettings_1, MainMenuControllerClass __instance)
        {
            Plugin.Logger.LogDebug($"{nameof(ScavModePatch)}.{nameof(PrefixOverrideMethod)}");

            if (___raidSettings_0.Side == ESideType.Savage)
                ___raidSettings_0.RaidMode = ERaidMode.Local;

            ___raidSettings_0.WavesSettings = ___raidSettings_1.WavesSettings;
            ___raidSettings_0.BotSettings = ___raidSettings_1.BotSettings;
            ___raidSettings_1 = ___raidSettings_0.Clone();
        }
    }
}
