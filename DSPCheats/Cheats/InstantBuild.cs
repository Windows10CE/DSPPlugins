using HarmonyLib;
using UnityEngine;

namespace DSPCheats.Cheats
{
    class InstantBuild : Cheat
    {
        public override string GetCheatName() => "InstantBuild";
        public override string GetCheatDesc() => "Construction drones build everything instantly.";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechaDroneLogic), "UpdateTargets")]
        public static void UpdateDronesPrefix(MechaDroneLogic __instance)
        {
            foreach (var prebuild in __instance.factory.prebuildPool)
                __instance.factory.BuildFinally(__instance.player, prebuild.id);
        }
    }
}
