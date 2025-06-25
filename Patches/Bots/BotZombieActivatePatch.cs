using Comfort.Common;
using EFT;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;
using System.Collections.Generic;
using System.Reflection;

namespace Paulov.Tarkov.Local.Patches.Bots
{
    /// <summary>
    /// This patch fixes an issue that causes the BotHalloweenWithZombies to not have a reference to the game's Bot Spawner
    /// </summary>
    public class BotZombieActivatePatch : NullPaulovHarmonyPatch
    {
        public BotZombieActivatePatch()
        {
        }

        public override MethodBase GetMethodToPatch()
        {
            Plugin.Logger.LogDebug($"{nameof(BotZombieActivatePatch)}.GetMethodToPatch");

            var method =
                typeof(BotHalloweenWithZombies)
                .GetMethod("Activate", BindingFlags.Public | BindingFlags.Instance);

            Plugin.Logger.LogDebug($"{nameof(BotZombieActivatePatch)}.GetMethodToPatch:{method.DeclaringType}.{method}");

            return method;
        }

        public static bool PrefixMethod(BotHalloweenWithZombies __instance, ref BotSpawner ____spawner)
        {
            var backendConfig = Singleton<BackendConfigSettingsClass>.Instance;
            var infectionConfig = backendConfig.SeasonActivityConfig.InfectionHalloweenConfig;
            if (infectionConfig.Enabled)
            {
                if (____spawner == null)
                {
                    Plugin.Logger.LogDebug($"{nameof(BotZombieActivatePatch)}.PrefixMethod. Spawner is NULL!");

                    var game = Singleton<AbstractGame>.Instance;
                    IBotGame ibotGame = game as IBotGame;

                    Plugin.Logger.LogDebug($"{nameof(BotZombieActivatePatch)}.PrefixMethod. Game type is {game.GetType()}");

                    if (ibotGame.BotsController != null)
                    {
                        Plugin.Logger.LogDebug($"{nameof(BotZombieActivatePatch)}.PrefixMethod. botsController found!");

                        ____spawner = ibotGame.BotsController.GetType().GetField("_botSpawner", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ibotGame.BotsController) as BotSpawner;

                        Plugin.Logger.LogDebug($"{nameof(BotZombieActivatePatch)}.PrefixMethod. Assigned spawner to BotsController spawner");

                        return true;
                    }
                }
            }

            return false;
        }


    }
}
