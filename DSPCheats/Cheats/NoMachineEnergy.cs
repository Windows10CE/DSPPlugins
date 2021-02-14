using System;
using System.Collections.Generic;
using HarmonyLib;

namespace DSPCheats.Cheats
{
    class NoMachineEnergy : Cheat
    {
        public override string GetCheatName() => "NoMachineEnergy";
        public override string GetCheatDesc() => "Stops machines from using energy.";

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PowerSystem), "GameTick")]
        public static void PowerTickPrefix(ref PowerSystem __instance, ref PowerConsumerComponent[] __state)
        {
            __state = new PowerConsumerComponent[__instance.consumerPool.Length];
            __instance.consumerPool.CopyTo(__state, 0);
            for (int i = 0; i < __instance.consumerPool.Length; i++)
            {
                __instance.consumerPool[i].idleEnergyPerTick = 0;
                __instance.consumerPool[i].workEnergyPerTick = 0;
                __instance.consumerPool[i].requiredEnergy = 0;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PowerSystem), "GameTick")]
        public static void PowerTickPostfix(ref PowerSystem __instance, ref PowerConsumerComponent[] __state)
        {
            __instance.consumerPool = __state;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "CalcTripEnergyCost")]
        public static bool StationComponentEnergyNeededPrefix(ref StationComponent __instance, ref long __result)
        {
            __instance.energy = __instance.energyMax;
            __result = 0L;
            return false;
        }
    }
}
