//using Comfort.Common;
//using EFT;
//using EFT.InventoryLogic;
//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;

//namespace Paulov.Tarkov.Local.Patches;

///// <summary>
///// The Prestige Controller seems to cause an issue because it is expecting the user to be online and downloaded the profile?
///// TODO: Fix this via the Server Side!!
///// </summary>
//internal class ScavModePrestigePatch : NullHarmonyPatch
//{
//    public override MethodBase GetMethodToPatch()
//    {
//        Plugin.Logger.LogDebug($"{nameof(ScavModePrestigePatch)}.GetMethodToPatch");

//        var prestigeControllerType = Plugin.EftTypes.FirstOrDefault(x
//            => ReflectionHelpers.GetAllMethodsForType(x).Any(y =>
//                y.IsConstructor
//                && y.GetParameters().Length == 4
//                && y.GetParameters()[0].Name == "profile"
//                && y.GetParameters()[1].Name == "inventoryController"
//                && y.GetParameters()[2].Name == "questBook"
//                && y.GetParameters()[3].Name == "session"
//                )
//            );
//        if (prestigeControllerType == null)
//        {
//            Plugin.Logger.LogError("Could not find PrestigeControllerType");
//            return null;
//        }

//        var questType = Plugin.EftTypes.FirstOrDefault(x
//            => ReflectionHelpers.GetAllMethodsForType(x).Any(y => y.Name == "RemoveConditionalsWithoutTemplate")
//            && ReflectionHelpers.GetAllMethodsForType(x).Any(y => y.Name == "UpdateDailyQuests")
//            );

//        if (questType == null)
//        {
//            Plugin.Logger.LogError("Could not find QuestType");
//            return null;
//        }




//        //return AccessTools.Constructor(typeof(GClass3690), new Type[] { typeof(Profile), typeof(InventoryController), questType, typeof(ISession) }, false);
//        return AccessTools.Constructor(prestigeControllerType, new Type[] { typeof(Profile), typeof(InventoryController), questType, typeof(ISession) }, false);
//    }

//    public override HarmonyMethod GetPrefixMethod()
//    {
//        return new HarmonyMethod(this.GetType().GetMethod(nameof(PrefixOverrideMethod), BindingFlags.Public | BindingFlags.Static));
//    }

//    public static void PrefixOverrideMethod(ref Profile profile)
//    {
//        if (profile.Side == EPlayerSide.Savage)
//            profile = Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession().Profile;
//    }
//}
