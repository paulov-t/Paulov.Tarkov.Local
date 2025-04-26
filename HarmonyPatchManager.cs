using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Paulov.Tarkov.Local;

public class HarmonyPatchManager
{
    private readonly ManualLogSource logger;
    private List<Harmony> harmonyList;

    public HarmonyPatchManager(in string managerName)
    {
        harmonyList = new List<Harmony>();
        logger = BepInEx.Logging.Logger.CreateLogSource(managerName ?? GetType().Name);
    }

    public void EnablePatches()
    {
        foreach (var patch in this.GetType().Assembly.GetTypes()
           .Where(x => x.GetInterface(nameof(IPaulovHarmonyPatch)) != null)
           .OrderBy(t => t.Name).ToArray())
        {
            try
            {
                var harmony = new Harmony(patch.Name);
                var obj = Activator.CreateInstance(patch) as IPaulovHarmonyPatch;
                if (obj == null || obj.GetMethodToPatch() == null)
                    continue;

                harmony.Patch(obj.GetMethodToPatch(), obj.GetPrefixMethod(), obj.GetPostfixMethod(), obj.GetTranspilerMethod(), obj.GetFinalizerMethod(), obj.GetILManipulatorMethod());
                harmonyList.Add(harmony);
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
        }
    }

    public void DisablePatches()
    {
        foreach (var harmony in harmonyList)
            harmony.UnpatchSelf();
    }

    public static CodeInstruction ParseCode(Code code)
    {
        if (!code.HasOperand)
        {
            return new CodeInstruction(code.OpCode) { labels = GetLabelList(code) };
        }

        if (code.OpCode == OpCodes.Ldfld || code.OpCode == OpCodes.Stfld)
        {
            return new CodeInstruction(code.OpCode, AccessTools.Field(code.CallerType, code.OperandTarget as string)) { labels = GetLabelList(code) };
        }

        if (code.OpCode == OpCodes.Call || code.OpCode == OpCodes.Callvirt)
        {
            return new CodeInstruction(code.OpCode, AccessTools.Method(code.CallerType, code.OperandTarget as string, code.Parameters)) { labels = GetLabelList(code) };
        }

        if (code.OpCode == OpCodes.Box)
        {
            return new CodeInstruction(code.OpCode, code.CallerType) { labels = GetLabelList(code) };
        }

        if (code.OpCode == OpCodes.Br || code.OpCode == OpCodes.Brfalse || code.OpCode == OpCodes.Brtrue || code.OpCode == OpCodes.Brtrue_S
            || code.OpCode == OpCodes.Brfalse_S || code.OpCode == OpCodes.Br_S)
        {
            return new CodeInstruction(code.OpCode, code.OperandTarget) { labels = GetLabelList(code) };
        }

        throw new ArgumentException($"Code with OpCode {nameof(code.OpCode)} is not supported.");
    }

    private static List<Label> GetLabelList(Code code)
    {
        if (code.GetLabel() == null)
        {
            return new List<Label>();
        }

        return new List<Label>() { (Label)code.GetLabel() };
    }

}

public class Code
{
    public OpCode OpCode { get; }
    public Type CallerType { get; }
    public object OperandTarget { get; }
    public Type[] Parameters { get; }
    public bool HasOperand { get; }

    public Code(OpCode opCode)
    {
        OpCode = opCode;
        HasOperand = false;
    }

    public Code(OpCode opCode, object operandTarget)
    {
        OpCode = opCode;
        OperandTarget = operandTarget;
        HasOperand = true;
    }

    public Code(OpCode opCode, Type callerType)
    {
        OpCode = opCode;
        CallerType = callerType;
        HasOperand = true;
    }

    public Code(OpCode opCode, Type callerType, object operandTarget, Type[] parameters = null)
    {
        OpCode = opCode;
        CallerType = callerType;
        OperandTarget = operandTarget;
        Parameters = parameters;
        HasOperand = true;
    }

    public virtual Label? GetLabel()
    {
        return null;
    }
}


public class CodeWithLabel : Code
{
    public Label Label { get; }

    public CodeWithLabel(OpCode opCode, Label label) : base(opCode)
    {
        Label = label;
    }

    public CodeWithLabel(OpCode opCode, Label label, object operandTarget) : base(opCode, operandTarget)
    {
        Label = label;
    }

    public CodeWithLabel(OpCode opCode, Label label, Type callerType) : base(opCode, callerType)
    {
        Label = label;
    }

    public CodeWithLabel(OpCode opCode, Label label, Type callerType, object operandTarget, Type[] parameters = null) : base(opCode, callerType, operandTarget, parameters)
    {
        Label = label;
    }

    public override Label? GetLabel()
    {
        return Label;
    }
}