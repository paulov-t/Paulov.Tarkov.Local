using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Paulov.Bepinex.Framework;

namespace Paulov.Tarkov.Local;

[BepInDependency("Paulov.Tarkov.Minimal", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("Paulov.Bepinex.Framework", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin("Paulov.Tarkov.Local", "Paulov.Tarkov.Local", "2025.02.02")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static Type[] EftTypes
    {
        get;
        private set;
    }

    public static Type TarkovApplicationType
    {
        get
        {
            return EftTypes.First(x => x.Name == "TarkovApplication");
        }
    }

    public static Type RaidSettingsType
    {
        get
        {
            return EftTypes.First(x
                =>
                x.GetMembers().Any(y => y.Name == "SelectedGameModes")
                && x.GetMembers().Any(y => y.Name == "KeyId")
                && x.GetMembers().Any(y => y.Name == "LocationId")
                && x.GetMembers().Any(y => y.Name == "TimeAndWeatherSettings")
            );
        }
    }

    public static object BackEndSession
    {
        get
        {
            var isessionType = EftTypes
                .Where(x => x.IsInterface)
                .FirstOrDefault(x =>
                    ReflectionHelpers.GetMethodForType(x, "GetPhpSessionId") != null
                    && ReflectionHelpers.GetMethodForType(x, "SetMainProfile") != null
                );

            var clientAppType = ReflectionHelpers.GetGenericWithArgs(typeof(ClientApplication<>), new Type[] { isessionType });
            var singletonInstance = ReflectionHelpers.GetSingletonInstance(clientAppType);
            return ReflectionHelpers.GetMethodForType(singletonInstance.GetType(), "GetClientBackEndSession").Invoke(singletonInstance, null);
        }
    }

    public static ISession BackEndSession2
    {
        get
        {
            return Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession();
        }
    }

    private void Awake()
    {
        Logger = base.Logger;

        Logger.LogInfo(ReflectionHelpers.GetBaseDirectory());

        EftTypes = typeof(AbstractGame).Assembly.GetTypes().OrderBy(t => t.Name).ToArray();

        HarmonyPatchManager hpm2 = new("Paulov's Main Harmony Manager", new LocalPatchProvider());
        hpm2.EnableAll();
    }
}
