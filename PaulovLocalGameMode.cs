using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Bots;
using EFT.InputSystem;
using EFT.InventoryLogic;
using EFT.UI;
using JsonType;
using System;
using System.Threading.Tasks;

namespace Paulov.Tarkov.Local
{
    public class PaulovLocalGameMode : EFT.LocalGame
    {
        public static ManualLogSource Logger { get; private set; }

        internal static PaulovLocalGameMode Create(
            InputTree inputTree
            , Profile profile
            , GameWorld gameWorld
            , GameDateTime backendDateTime
            , InsuranceCompanyClass insurance
            , MenuUI menuUI
            , CommonUI commonUI
            , PreloaderUI preloaderUI
            , GameUI gameUI
            , LocationSettingsClass.Location location
            , TimeAndWeatherSettings timeAndWeather
            , WavesSettings wavesSettings
            , EDateTime dateTime
            , Callback<ExitStatus, TimeSpan, MetricsClass> callback
            , float fixedDeltaTime
            , EUpdateQueue updateQueue
            , ISession backEndSession
            , TimeSpan sessionTime
            , object metricsEvents
            , GClass2474 metricsCollector
            , LocalRaidSettings raidSettings)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(PaulovLocalGameMode));
            Logger.LogInfo($"{nameof(PaulovLocalGameMode)}.{nameof(Create)}");

            PaulovLocalGameMode game =
                smethod_0<PaulovLocalGameMode>(inputTree, profile, gameWorld, backendDateTime, insurance, gameUI, location, timeAndWeather, wavesSettings, dateTime
                , callback, fixedDeltaTime, updateQueue, backEndSession, new TimeSpan?(sessionTime), metricsEvents as GClass2485, metricsCollector, raidSettings);

            return game;
        }

        public Task Run(BotControllerSettings botsSettings, string backendUrl, InventoryController inventoryController)
        {
            return base.method_4(botsSettings, backendUrl, inventoryController);
        }
    }
}
