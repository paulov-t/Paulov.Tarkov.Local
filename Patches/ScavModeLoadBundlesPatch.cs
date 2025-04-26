using EFT;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Paulov.Tarkov.Local.Patches
{
    internal class ScavModeLoadBundlesPatch : NullPaulovHarmonyPatch
    {
        public static Type BackendProfileInterfaceType { get; private set; }

        public override MethodBase GetMethodToPatch()
        {
            Plugin.Logger.LogDebug($"{nameof(ScavModeLoadBundlesPatch)}.GetMethodToPatch");

            BackendProfileInterfaceType = Plugin.EftTypes.Single(x => x.GetMethods().Length == 2 && x.GetMethods().Select(y => y.Name).Contains("get_Profile") && x.IsInterface);
            var desiredType = typeof(TarkovApplication)
                    .GetNestedTypes(BindingFlags.Public | BindingFlags.Instance)
                    .Single(x => x.GetField("timeAndWeather") != null
                                       && x.GetField("tarkovApplication_0") != null
                                       && x.GetField("inTransition") != null
                                       && x.Name.Contains("Struct"));

            var method = desiredType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == "MoveNext");

            return method;
        }

        public override HarmonyMethod GetTranspilerMethod()
        {
            return new HarmonyMethod(this.GetType().GetMethod(nameof(TranspilerMethod), BindingFlags.Public | BindingFlags.Static));
        }

        public static IEnumerable<CodeInstruction> TranspilerMethod(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            var searchCode = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(BackendProfileInterfaceType, "get_Profile"));
            var searchIndex = -1;

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
                {
                    searchIndex = i;
                    break;
                }
            }

            if (searchIndex == -1)
            {
                return instructions;
            }

            searchIndex -= 2;

            var brFalseLabel = generator.DefineLabel();
            var brLabel = generator.DefineLabel();

            List<HarmonyLib.CodeInstruction> newCodes = new();
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Ldloc_1)));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Call, typeof(ClientApplication<ISession>), "get_Session")));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Ldloc_1)));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Ldfld, typeof(TarkovApplication), "_raidSettings")));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Callvirt, typeof(RaidSettings), "get_IsPmc")));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Brfalse, brFalseLabel)));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Callvirt, BackendProfileInterfaceType, "get_Profile")));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Br, brLabel)));
            newCodes.Add(HarmonyPatchManager.ParseCode(new CodeWithLabel(OpCodes.Callvirt, brFalseLabel, BackendProfileInterfaceType, "get_ProfileOfPet")));
            newCodes.Add(HarmonyPatchManager.ParseCode(new CodeWithLabel(OpCodes.Ldc_I4_1, brLabel)));

            codes.RemoveRange(searchIndex, 4);
            codes.InsertRange(searchIndex, newCodes);

            return codes.AsEnumerable();
        }
    }
}
