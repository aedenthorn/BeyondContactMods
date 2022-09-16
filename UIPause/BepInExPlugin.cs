using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Beyond.Cameras;
using Beyond.UI;
using Beyond.World;
using DevConsole;
using HarmonyLib;
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
        public static ConfigEntry<bool> disablePostProcessing;
        public static ConfigEntry<float> pauseTimeScale;

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
            disablePostProcessing = Config.Bind("Options", "DisablePostProcessing", true, "Disable post-processing while paused (removes flickering).");
            pauseTimeScale = Config.Bind("Options", "PauseTimeScale", 0.00001f, "Timescale while paused (set to 0.1 or higher to prevent flickering). 1 means no pause at all. Setting to 0 causes problems.");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);

        }


        [HarmonyPatch(typeof(WorldManagerState), nameof(WorldManagerState.Update))]
        static class WorldManagerState_Update_Patch
        {
            static void Postfix()
            {
                if (!modEnabled.Value)
                    return;
                if (GUIManager.Inst.Panels.ResearchCraftingPanel.IsResearchAndDevOpen())
                {
                    if (disablePostProcessing.Value && CameraManager.Inst.InGameCamera.PostProcessLayer.enabled)
                        CameraManager.Inst.InGameCamera.PostProcessLayer.enabled = false;
                    Time.timeScale = pauseTimeScale.Value;
                }
                else if (disablePostProcessing.Value && !CameraManager.Inst.InGameCamera.PostProcessLayer.enabled)
                {
                    CameraManager.Inst.InGameCamera.PostProcessLayer.enabled = true;
                }
            }
        }
    }
}
