using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Beyond.Environment.Rifts;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace UIPause
{
    [BepInPlugin("aedenthorn.RiftMod", "Rift Mod", "0.1.0")]
    public class BepInExPlugin : BasePlugin
    {
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<int> nexusID;

        public static ConfigEntry<bool> disableRifts;
        public static ConfigEntry<float> doomsdayRiftCountMult;

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
            disableRifts = Config.Bind<bool>("Options", "DisableRifts", true, "Disable rifts entirely");
            //doomsdayRiftCountMult = Config.Bind<float>("Options", "DoomsdayRiftCountMult", 1f, "Multiply number of rifts required for doomsday by this amount.");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);

        }
        [HarmonyPatch(typeof(RiftSpawnSystem), nameof(RiftSpawnSystem.IsRiftSpawningAllowed))]
        static class RiftSpawnSystem_IsRiftSpawningAllowed_Patch
        {
            static bool Prefix(ref bool __result)
            {
                if (!modEnabled.Value || !disableRifts.Value)
                    return true;
                __result = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(SpawnRift), nameof(SpawnRift.Execute))]
        static class SpawnRif_Execute_Patch
        {
            static bool Prefix()
            {
                return !modEnabled.Value || !disableRifts.Value;
            }
        }
        [HarmonyPatch(typeof(SpawnRift), nameof(SpawnRift.CreateRift))]
        static class SpawnRif_CreateRift_Patch
        {
            static bool Prefix()
            {
                return !modEnabled.Value || !disableRifts.Value;
            }
        }
    }
}
