using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;

namespace Paulov.Tarkov.Local.Patches.Performance;

public class RemoveStopwatchAllocationCoverPoint : NullPaulovHarmonyPatch
{
    public override IEnumerable<MethodBase> GetMethodsToPatch()
    {
        yield return Plugin.EftTypes.Single(x => x.Name == "CoverPointMaster").GetMethod("method_0", BindingFlags.Public | BindingFlags.Instance);
    }

    public override HarmonyMethod GetTranspilerMethod()
    {
        return new HarmonyMethod(GetType().GetMethod(nameof(TranspilerMethod), BindingFlags.NonPublic | BindingFlags.Static));
    }

    private static IEnumerable<CodeInstruction> TranspilerMethod(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatcher matcher = new CodeMatcher(instructions).Start();

        //Find and remove stopwatch creation and start instructions.
        Type stopwatchType = typeof(System.Diagnostics.Stopwatch);
        matcher.MatchForward(false,
                new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(stopwatchType, Type.EmptyTypes)),
                new CodeMatch(OpCodes.Dup),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(stopwatchType, "Start")))
            .ThrowIfInvalid("Could not find stopwatch creation and start.");
        
        matcher.Instruction.MoveLabelsTo(matcher.InstructionAt(3));
        matcher.RemoveInstructions(3);
        
        //Find and remove stopwatch stop instruction.
        matcher.MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(stopwatchType, "Stop")))
            .ThrowIfInvalid("Could not find stopwatch stop.")
            .Instruction.MoveLabelsTo(matcher.InstructionAt(1));
        matcher.RemoveInstruction();
        
        return matcher.InstructionEnumeration();
    }
}