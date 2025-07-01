/**
 * LICENSE 
 */

using Comfort.Common;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Paulov.Tarkov.Local.Patches.Bots
{
    /// <summary>
    /// This patch fixes the events but can be used to make alterations to bot behavior
    /// </summary>
    public sealed class BotCoreLoadPatch : NullPaulovHarmonyPatch
    {
        public BotCoreLoadPatch()
        {
        }

        private List<string> fieldNamesToDetectBotCoreType = new List<string>() { "ACTIVE_HALLOWEEN_ZOMBIES_EVENT", "GRENADE_PRECISION" };

        private List<string> methodNamesToDetectBotCoreLoadType = new List<string>() { "Load", "LoadCoreByString", "LoadDifficultyStringInternal" };

        public static Type TypeOfBotCore;

        public static Type TypeOfBotCoreLoad;

        public override MethodBase GetMethodToPatch()
        {
            Plugin.Logger.LogDebug($"{nameof(BotCoreLoadPatch)}.GetMethodToPatch");

            TypeOfBotCore =
               Plugin.EftTypes
               .First(x => x.GetFields(BindingFlags.Public | BindingFlags.Instance).Count(x => fieldNamesToDetectBotCoreType.Contains(x.Name)) == 2);

            Plugin.Logger.LogDebug($"{nameof(BotCoreLoadPatch)}.{nameof(TypeOfBotCore)}:{TypeOfBotCore.Name}");

            TypeOfBotCoreLoad =
                Plugin.EftTypes
                .First(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static).Count(x => methodNamesToDetectBotCoreLoadType.Contains(x.Name)) == 3);

            Plugin.Logger.LogDebug($"{nameof(BotCoreLoadPatch)}.{nameof(TypeOfBotCoreLoad)}:{TypeOfBotCoreLoad.Name}");

            var method = TypeOfBotCoreLoad.GetMethod("Load", BindingFlags.Public | BindingFlags.Static);

            Plugin.Logger.LogDebug($"{nameof(BotCoreLoadPatch)}.GetMethodToPatch:{method.DeclaringType}.{method}");

            return method;

        }

        public static void PostfixMethod()
        {
            // Globals
            var backendConfig = Singleton<BackendConfigSettingsClass>.Instance;

            FixZombieEvent(backendConfig);

        }

        /// <summary>
        /// Ensures only the Zombie Event is active when Enabled via Globals
        /// </summary>
        /// <param name="backendConfig"></param>
        private static void FixZombieEvent(BackendConfigSettingsClass backendConfig)
        {
            // Globals Infection Configuration
            var infectionConfig = backendConfig.SeasonActivityConfig.InfectionHalloweenConfig;
            if (infectionConfig.Enabled)
            {
                var core = TypeOfBotCoreLoad.GetFields(BindingFlags.Public | BindingFlags.Static).First(x => x.Name == "Core").GetValue(null);
                // Disable all other events
                core.GetType().GetField("ACTIVE_FOLLOW_PLAYER_EVENT", BindingFlags.Public | BindingFlags.Instance).SetValue(core, false);
                core.GetType().GetField("ACTIVE_FORCE_ATTACK_EVENTS", BindingFlags.Public | BindingFlags.Instance).SetValue(core, false);
                core.GetType().GetField("ACTIVE_FORCE_KHOROVOD_EVENTS", BindingFlags.Public | BindingFlags.Instance).SetValue(core, false);
                core.GetType().GetField("ACTIVE_PATROL_GENERATOR_EVENT", BindingFlags.Public | BindingFlags.Instance).SetValue(core, false);

                // Enable Zombies
                core.GetType().GetField("ACTIVE_HALLOWEEN_ZOMBIES_EVENT", BindingFlags.Public | BindingFlags.Instance).SetValue(core, true);
            }
        }
    }
}
