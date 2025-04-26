//using BepInEx.Logging;
//using Comfort.Common;
//using EFT;
//using EFT.InputSystem;
//using EFT.UI;
//using EFT.UI.Matchmaker;
//using HarmonyLib;
//using HarmonyLib.Tools;
//using System;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.CompilerServices;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace Paulov.Tarkov.Local.Patches
//{
//    /// <summary>
//    /// Created by: Paulov
//    /// Paulov: Overwrite and use our own CoopGame instance instead
//    /// </summary>
//    public sealed class TarkovApplicationLocalGameCreatorPatch : NullHarmonyPatch
//    {
//        static TarkovApplicationLocalGameCreatorPatch()
//        {

//        }

//        public override MethodBase GetMethodToPatch()
//        {
//            var m = ReflectionHelpers.GetAllMethodsForType(typeof(TarkovApplication)).Single(
//                 x =>
//             x.GetParameters().Length >= 2
//            && x.GetParameters()[1].ParameterType == typeof(TimeAndWeatherSettings)
//            && x.GetParameters()[2].Name == "timeHasComeScreenController"
//                );


//            Plugin.Logger.LogDebug($"{nameof(TarkovApplicationLocalGameCreatorPatch)}");
//            Plugin.Logger.LogDebug(m.Name);
//            return m;
//        }

//        public override HarmonyMethod GetPrefixMethod()
//        {
//            return new HarmonyMethod(this.GetType().GetMethod(nameof(Prefix), BindingFlags.Public | BindingFlags.Static));
//        }

//        public static bool Prefix(
//              Task __result,
//           TarkovApplication __instance,
//           GameWorld gameWorld,
//           TimeAndWeatherSettings timeAndWeather,
//           object timeHasComeScreenController,
//           object metricsEvents,
//           MetricsConfigClass metricsConfig,
//            RaidSettings ____raidSettings,
//            InputTree ____inputTree,
//            GameDateTime ____localGameDateTime,
//            float ____fixedDeltaTime,
//            string ____backendUrl,
//            LocalRaidSettings ___localRaidSettings_0

//            )
//        {
//            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

//            var session = __instance.GetClientBackEndSession();
//            if (session == null)
//                return true;


//            Profile profile = session.GetProfileBySide(____raidSettings.Side);

//            //PaulovLocalGameMode localGame = PaulovLocalGameMode.Create(
//            //var localGame = LocalGame.smethod_6(

//            //    ____inputTree
//            //    , profile
//            //    , gameWorld
//            //    , ____localGameDateTime
//            //    , session.InsuranceCompany
//            //    , MonoBehaviourSingleton<MenuUI>.Instance
//            //    , MonoBehaviourSingleton<GameUI>.Instance
//            //    , ____raidSettings.SelectedLocation
//            //    , timeAndWeather
//            //    , ____raidSettings.WavesSettings
//            //    , ____raidSettings.SelectedDateTime
//            //    , new Callback<ExitStatus, TimeSpan, MetricsClass>((r) =>
//            //    {
//            //        ReflectionHelpers.GetAllMethodsForObject(__instance).FirstOrDefault(
//            //            x =>
//            //            x.GetParameters().Length >= 5
//            //            && x.GetParameters()[0].ParameterType == typeof(string)
//            //            && x.GetParameters()[1].ParameterType == typeof(Profile)
//            //            && x.GetParameters()[2].ParameterType == typeof(LocationSettingsClass.Location)
//            //            && x.GetParameters()[3].ParameterType == typeof(Result<ExitStatus, TimeSpan, MetricsClass>)
//            //            ).Invoke(__instance, new object[] {
//            //                    session.Profile.Id, session.ProfileOfPet, ____raidSettings.SelectedLocation, r, timeHasComeScreenController });

//            //    })
//            //    , ____fixedDeltaTime
//            //    , EUpdateQueue.Update
//            //    , session
//            //    , TimeSpan.FromSeconds(60 * ____raidSettings.SelectedLocation.EscapeTimeLimit)
//            //    , metricsEvents as GClass2444
//            //    , new GClass2433(metricsConfig, __instance)
//            //    , ___localRaidSettings_0
//            //);


//            return true;
//            //            __result = Task.Run(async () =>
//            //            {


//            //                if (Singleton<NotificationManagerClass>.Instantiated)
//            //                    Singleton<NotificationManagerClass>.Instance.Deactivate();

//            //                var session = __instance.GetClientBackEndSession();
//            //                if (session == null)
//            //                    return;

//            //                if (____raidSettings == null)
//            //                {
//            //                    Plugin.Logger.LogError("RaidSettings is Null");
//            //                    throw new ArgumentNullException("RaidSettings");
//            //                }

//            //                if (timeHasComeScreenController == null)
//            //                {
//            //                    Plugin.Logger.LogError("timeHasComeScreenController is Null");
//            //                    throw new ArgumentNullException("timeHasComeScreenController");
//            //                }

//            //                // Get player profile by side
//            //                Profile profile = session.GetProfileBySide(____raidSettings.Side);

//            //                LocationSettingsClass.Location location = ____raidSettings.SelectedLocation;
//            //                profile.Inventory.Stash = null;
//            //                profile.Inventory.QuestStashItems = null;
//            //                profile.Inventory.DiscardLimits = Singleton<ItemFactory>.Instance.GetDiscardLimits();

