using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;

namespace Paulov.Tarkov.Local.Patches.Performance;

public class RemoveStopwatchAllocationBotOwner : NullPaulovHarmonyPatch
{
    public override MethodBase GetMethodToPatch()
    {
        return Plugin.EftTypes.Where(x => x.GetInterfaces().Any(y => y.Name == "IPlayer"))
            .Single(x => x.Name == "BotOwner")
            .GetMethods().Single(x => x.Name == "UpdateManual");
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
            .ThrowIfInvalid("Could not find stopwatch creation and start.")
            .RemoveInstructions(3);
        
        //Find and remove stopwatch stop instruction.
        matcher.MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(stopwatchType, "Stop")))
            .ThrowIfInvalid("Could not find stopwatch stop.")
            .RemoveInstruction();
        
        return matcher.InstructionEnumeration();
    }
}