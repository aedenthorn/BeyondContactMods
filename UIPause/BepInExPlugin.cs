using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Beyond.UI;
using Beyond.World;
using DevConsole;
using HarmonyLib;
using Il2CppSystem.Collections;
using System.Reflection;
using UnityEngine;

namespace UIPause
{
    [BepInPlugin("aedenthorn.UIPause", "UI Pause", "0.1.0")]
    public class BepInExPlugin : BasePlugin
    {
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<int> nexusID;

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

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);

        }


        [HarmonyPatch(typeof(Console), nameof(Console.OnGUI))]
        static class Console_OnGUI_Patch
        {
            static void Postfix(Console __instance)
            {
                if (!modEnabled.Value || Event.current.type != EventType.Repaint || (!GUIManager.Inst.Panels.ResearchCraftingPanel.IsResearchAndDevOpen()))
                    return;
                Time.timeScale = 0;
            }
        }
    }
}
