using EFT;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Paulov.Bepinex.Framework;

namespace Paulov.Tarkov.Local.Patches
{
    internal class ScavModeLoadBundlesPatch : NullPaulovHarmonyPatch
    {
        public static Type BackendProfileInterfaceType { get; private set; }

        public override IEnumerable<MethodBase> GetMethodsToPatch()
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

            yield return method;
        }

        public override HarmonyMethod GetTranspilerMethod()
        {
            return new HarmonyMethod(this.GetType().GetMethod(nameof(TranspilerMethod), BindingFlags.Public | BindingFlags.Static));
        }

        //TODO: There is massive code duplication between this and the ScavModeProfilePatch. Look into how to reduce this.
        public static IEnumerable<CodeInstruction> TranspilerMethod(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator).Start();

            //Generate labels
            Label brFalseLabel = generator.DefineLabel();
            Label brLabel = generator.DefineLabel();

            //Match start of get_Profile call and remove relevant instructions.
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(BackendProfileInterfaceType, "get_Profile"))
            ).ThrowIfInvalid("Could not find get_Session or surrounding OpCodes")
                .Advance(1).RemoveInstructions(3);
            //We match ldloc.1 and skip past it so we can reuse it.

            //Generate new instructions to replace the original get_Profile call.
            IEnumerable<CodeInstruction> codesToAdd =
            [
                TranspilerHelper.ParseCode(new Code(OpCodes.Call, "get_Session", typeof(ClientApplication<ISession>))),
                TranspilerHelper.ParseCode(new Code(OpCodes.Ldloc_1)),
                TranspilerHelper.ParseCode(new Code(OpCodes.Ldfld, "_raidSettings", Plugin.TarkovApplicationType)),
                TranspilerHelper.ParseCode(new Code(OpCodes.Callvirt, "get_IsPmc", Plugin.RaidSettingsType)),
                TranspilerHelper.ParseCode(new Code(OpCodes.Brfalse, brFalseLabel)),
                TranspilerHelper.ParseCode(new Code(OpCodes.Callvirt, "get_Profile", BackendProfileInterfaceType)),
                TranspilerHelper.ParseCode(new Code(OpCodes.Br, brLabel)),
                TranspilerHelper.ParseCode(new Code(OpCodes.Callvirt, "get_ProfileOfPet", BackendProfileInterfaceType, label: brFalseLabel)),
                TranspilerHelper.ParseCode(new Code(OpCodes.Ldc_I4_1, label: brLabel))
            ];

            //Insert new instructions and return
            matcher.InsertAndAdvance(codesToAdd);
            return matcher.InstructionEnumeration();
        }
    }
}
