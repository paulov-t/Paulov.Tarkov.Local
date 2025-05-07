using System;
using System.Collections.Generic;
using System.Linq;
using Paulov.Bepinex.Framework;

namespace Paulov.Tarkov.Local;

public class LocalPatchProvider : IPatchProvider
{
    public IEnumerable<IPaulovHarmonyPatch> GetPatches()
    {
        IOrderedEnumerable<Type> assemblyTypes = GetType().Assembly.GetTypes().OrderBy(x => x.Name);
        foreach (Type type in assemblyTypes)
        {
            if(type.GetInterface(nameof(IPaulovHarmonyPatch)) is null) continue;
            yield return (IPaulovHarmonyPatch)Activator.CreateInstance(type);
        }
    }
}