using Comfort.Common;
using EFT;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;
using System.Collections.Generic;
using System.Reflection;

namespace Paulov.Tarkov.Local.Patches.Bots
{
    /// <summary>
    /// This patch fixes AI PMC and Scavs not targeting Zombies and Zombies not targeting them!
    /// </summary>
    public class BotZombieModeFreeForAllPatch : NullPaulovHarmonyPatch
    {
        public BotZombieModeFreeForAllPatch()
        {
        }

        public override MethodBase GetMethodToPatch()
        {
            Plugin.Logger.LogDebug($"{nameof(BotZombieModeFreeForAllPatch)}.GetMethodToPatch");

            var method =
                typeof(BotSpawner)
                .GetMethod("GetGroupAndSetEnemies", BindingFlags.Public | BindingFlags.Instance);

            Plugin.Logger.LogDebug($"{nameof(BotZombieModeFreeForAllPatch)}.GetMethodToPatch:{method.DeclaringType}.{method}");

            return method;
        }

        public static void PrefixMethod(BotsGroup __instance, BotOwner bot, BotZone zone, ref bool ____freeForAll)
        {
            var backendConfig = Singleton<BackendConfigSettingsClass>.Instance;
            var infectionConfig = backendConfig.SeasonActivityConfig.InfectionHalloweenConfig;
            if (infectionConfig.Enabled)
            {
                ____freeForAll = true;
            }
        }

        public static void PostfixMethod(BotsGroup __instance, ref bool ____freeForAll, ref BotsGroup __result, ref List<EFT.Player> ____allPlayers)
        {
            var backendConfig = Singleton<BackendConfigSettingsClass>.Instance;
            var infectionConfig = backendConfig.SeasonActivityConfig.InfectionHalloweenConfig;
            if (infectionConfig.Enabled)
            {
                ____freeForAll = false;
            }
        }


    }
}
