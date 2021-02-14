using HarmonyLib;

namespace DSPCheats.Cheats
{
    public class UnlockInstantlyCheat : Cheat
    {
        public override string GetCheatName() => "UnlockInstantly";
        public override string GetCheatDesc() => "Starting a research will instantly unlock it.";

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
}