//            //                if (!____raidSettings.isInTransition)
//            //                {
//            //                    await session.SendRaidSettings(____raidSettings);
//            //                }

//            //                ___localRaidSettings_0 = new LocalRaidSettings
//            //                {
//            //                    location = ____raidSettings.LocationId,
//            //                    timeVariant = ____raidSettings.SelectedDateTime,
//            //                    mode = ELocalMode.PVE_OFFLINE,
//            //                    playerSide = ____raidSettings.Side,
//            //                    serverId = ""// PaulovMatchmaking.GetGroupId()
//            //                };

//            //                Plugin.Logger.LogDebug($"{___localRaidSettings_0.ToJson()}");

//            //                try
//            //                {
//            //                    Plugin.Logger.LogDebug($"Attempt \"/client/match/local/start\" \"LocalRaidStarted\"");

//            //                    // Get local raid settings
//            //                    // "/client/match/local/start"
//            //                    var localSettings = await session.LocalRaidStarted(___localRaidSettings_0);
//            //                    if (___localRaidSettings_0 != null)
//            //                        ___localRaidSettings_0.selectedLocation = localSettings.locationLoot;
//            //                }
//            //                catch (Exception ex)
//            //                {
//            //                    Plugin.Logger.LogError($"{ex}");
//            //                }

//            //                //if (PaulovMatchmaking.IsClient)
//            //                //    timeHasComeScreenController.ChangeStatus(PaulovTarkovMPPlugin.LanguageDictionary["JOINING_COOP_GAME"].ToString());
//            //                //else
//            //                //    timeHasComeScreenController.ChangeStatus(PaulovTarkovMPPlugin.LanguageDictionary["CREATED_COOP_GAME"].ToString());

//            //                //PaulovLocalGameMode localGame = PaulovLocalGameMode.Create(
//            //                var localGame = LocalGame.smethod_6(

//            //                    ____inputTree
//            //                    , profile
//            //                    , gameWorld
//            //                    , ____localGameDateTime
//            //                    , session.InsuranceCompany
//            //                    , MonoBehaviourSingleton<MenuUI>.Instance
//            //                    , MonoBehaviourSingleton<GameUI>.Instance
//            //                    , ____raidSettings.SelectedLocation
//            //                    , timeAndWeather
//            //                    , ____raidSettings.WavesSettings
//            //                    , ____raidSettings.SelectedDateTime
//            //                    , new Callback<ExitStatus, TimeSpan, MetricsClass>((r) =>
//            //                    {
//            //                        ReflectionHelpers.GetAllMethodsForObject(__instance).FirstOrDefault(
//            //                            x =>
//            //                            x.GetParameters().Length >= 5
//            //                            && x.GetParameters()[0].ParameterType == typeof(string)
//            //                            && x.GetParameters()[1].ParameterType == typeof(Profile)
//            //                            && x.GetParameters()[2].ParameterType == typeof(LocationSettingsClass.Location)
//            //                            && x.GetParameters()[3].ParameterType == typeof(Result<ExitStatus, TimeSpan, MetricsClass>)
//            //                            //&& x.GetParameters()[4].ParameterType == typeof(TimeHasComeScreenController)
//            //                            ).Invoke(__instance, new object[] {
//            //                    session.Profile.Id, session.ProfileOfPet, ____raidSettings.SelectedLocation, r, timeHasComeScreenController });

//            //                    })
//            //                    , ____fixedDeltaTime
//            //                    , EUpdateQueue.Update
//            //                    , session
//            //                    , TimeSpan.FromSeconds(60 * ____raidSettings.SelectedLocation.EscapeTimeLimit)
//            //                    , metricsEvents
//            //                    , new GClass2433(metricsConfig, __instance)
//            //                    , ___localRaidSettings_0
//            //                );

//            //#if DEBUG
//            //                Plugin.Logger.LogDebug($"{nameof(TarkovApplicationLocalGameCreatorPatch)}:{nameof(Prefix)} Game Created");
//            //#endif

//            //                Singleton<AbstractGame>.Create(localGame);
//            //                metricsEvents.SetGameCreated();

//            //#if DEBUG
//            //                Plugin.Logger.LogDebug($"{nameof(TarkovApplicationLocalGameCreatorPatch)}:{nameof(Prefix)} Attempting to Run");
//            //#endif

//            //                //await localGame.Run(____raidSettings.BotSettings, ____backendUrl, null);
//            //                await localGame.method_4(____raidSettings.BotSettings, ____backendUrl, null);

//            //                //using (TokenStarter.StartWithToken("LoadingScreen.LoadComplete"))
//            //                //{
//            //                //    Plugin.Logger.LogDebug("LoadComplete");

//            //                //    UnityEngine.Object.DestroyImmediate(MonoBehaviourSingleton<MenuUI>.Instance.gameObject);
//            //                //    MainMenuController mmc =
//            //                //            (MainMenuController)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(TarkovApplication), typeof(MainMenuController)).GetValue(__instance);
//            //                //    mmc.Unsubscribe();

//            //                //    Singleton<GameWorld>.Instance.OnGameStarted();
//            //                //    Plugin.Logger.LogDebug("OnGameStarted");
//            //                //}

//            //            });
//            //            return false;
//        }

//    }
//}
