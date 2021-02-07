using System.Reflection;
using UnityEngine;
using BepInEx;
using HarmonyLib;

namespace IcarusColorPicker
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class IcarusColorPickerPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.Windows10CE.IcarusColorPicker";
        public const string ModName = "IcarusColorPicker";
        public const string ModVer = "1.0.0";

        public static Color ArmorColor { get; private set; } = new Color();
        public static Color SkeletonColor { get; private set; } = new Color();

        public void Awake()
        {
            ArmorColor = new Color(
                Config.Bind<float>("Armor", "R", 1f, "Red value of the armor").Value,
                Config.Bind<float>("Armor", "G", 0.6846404f, "Blue value of the armor").Value,
                Config.Bind<float>("Armor", "B", 0.24313718f, "Green value of the armor").Value,
                Config.Bind<float>("Armor", "A", 1f, "Alpha value of the armor").Value
            );
            SkeletonColor = new Color(
                Config.Bind<float>("Skeleton", "R", 1f, "Red value of the skeleton").Value,
                Config.Bind<float>("Skeleton", "G", 1f, "Green value of the skeleton").Value,
                Config.Bind<float>("Skeleton", "B", 1f, "Blue value of the skeleton").Value,
                Config.Bind<float>("Skeleton", "A", 1f, "Alpha value of the skeleton").Value
            );

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGuid);
        }
    }

    [HarmonyPatch(typeof(PlayerAnimator), "Start")]
    public class IcarusColorPatch
    {
        [HarmonyPrefix]
        public static void IcarusAnimatorStartPrefix(ref PlayerAnimator __instance)
        {
            for (int i = 0; i < __instance.materials.Length; i++)
            {
                if (__instance.materials[i].name == "icarus-armor")
                    __instance.materials[i].color = IcarusColorPickerPlugin.ArmorColor;
                else if (__instance.materials[i].name == "icarus-skeleton")
                    __instance.materials[i].color = IcarusColorPickerPlugin.SkeletonColor;
            }
        }
    }
}
