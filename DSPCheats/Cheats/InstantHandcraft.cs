using System;
using HarmonyLib;

namespace DSPCheats.Cheats
{
    class InstantHandcraft : Cheat
    {
        public override string GetCheatName() => "InstantHandcraft";
        public override string GetCheatDesc() => "Craft things in the replicator instantly";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ForgeTask), MethodType.Constructor, new Type[] { typeof(int), typeof(int) })]
        public static void ForgeTaskCreatePostfix(ref ForgeTask __instance)
        {
            __instance.tickSpend = 1;
        }
    }
}
