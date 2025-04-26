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
    public class ScavModeProfilePatch : NullPaulovHarmonyPatch
    {
        public static Type BackendProfileInterfaceType { get; private set; }


        public override MethodBase GetMethodToPatch()
        {
            Plugin.Logger.LogDebug($"{nameof(ScavModeProfilePatch)}.GetMethodToPatch");

            var desiredType =
                Plugin
                .EftTypes
                .First(x => x.Name == "TarkovApplication")
                .GetNestedTypes(BindingFlags.Public)
                .Single(x => x.GetField("timeAndWeather") != null
                                   && x.GetField("gameWorld") != null
                                   && x.GetField("metricsConfig") != null
                                   && x.Name.Contains("Struct"));

            var desiredMethod = AccessTools.Method(desiredType, "MoveNext");

            BackendProfileInterfaceType = Plugin.EftTypes.Single(x => x.GetMethods().Length == 2 && x.GetMethods().Select(y => y.Name).Contains("get_Profile") && x.IsInterface);

            return desiredMethod;
        }


        public override HarmonyMethod GetTranspilerMethod()
        {
            return new HarmonyMethod(this.GetType().GetMethod(nameof(TranspilerMethod), BindingFlags.Public | BindingFlags.Static));
        }

        public static IEnumerable<CodeInstruction> TranspilerMethod(System.Reflection.Emit.ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Search for code where backend.Session.getProfile() is called.
            var searchCode = new CodeInstruction(System.Reflection.Emit.OpCodes.Callvirt, AccessTools.Method(BackendProfileInterfaceType, "get_Profile"));
            var searchIndex = -1;

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
                {
                    searchIndex = i;
                    break;
                }
            }


            // Move back by 2. This is the start of this method call.
            searchIndex -= 2;

            var brFalseLabel = generator.DefineLabel();
            var brLabel = generator.DefineLabel();

            List<HarmonyLib.CodeInstruction> newCodes = new();
            //var newCodes = CodeGenerator.GenerateInstructions(new List<Code>()
            //{
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(System.Reflection.Emit.OpCodes.Ldloc_1)));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Call, typeof(ClientApplication<ISession>), "get_Session")));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Ldloc_1)));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Ldfld, Plugin.TarkovApplicationType, "_raidSettings")));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Callvirt, Plugin.RaidSettingsType, "get_IsPmc")));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Brfalse, brFalseLabel)));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Callvirt, BackendProfileInterfaceType, "get_Profile")));
            newCodes.Add(HarmonyPatchManager.ParseCode(new Code(OpCodes.Br, brLabel)));
            newCodes.Add(HarmonyPatchManager.ParseCode(new CodeWithLabel(OpCodes.Callvirt, brFalseLabel, BackendProfileInterfaceType, "get_ProfileOfPet")));
            newCodes.Add(HarmonyPatchManager.ParseCode(new CodeWithLabel(OpCodes.Stfld, brLabel, Plugin.TarkovApplicationType.GetNestedTypes(BindingFlags.Public).Single(IsTargetNestedType), "profile")));
            //});

            codes.RemoveRange(searchIndex, 4);
            codes.InsertRange(searchIndex, newCodes.AsEnumerable());

            return codes.AsEnumerable();
        }

        private static bool IsTargetNestedType(Type nestedType)
        {
            return nestedType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Any() &&
                   nestedType.GetFields().Length == 5 &&
                   nestedType.GetField("savageProfile") != null &&
                   nestedType.GetField("profile") != null;
        }
    }
}
