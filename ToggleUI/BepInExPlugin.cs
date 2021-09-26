using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using DevConsole;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace ToggleUI
{
    [BepInPlugin("aedenthorn.ToggleUI", "Toggle UI", "0.1.0")]
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
            
            hotKey = Config.Bind<string>("Options", "HotKey", "u", "Hotkey to toggle dev console.");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }


        [HarmonyPatch(typeof(Console), nameof(Console.Update))]
        static class Console_Update_Patch
        {
            static void Postfix()
            {
                if (!modEnabled.Value || !AedenthornUtils.CheckKeyDown(hotKey.Value))
                    return;
                Console.ExecuteCommand("toggleUI");
            }
        }

    }
}
