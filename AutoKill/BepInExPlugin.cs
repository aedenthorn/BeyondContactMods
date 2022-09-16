using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Beyond;
using Beyond.Cameras;
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

namespace AutoKill
{
    [BepInPlugin("aedenthorn.AutoKill", "AutoKill", "0.1.0")]
    public class BepInExPlugin : BasePlugin
    {
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<int> nexusID;
        
        public static ConfigEntry<float> areaKillRange;
        public static ConfigEntry<string> areaHotKey;
        public static ConfigEntry<string> closestHotKey;

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

            areaKillRange = Config.Bind<float>("Options", "AreaKillRange", 10f, "Area kill range.");
            areaHotKey = Config.Bind<string>("Options", "AreaHotKey", "k", "Hotkey to kill enemies in an area");
            closestHotKey = Config.Bind<string>("Options", "ClosestHotKey", "j", "Hotkey to kill nearest enemy");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }
        [HarmonyPatch(typeof(InGameCamera), "LateUpdate")]
        static class InGameCamera_Update_Patch
        {
            static void Postfix()
            {
                if (!modEnabled.Value)
                    return;

                if (AedenthornUtils.CheckKeyDown(areaHotKey.Value))
                {
                    foreach(var actor in GameObject.FindObjectsOfType<BaseActor>())
                    {
                        if(actor != PlayerManager.Inst.LocalPlayer.PlayerActor && !actor.IsHarmless && Vector3.Distance(PlayerManager.Inst.LocalPlayer.PlayerActor.transform.position, actor.transform.position) <= areaKillRange.Value)
                        {
                            actor.GetComponent<ComponentStats>().
                        }
                    }
                }
                else if (AedenthornUtils.CheckKeyDown(closestHotKey.Value))
                {
                    BaseActor closestActor = null;
                    foreach (var actor in GameObject.FindObjectsOfType<BaseActor>())
                    {
                        if (actor != PlayerManager.Inst.LocalPlayer.PlayerActor && !actor.IsHarmless && Vector3.Distance(PlayerManager.Inst.LocalPlayer.PlayerActor.transform.position, actor.transform.position) <= areaKillRange.Value && (closestActor == null || Vector3.Distance(PlayerManager.Inst.LocalPlayer.PlayerActor.transform.position, actor.transform.position) < Vector3.Distance(PlayerManager.Inst.LocalPlayer.PlayerActor.transform.position, closestActor.transform.position)))
                        {
                            closestActor = actor; 
                        }
                        if(closestActor != null)
                            Dbgl(actor.gameObject.name);
                    }
                }
            }
        }
    }
}
