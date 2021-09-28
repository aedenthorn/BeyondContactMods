using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Beyond.Input;
using Beyond.Networking;
using Beyond.World;
using DevConsole;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Console = DevConsole.Console;

namespace DevConsoleMod
{
    [BepInPlugin("aedenthorn.Sprint", "Sprint", "0.1.0")]
    public class BepInExPlugin : BasePlugin
    {
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<int> nexusID;
        
        public static ConfigEntry<float> sprintSpeedMult;
        public static ConfigEntry<string> hotKey;

        public static BepInExPlugin context;
        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug.Value)
                Debug.Log((pref ? typeof(BepInExPlugin).Namespace + " " : "") + str);
        }

        public static bool pressedKey = false;

        public override void Load()
        {
            context = this;
            modEnabled = Config.Bind("General", "Enabled", true, "Enable this mod");
            isDebug = Config.Bind<bool>("General", "IsDebug", true, "Enable debug logs");

            sprintSpeedMult = Config.Bind<float>("Options", "SprintSpeedMult", 1.5f, "Sprint speed multiplier.");
            hotKey = Config.Bind<string>("Options", "HotKey", "left shift", "Hotkey to toggle sprinting");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }
        [HarmonyPatch(typeof(Console), nameof(Console.Update))]
        static class Console_Update_Patch
        {
            static void Postfix(Console __instance)
            {
                if (!modEnabled.Value)
                    return;

                if (AedenthornUtils.CheckKeyDown(hotKey.Value))
                {
                    pressedKey = true;
                    PlayerManager.Inst.LocalPlayer.PlayerActor._moveSpeed *= sprintSpeedMult.Value;
                    //Dbgl($"starting sprint {PlayerManager.Inst.LocalPlayer.PlayerActor._moveSpeed}x");
                }
                else if (pressedKey && AedenthornUtils.CheckKeyUp(hotKey.Value))
                {
                    pressedKey = false;
                    PlayerManager.Inst.LocalPlayer.PlayerActor._moveSpeed /= sprintSpeedMult.Value;
                    //Dbgl($"Ending sprint {PlayerManager.Inst.LocalPlayer.PlayerActor._moveSpeed}x");
                }
            }
        }
    }
}
