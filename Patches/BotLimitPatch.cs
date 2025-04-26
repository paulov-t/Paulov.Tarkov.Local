//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;

//namespace Paulov.Tarkov.Local.Patches;
//public class BotLimitPatch : NullHarmonyPatch
//{

//    private static Random _rand = new();
//    static BotLimitPatch()
//    {
//    }

//    public override MethodBase GetMethodToPatch()
//    {
//        Plugin.Logger.LogDebug($"{nameof(BotLimitPatch)}.GetMethodToPatch");
//        return AccessTools.Method(typeof(BotsPresets), nameof(BotsPresets.method_3));
//    }

//    public override HarmonyMethod GetPostfixMethod()
//    {
//        return new HarmonyMethod(this.GetType().GetMethod(nameof(PatchPostfix), BindingFlags.Public | BindingFlags.Static));
//    }

//    public static void PatchPostfix(List<WaveInfo> __result, List<WaveInfo> wavesProfiles, List<WaveInfo> delayed)
//    {
//        delayed?.Clear();
//        foreach (WaveInfo wave in __result)
//        {
//            if (wave.Role == EFT.WildSpawnType.pmcBEAR || wave.Role == EFT.WildSpawnType.pmcUSEC)
//                wave.Limit = _rand.Next(1, 5);
//            else if (wave.Role == EFT.WildSpawnType.marksman)
//                wave.Limit = 1;
//            else
//                wave.Limit = _rand.Next(0, 1);
//        }
//    }
//}