using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Beyond.Cameras;
using Beyond.Networking;
using HarmonyLib;
using RootMotion;
using System.Reflection;
using UnityEngine;

namespace FreeCamera
{
    [BepInPlugin("aedenthorn.FreeCamera", "Free Camera", "0.1.0")]
    public class BepInExPlugin : BasePlugin
    {
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<int> nexusID;

        public static ConfigEntry<bool> depthOfField;
        public static ConfigEntry<float> minZoomDistance;
        public static ConfigEntry<float> maxZoomDistance;
        public static ConfigEntry<float> scrollWheelSensitivity;
        
        public static ConfigEntry<string> hotKey;
        public static ConfigEntry<string> resetHotKey;
        public static ConfigEntry<string> modKeyX;
        public static ConfigEntry<string> modKeyY;
        public static ConfigEntry<string> modKeyZ;

        public static Vector3 currentRotOffset = Vector3.zero;
        public static Vector3 currentOffset = Vector3.zero;


        private static Vector3 lastMousePosition = -Vector3.one;

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
            
            depthOfField = Config.Bind<bool>("Options", "DepthOfField", true, "Enable depth of field");

            minZoomDistance = Config.Bind<float>("Options", "MinZoomDistance", 1, "Minimum zoom distance");
            maxZoomDistance = Config.Bind<float>("Options", "MaxZoomDistance", 19, "Maximum zoom distance");
            scrollWheelSensitivity = Config.Bind<float>("Options", "ScrollSensitivity", 10, "Scroll sensitivity");
            hotKey = Config.Bind<string>("Options", "HotKey", "mouse 2", "Hotkey to toggle dev console.");
            resetHotKey = Config.Bind<string>("Options", "ResetHotKey", "end", "Hotkey to reset camera x and z rotation.");
            modKeyX = Config.Bind<string>("Options", "ModKeyX", "left alt", "Modifier key to rotate or move on X axis.");
            modKeyY = Config.Bind<string>("Options", "ModKeyY", "left shift", "Modifier key to move on Y axis.");
            modKeyZ = Config.Bind<string>("Options", "ModKeyZ", "left ctrl", "Modifier key to rotate or move on Z axis.");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }


        [HarmonyPatch(typeof(InGameCamera), nameof(InGameCamera.ZoomCamera))]
        static class InGameCamera_ZoomCamera_Patch
        {
            static bool Prefix(InGameCamera __instance, float zoomSpeed, ref float scrollSensitivity)
            {
                if (!modEnabled.Value)
                    return true;

                scrollSensitivity = scrollWheelSensitivity.Value;

                if (AedenthornUtils.CheckKeyHeld(modKeyX.Value))
                {
                    currentOffset.x += zoomSpeed * scrollWheelSensitivity.Value / 10f;
                }
                else if (AedenthornUtils.CheckKeyHeld(modKeyZ.Value))
                {
                    currentOffset.z += zoomSpeed * scrollWheelSensitivity.Value / 10f;
                }
                else if (AedenthornUtils.CheckKeyHeld(modKeyY.Value))
                {
                    currentOffset.y += zoomSpeed * scrollWheelSensitivity.Value / 10f;
                }
                else
                    return true;

                return false;
            }
        }

        [HarmonyPatch(typeof(InGameCamera), nameof(InGameCamera.LateUpdate))]
        static class InGameCamera_LateUpdate_Patch
        {

            static void Prefix(InGameCamera __instance)
            {
                __instance.MinZoomDistance = minZoomDistance.Value;
                __instance.MaxZoomDistance = maxZoomDistance.Value;
            }
            static void Postfix(InGameCamera __instance)
            {
                if (!modEnabled.Value)
                    return;

                if (AedenthornUtils.CheckKeyHeld(hotKey.Value))
                {
                    float diff = Input.mousePosition.x - lastMousePosition.x;
                    if (!AedenthornUtils.CheckKeyHeld(modKeyX.Value) && !AedenthornUtils.CheckKeyHeld(modKeyZ.Value))
                    {
                        __instance.Rotate(diff / 2);
                    }
                    else
                    {
                        if (AedenthornUtils.CheckKeyHeld(modKeyX.Value))
                        {
                            currentRotOffset.x += diff;
                        }
                        else
                        {
                            currentRotOffset.z += diff;
                        }
                    }
                }
                else if (AedenthornUtils.CheckKeyDown(resetHotKey.Value))
                {
                    Dbgl($"Resetting camera");

                    currentRotOffset = Vector3.zero;
                    currentOffset = Vector3.zero;
                }
                __instance.Camera.transform.parent.position += currentOffset;
                __instance.Camera.transform.parent.rotation = Quaternion.Euler(__instance.Camera.transform.parent.rotation.eulerAngles + currentRotOffset);

                lastMousePosition = Input.mousePosition;
            }
        }
    }
}
