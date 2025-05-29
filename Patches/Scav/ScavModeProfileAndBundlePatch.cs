using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using EFT;
using HarmonyLib;
using Paulov.Bepinex.Framework;
using Paulov.Bepinex.Framework.Patches;

namespace Paulov.Tarkov.Local.Patches.Scav;

public class ScavModeProfileAndBundlePatch : NullPaulovHarmonyPatch
{
    private static readonly Lazy<Type> BackendProfileInterfaceTypeLazy = new(() =>
        Plugin.EftTypes.Single(x =>
            x.GetMethods().Length == 2 && x.GetMethods().Any(y => y.Name == "get_Profile") && x.IsInterface));
    private static Type BackendProfileInterfaceType => BackendProfileInterfaceTypeLazy.Value;

    private static readonly string[] NestedTarkovAppProfileStructFields =
    [
        "timeAndWeather",
        "gameWorld",
        "metricsConfig"
    ];

    private static readonly string[] NestedTarkokAppBundleStructFields =
    [
        "timeAndWeather",
        "tarkovApplication_0",
        "inTransition"
    ];
        
    public override IEnumerable<MethodBase> GetMethodsToPatch()
    {
        Plugin.Logger.LogDebug($"{nameof(ScavModeProfileAndBundlePatch)}.GetMethodsToPatch");

        //Base for both methods
        Type tarkovApplicationType = Plugin.EftTypes.Single(x => x.Name == "TarkovApplication");

        //Avoid double enumerating
        Type[] tarkovApplicationNestedStructs = tarkovApplicationType.GetNestedTypes(BindingFlags.Public)
            .Where(x => x.IsValueType && !x.IsPrimitive && !x.IsEnum).ToArray();
        
        //ScavModeProfilePatch
        Type scavModeProfilePatchDesiredType = tarkovApplicationNestedStructs
            .Single(x => NestedTarkovAppProfileStructFields.All(y => x.GetField(y) != null));

        MethodInfo scavModeProfilePatchDesiredMethod =
            AccessTools.Method(scavModeProfilePatchDesiredType, "MoveNext");

        yield return scavModeProfilePatchDesiredMethod;
        
        //ScavModeBundlePatch
        Type scavModeBundlePatchDesiredType = tarkovApplicationNestedStructs
            .Single(x => NestedTarkokAppBundleStructFields.All(y => x.GetField(y) != null));
        
        MethodInfo scavModeBundlePatchDesiredMethod =
            AccessTools.Method(scavModeBundlePatchDesiredType, "MoveNext");
        
        yield return scavModeBundlePatchDesiredMethod;
    }


    public override HarmonyMethod GetTranspilerMethod()
    {
        return new HarmonyMethod(GetType().GetMethod(nameof(TranspilerMethod), BindingFlags.Public | BindingFlags.Static));
    }

    public static IEnumerable<CodeInstruction> TranspilerMethod(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
    {
        CodeMatcher matcher = new CodeMatcher(instructions, generator).Start();
        
        //Match instructions leading up to the get_Profile call.
        matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(BackendProfileInterfaceType, "get_Profile"))
            ).ThrowIfInvalid("Could not find get_Session or preceding OpCodes")
            .RemoveInstruction();
        //Keep the get_session call on the stack for re-use.
        
        //Generate branch labels
        Label brFalseLabel = generator.DefineLabel();
        Label brLabel = generator.DefineLabel();
        
        //Generate new instructions to replace the original get_Profile call.
        IEnumerable<CodeInstruction> codesToAdd =
        [
            TranspilerHelper.ParseCode(new Code(OpCodes.Ldloc_1)),
            TranspilerHelper.ParseCode(new Code(OpCodes.Ldfld, "_raidSettings", Plugin.TarkovApplicationType)),
            TranspilerHelper.ParseCode(new Code(OpCodes.Callvirt, "get_IsPmc", Plugin.RaidSettingsType)),
            TranspilerHelper.ParseCode(new Code(OpCodes.Brfalse, brFalseLabel)),
            TranspilerHelper.ParseCode(new Code(OpCodes.Callvirt, "get_Profile", BackendProfileInterfaceType)),
            TranspilerHelper.ParseCode(new Code(OpCodes.Br, brLabel)),
            TranspilerHelper.ParseCode(new Code(OpCodes.Callvirt, "get_ProfileOfPet", BackendProfileInterfaceType, label: brFalseLabel)),
        ];
        
        //Insert alternate profile instructions.
        matcher.InsertAndAdvance(codesToAdd);
        
        //Assign our branch label to the next instruction.
        matcher.Instruction.labels.Add(brLabel);
        
        return matcher.InstructionEnumeration();
    }
}