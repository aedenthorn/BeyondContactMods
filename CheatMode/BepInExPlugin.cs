using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Beyond;
using Beyond.DebugUtility;
using Beyond.Equipment;
using Beyond.World;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

namespace CheatMode
{
    [BepInPlugin("aedenthorn.CheatMode", "Cheat Mode", "0.1.0")]
    public class BepInExPlugin: BasePlugin
    {
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<int> nexusID;

        public static ConfigEntry<bool> cheatModeOn;
        public static ConfigEntry<bool> infiniteHealth;
        public static ConfigEntry<bool> noTemperatureDamage;
        
        public static ConfigEntry<string> hotKey;

        public static ConfigEntry<int> durabilityLossTickMult;
        public static ConfigEntry<int> foodLossTickMult;
        public static ConfigEntry<int> oxygenLossTickMult;
        public static ConfigEntry<int> timeTickDayMult;
        public static ConfigEntry<int> timeTickNightMult;


        public static int foodTimeDelta;
        public static int oxygenTimeDelta;
        public static int durabilityTimeDelta1;
        public static int durabilityTimeDelta2;
        public static int timeTickDelta;
        public static TimeOfDay lastTickTimeOfDay;

        public static bool changed = false;

        public static BepInExPlugin context;
        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug.Value)
                Debug.Log((pref ? typeof(BepInExPlugin).Namespace + " " : "") + str);
        }
        public override void Load()
        {
            context = this;
            modEnabled = Config.Bind("General", "Enabled", true, "Enable this mod");
            isDebug = Config.Bind<bool>("General", "IsDebug", true, "Enable debug logs");

            cheatModeOn = Config.Bind<bool>("Options", "CheatModeOn", false, "Cheat mode currently enabled");

            durabilityLossTickMult = Config.Bind<int>("Options", "DurabilityLossMult", 0, "Multiply time it takes for one tick of durability loss by this amount while cheat mode is on (set to 0 to disable durability loss)");
            foodLossTickMult = Config.Bind<int>("Options", "FoodLossMult", 0, "Multiply time it takes for one tick of food loss by this amount while cheat mode is on (set to 0 to disable food loss)");
            oxygenLossTickMult = Config.Bind<int>("Options", "OxygenLossMult", 0, "Multiply time it takes for one tick of oxygen loss by this amount while cheat mode is on (set to 0 to disable oxygen loss)");
            timeTickDayMult = Config.Bind<int>("Options", "TimeTickDayMult", 0, "During the day, multiply time it takes for one time tick by this amount while cheat mode is on (set to 0 to disable time of day change)");
            timeTickNightMult = Config.Bind<int>("Options", "TimeTickNightMult", 0, "During the night, multiply time it takes for one time tick by this amount while cheat mode is on (set to 0 to disable time of day change)");

            //infiniteHealth = Config.Bind<bool>("Options", "InfiniteHealth", true, "Infinite health while cheat mode is on");
            noTemperatureDamage = Config.Bind<bool>("Options", "NoTemperatureDamage", true, "No temperature damage while cheat mode is on");

            hotKey = Config.Bind<string>("Options", "HotKey", "home", "Hotkey to toggle cheat mode.");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);

        }

        [HarmonyPatch(typeof(WorldManagerState), "Update")]
        static class WorldManagerState_OnUpdate_Patch
        {
            static void Postfix(WorldManagerState __instance)
            {
                if (!modEnabled.Value)
                    return;

                if (AedenthornUtils.CheckKeyDown(hotKey.Value))
                {
                    cheatModeOn.Value = !cheatModeOn.Value;
                    Dbgl($"Cheat mode: {cheatModeOn.Value}");
                }

            }
        }
        
        [HarmonyPatch(typeof(EnviromentalDamageSystem), nameof(EnviromentalDamageSystem.CheckColdTemperatureDamage))]
        static class EnviromentalDamageSystem_CheckColdTemperatureDamage_Patch
        {
            static bool Prefix()
            {
                return !modEnabled.Value || !cheatModeOn.Value || !noTemperatureDamage.Value;
            }
        }

        [HarmonyPatch(typeof(EnviromentalDamageSystem), nameof(EnviromentalDamageSystem.CheckHotTemperatureDamage))]
        static class EnviromentalDamageSystem_CheckHotTemperatureDamage_Patch
        {
            static bool Prefix()
            {
                return !modEnabled.Value || !cheatModeOn.Value || !noTemperatureDamage.Value;
            }
        }

        [HarmonyPatch(typeof(DurabilityConstantLossSystem), nameof(DurabilityConstantLossSystem.TimeslicedUpdate))]
        static class DurabilityConstantLossSystem_TimeslicedUpdate_Patch
        {
            static bool Prefix()
            {
                if (!modEnabled.Value || !cheatModeOn.Value)
                    return true;

                if (durabilityLossTickMult.Value <= 0)
                    return false;

                durabilityTimeDelta1++;
                if (durabilityTimeDelta1 >= durabilityLossTickMult.Value)
                {
                    durabilityTimeDelta1 = 0;
                    return true;
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(DurabilityConstantLossWhenEquippedSystem), nameof(DurabilityConstantLossWhenEquippedSystem.TimeslicedUpdate))]
        static class DurabilityConstantLossSystem_DurabilityConstantLossWhenEquippedSystem_Patch
        {
            static bool Prefix()
            {
                if (!modEnabled.Value || !cheatModeOn.Value)
                    return true;

                if (durabilityLossTickMult.Value <= 0)
                    return false;

                durabilityTimeDelta2++;
                if (durabilityTimeDelta2 >= durabilityLossTickMult.Value)
                {
                    durabilityTimeDelta2 = 0;
                    return true;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(TimeOfDaySystem), nameof(TimeOfDaySystem.OnUpdate))]
        static class TimeOfDaySystem_Update_Patch
        {
            static bool Prefix()
            {
                if (!modEnabled.Value || !cheatModeOn.Value)
                    return true;

                if(NetworkedWorldObject.TimeOfDay != lastTickTimeOfDay)
                {
                    timeTickDelta = 0;
                }

                var mult = NetworkedWorldObject.TimeOfDay == TimeOfDay.Night ? timeTickNightMult.Value : timeTickDayMult.Value;

                if (mult <= 0)
                    return false;

                timeTickDelta++;
                if (timeTickDelta >= mult)
                {
                    timeTickDelta = 0;
                    return true;
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(StatsDrainingSystem), nameof(StatsDrainingSystem.DrainOxygen))]
        static class StatsDrainingSystem_DrainOxygen_Patch
        {
            static bool Prefix()
            {
                if (!modEnabled.Value || !cheatModeOn.Value)
                    return true;

                if (oxygenLossTickMult.Value <= 0)
                    return false;

                oxygenTimeDelta++;
                if (oxygenTimeDelta >= oxygenLossTickMult.Value)
                {
                    oxygenTimeDelta = 0;
                    return true;
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(StatsDrainingSystem), nameof(StatsDrainingSystem.DrainStamina))]
        static class StatsDrainingSystem_DrainStamina_Patch
        {
            static bool Prefix()
            {
                if (!modEnabled.Value || !cheatModeOn.Value)
                    return true;

                if (foodLossTickMult.Value <= 0)
                    return false;

                foodTimeDelta++;
                if (foodTimeDelta >= foodLossTickMult.Value)
                {
                    foodTimeDelta = 0;
                    return true;
                }
                return false;
            }
        }
    }
}
