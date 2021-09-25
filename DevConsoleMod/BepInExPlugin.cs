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
    [BepInPlugin("aedenthorn.DevConsoleMod", "Dev Console Mod", "0.1.0")]
    public class BepInExPlugin : BasePlugin
    {
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<int> nexusID;

        public static ConfigEntry<string> hotKey;


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

            hotKey = Config.Bind<string>("Options", "HotKey", "`", "Hotkey to toggle dev console.");

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
                    Dbgl("Opening Console");
                    Console.Open();
                }
            }
        }
        [HarmonyPatch(typeof(Console), nameof(Console.FadeInOut))]
        static class Console_FadeInOut_Patch
        {
            private static InputStateType lastState;

            static void Postfix(Console __instance, bool open)
            {
                if (!modEnabled.Value)
                    return;
                if (open)
                {
                    lastState = InputManager.Inst.CurrentInputStateType;
                    if (lastState != InputStateType.GameplayMenu)
                        InputManager.ChangeInputState(InputStateType.MainMenuLoading);
                }
                else
                {
                    Dbgl("Last State: " + lastState.ToString());
                    if(lastState != InputStateType.GameplayMenu)
                        InputManager.ChangeInputState(lastState);
                }
            }
        }
        [HarmonyPatch(typeof(Console), nameof(Console.CommandExists))]
        static class Console_CommandExists_Patch
        {
            static void Postfix(Console __instance, string commandName)
            {
                if (!modEnabled.Value)
                    return;

                Dbgl($"Command: {commandName} ");
            }
        }
        [HarmonyPatch(typeof(Console), nameof(Console.ExecuteCommandInternal), new Type[] { typeof(string)})]
        static class Console_ExecuteCommandInternal_Patch
        {
            static bool Prefix(Console __instance, string command)
            {
                if (!modEnabled.Value)
                    return true;

                command = command.ToLower();

                Dbgl($"Executing {command}");

                if (command == "pos")
                {
                    var pos = PlayerManager.Inst.LocalPlayer.PlayerActor.Trans.position;
                    var output = $"{Math.Round(pos.x, 2)},{Math.Round(pos.y, 2)},{Math.Round(pos.z, 2)}\n";
                    Dbgl(output);
                    __instance.AddText(output, "FFFFAA");
                    return false;
                }

                return true;
            }
        }
    }
}
