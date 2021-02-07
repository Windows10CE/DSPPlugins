using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace DSPCheats
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInProcess("DSPGAME.exe")]
    public class DSPCheatsPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.Windows10CE.DSPCheats";
        public const string ModName = "DSPCheats";
        public const string ModVer = "1.1.0";

        public void Awake()
        {
            bool unlockInstantly = Config.Bind<bool>("Cheat Toggles", "UnlockInstantly", false, "Starting research will instantly unlock it.").Value;
            bool noMechaEnergy = Config.Bind<bool>("Cheat Toggles", "NoMechaEnergy", false, "Stops the mecha from consuming energy.").Value;
            bool noMachineEnergy = Config.Bind<bool>("Cheat Toggles", "NoMachineEnergy", false, "Stops machines from using energy.").Value;
            bool freeHandcraft = Config.Bind<bool>("Cheat Toggles", "FreeHandcraft", false, "Craft things in the replicator for free.").Value;
            bool instantBuild = Config.Bind<bool>("Cheat Toggles", "InstantBuild", false, "Construction drones build everything instantly.").Value;

            var harmony = new Harmony(ModGuid);
            
            if (unlockInstantly)
                harmony.PatchAll(typeof(UnlockInstantlyPatch));
            if (noMechaEnergy)
                harmony.PatchAll(typeof(NoMechaEnergyPatch));
            if (noMachineEnergy)
                harmony.PatchAll(typeof(NoMachineEnergyPatch));
            if (freeHandcraft)
            {
                new ILHook(typeof(MechaForge).GetMethod("AddTaskIterate", BindingFlags.NonPublic | BindingFlags.Instance), new ILContext.Manipulator(FreeHandcraftPatch.DontTakeItemsILHook));
                harmony.PatchAll(typeof(FreeHandcraftPatch));
            }
            if (instantBuild)
                harmony.PatchAll(typeof(InstantBuildPatch));
        }
    }

    [HarmonyPatch]
    public class UnlockInstantlyPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameHistoryData), "currentTech", MethodType.Setter)]
        public static void CurrentTechSet(ref GameHistoryData __instance, ref int value)
        {
            if (value != 0)
            {
                TechState tech = __instance.TechState(value);
                TechProto proto = LDB.techs.Select(value);
                int current = tech.curLevel;
                for (; current < tech.maxLevel; current++)
                    for (int j = 0; j < proto.UnlockFunctions.Length; j++)
                        __instance.UnlockTechFunction(proto.UnlockFunctions[j], proto.UnlockValues[j], current);
                __instance.UnlockTech(value);
                __instance.DequeueTech();
            }
        }
    }

    [HarmonyPatch]
    public class NoMechaEnergyPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mecha), "UseEnergy")]
        public static bool UseEnergyPrefix(ref Mecha __instance, ref float __result)
        {
            __result = 1f;

            __instance.coreEnergy = __instance.coreEnergyCap;

            return false;
        }
    }

    [HarmonyPatch]
    public class NoMachineEnergyPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PowerConsumerComponent), "Import")]
        public static void PowerConsumerPostfix(ref PowerConsumerComponent __instance)
        {
            __instance.idleEnergyPerTick = 0L;
            __instance.workEnergyPerTick = 0L;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PowerSystem), "NewConsumerComponent")]
        public static void NewConsumerComponentPrefix(ref long work, ref long idle)
        {
            work = idle = 0;
        }
    }

    [HarmonyPatch]
    public class FreeHandcraftPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MechaForge), "PredictTaskCount")]
        public static bool MechaForgePredictTaskCountPrefix(ref int __result, ref int maxShowing)
        {
            __result = maxShowing;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ForgeTask), MethodType.Constructor, new Type[] { typeof(int), typeof(int) })]
        public static void ForgeTaskCreatePostfix(ref ForgeTask __instance)
        {
            __instance.tickSpend = 1;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MechaForge), "TryAddTask")]
        public static bool TryAddTaskPrefix(ref bool __result)
        {
            __result = true;
            return false;
        }

        public static void DontTakeItemsILHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int count2loc = 0;

            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt(typeof(MechaForge).GetProperty("player").GetGetMethod()),
                x => x.MatchCallOrCallvirt(typeof(Player).GetProperty("package").GetGetMethod()),
                x => x.MatchLdloc(out _),
                x => x.MatchLdloc(out count2loc),
                x => x.MatchCallOrCallvirt(typeof(StorageComponent).GetMethod("TakeItem")),
                x => x.MatchStloc(out _)
            );

            c.RemoveRange(6);
            c.Emit(OpCodes.Ldloc, count2loc);
        }
    }

    [HarmonyPatch]
    public class InstantBuildPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MechaDroneLogic), "UpdateTargets")]
        public static void UpdateDronesPrefix(MechaDroneLogic __instance, ref Vector3 ___playerPos, ref float ___sqrMinBuildAlt)
        {
            var _this = __instance;

            PrebuildData[] prebuildPool = _this.factory.prebuildPool;
            float num2 = _this.player.mecha.buildArea * _this.player.mecha.buildArea;
            int num3 = 0;
            Vector3 zero = Vector3.zero;

            for (int i = 1; i < _this.factory.prebuildCursor; i++)
            {
                if (prebuildPool[i].id == i)
                {
                    int num4 = -i;
                    Vector3 a = new Vector3();
                    a = Vector3.zero;
                    if (num4 > 0)
                    {
                        a = _this.factory.entityPool[num4].pos;
                    }
                    if (num4 < 0)
                    {
                        a = _this.factory.prebuildPool[-num4].pos;
                    }
                    float sqrMagnitude = (a - ___playerPos).sqrMagnitude;
                    if (a.sqrMagnitude > ___sqrMinBuildAlt && sqrMagnitude <= num2 && !_this.serving.Contains(num4))
                    {
                        num2 = sqrMagnitude;
                        num3 = num4;
                    }
                }
            }
            for (int j = 0; j < prebuildPool.Length; j++)
            {
                __instance.factory.BuildFinally(_this.player, prebuildPool[j].id);
            }
            if (_this.player.mecha.coreEnergy > _this.player.mecha.droneEjectEnergy && num3 != 0)
            {
                MechaDrone[] drones = _this.player.mecha.drones;
                int droneCount = _this.player.mecha.droneCount;
                for (int k = 0; k < droneCount; k++)
                {
                    if (drones[k].stage == 0)
                    {
                        drones[k].stage = 1;
                        drones[k].targetObject = num3;
                        drones[k].movement = _this.player.mecha.droneMovement;
                        drones[k].position = ___playerPos;
                        Vector3 a = new Vector3();
                        a = Vector3.zero;
                        if (num3 > 0)
                        {
                            a = _this.factory.entityPool[num3].pos;
                        }
                        if (num3 < 0)
                        {
                            a = _this.factory.prebuildPool[-num3].pos;
                        }
                        drones[k].target = a;
                        drones[k].initialVector = ___playerPos + ___playerPos.normalized * 4.5f + ((drones[k].target - drones[k].position).normalized + UnityEngine.Random.insideUnitSphere) * 1.5f;
                        drones[k].forward = drones[k].initialVector;
                        drones[k].progress = 0f;
                        _this.serving.Add(num3);
                        _this.player.mecha.UseEnergy(_this.player.mecha.droneEjectEnergy);
                        return;
                    }
                }
            }
        }
    }
}
