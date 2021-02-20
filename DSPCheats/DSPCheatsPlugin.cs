using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using BepInEx.Configuration;
using DSPCheats.Cheats;

namespace DSPCheats
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInProcess("DSPGAME.exe")]
    public class DSPCheatsPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.Windows10CE.DSPCheats";
        public const string ModName = "DSPCheats";
        public const string ModVer = "2.2.0";

        internal static Harmony harmony;
        new internal static ManualLogSource Logger;
        new internal static ConfigFile Config;

        public static UnityEngine.AssetBundle DSPCheatsAssets;

        public static List<Cheat> Cheats;

        public void Awake()
        {
            DSPCheatsPlugin.Logger = base.Logger;
            DSPCheatsPlugin.Config = base.Config;

            DSPCheatsAssets = UnityEngine.AssetBundle.LoadFromMemory(Properties.Resources.dspcheatsbundle);

            harmony = new Harmony(ModGuid);
            harmony.PatchAll(typeof(ResetConfigHook));

            Cheats = new List<Cheat>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.BaseType == typeof(Cheat)))
                Cheats.Add((Cheat)type.GetConstructor(new Type[0]).Invoke(new object[0]));

            foreach (var cheat in Cheats)
                if (cheat.ConfigValue.Value)
                {
                    harmony.PatchAll(cheat.GetType());
                    cheat.ChangeState(true);
                }

            Config.ConfigReloaded += ResetConfigHook.OnConfigReload;
        }
    }

    public class ResetConfigHook
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameMain), "Begin")]
        public static void HookGameStart() => DSPCheatsPlugin.Config.Reload();

        public static void OnConfigReload(object sender, EventArgs e)
        {
            DSPCheatsPlugin.harmony.UnpatchSelf();
            DSPCheatsPlugin.harmony.PatchAll(typeof(ResetConfigHook));
            foreach (var cheat in DSPCheatsPlugin.Cheats)
            {
                DSPCheatsPlugin.Logger.LogMessage($"{cheat.GetCheatName()}: {(cheat.ConfigValue.Value ? "Enabled" : "Disabled")}");
                if (cheat.ConfigValue.Value)
                    DSPCheatsPlugin.harmony.PatchAll(cheat.GetType());
                cheat.ChangeState(cheat.ConfigValue.Value);
            }
        }
    }
}
