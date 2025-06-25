using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using EFT;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;

namespace Paulov.Tarkov.Local.Patches.Performance;

public class RemoveStopwatchAllocationsPatch : NullPaulovHarmonyPatch
{
    public override IEnumerable<MethodBase> GetMethodsToPatch()
    {
        //CoverPointPatch
        yield return AccessTools.Method(typeof(CoverPointMaster), nameof(CoverPointMaster.method_0));
        
        //BotOwnerPatch
        yield return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.UpdateManual));
    }

    private static IEnumerable<CodeInstruction> TranspilerMethod(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatcher matcher = new CodeMatcher(instructions).Start();
        
        //Find stopwatch creation and start instructions.
        Type stopwatchType = typeof(Stopwatch);
        CodeMatch[] stopwatchCreationInstructions =
        [
            new(OpCodes.Newobj, AccessTools.Constructor(stopwatchType, Type.EmptyTypes)),
            new(OpCodes.Dup),
            new(OpCodes.Callvirt, AccessTools.Method(stopwatchType, nameof(Stopwatch.Start)))
        ];
        matcher.MatchForward(false, stopwatchCreationInstructions)
            .ThrowIfInvalid("Could not find stopwatch creation and start instructions.");
        
        //Move any labels to the next valid instruction.
        matcher.Instruction.MoveLabelsTo(matcher.InstructionAt(stopwatchCreationInstructions.Length));
        
        //Remove stopwatch creation and start instructions.
        matcher.RemoveInstructions(stopwatchCreationInstructions.Length);
        
        //Find stopwatch stop instruction.
        matcher.MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(stopwatchType, nameof(Stopwatch.Stop))))
            .ThrowIfInvalid("Could not find stopwatch stop instruction.");
        
        //Move any labels to the next valid instruction.
        //Note: This is not necessary but future-proofs in case BSG updates any of these and a branch gets added
        matcher.Instruction.MoveLabelsTo(matcher.InstructionAt(1));
        matcher.RemoveInstruction();
        
        return matcher.InstructionEnumeration();
    }
}