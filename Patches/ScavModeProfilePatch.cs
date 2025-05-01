using EFT;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

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

        public static IEnumerable<CodeInstruction> TranspilerMethod(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            CodeMatcher matcher = new CodeMatcher(codes, generator).Start();

            //Generate labels
            Label brFalseLabel = generator.DefineLabel();
            Label brLabel = generator.DefineLabel();
            
            //Match start of get_Profile call and remove relevant instructions.
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(BackendProfileInterfaceType, "get_Profile")),
                new CodeMatch(OpCodes.Stfld)
            ).ThrowIfInvalid("Could not find get_Session or surrounding OpCodes")
                .Advance(1).RemoveInstructions(3);
            //We match ldloc.1 and skip past it so we can reuse it.
            
            //Generate new instructions to replace the original get_Profile call.
            IEnumerable<CodeInstruction> codesToAdd =
            [
                HarmonyPatchManager.ParseCode(new Code(OpCodes.Call, typeof(ClientApplication<ISession>), "get_Session")),
                HarmonyPatchManager.ParseCode(new Code(OpCodes.Ldloc_1)),
                HarmonyPatchManager.ParseCode(new Code(OpCodes.Ldfld, Plugin.TarkovApplicationType, "_raidSettings")),
                HarmonyPatchManager.ParseCode(new Code(OpCodes.Callvirt, Plugin.RaidSettingsType, "get_IsPmc")),
                HarmonyPatchManager.ParseCode(new Code(OpCodes.Brfalse, brFalseLabel)),
                HarmonyPatchManager.ParseCode(new Code(OpCodes.Callvirt, BackendProfileInterfaceType, "get_Profile")),
                HarmonyPatchManager.ParseCode(new Code(OpCodes.Br, brLabel)),
                HarmonyPatchManager.ParseCode(new CodeWithLabel(OpCodes.Callvirt, brFalseLabel, BackendProfileInterfaceType, "get_ProfileOfPet")),
                HarmonyPatchManager.ParseCode(new CodeWithLabel(OpCodes.Stfld, brLabel, Plugin.TarkovApplicationType.GetNestedTypes(BindingFlags.Public).Single(IsTargetNestedType), "profile"))
            ];
            
            //Insert new instructions and return
            matcher.InsertAndAdvance(codesToAdd);
            return matcher.InstructionEnumeration();
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
