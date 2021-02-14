using HarmonyLib;
using UnityEngine;

namespace DSPCheats.Cheats
{
    class InstantBuild : Cheat
    {
        public override string GetCheatName() => "InstantBuild";
        public override string GetCheatDesc() => "Construction drones build everything instantly.";

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
