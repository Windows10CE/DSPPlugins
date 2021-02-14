using HarmonyLib;

namespace DSPCheats.Cheats
{
    public class NoMechaEnergy : Cheat
    {
        public override string GetCheatName() => "NoMechaEnergy";
        public override string GetCheatDesc() => "Stops the mecha from consuming energy.";

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mecha), "UseEnergy")]
        public static bool UseEnergyPrefix(ref Mecha __instance, ref float __result)
        {
            __result = 1f;

            __instance.coreEnergy = __instance.coreEnergyCap;

            return false;
        }
    }
}
