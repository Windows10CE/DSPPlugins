using System.Reflection;
using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;

namespace DSPCheats.Cheats
{
    class FreeHandcraft : Cheat
    {
        public override string GetCheatName() => "FreeHandcraft";
        public override string GetCheatDesc() => "Craft things in the replicator for free.";

        private ILHook _takeItemsHook = new ILHook(typeof(MechaForge).GetMethod("AddTaskIterate", BindingFlags.NonPublic | BindingFlags.Instance), new ILContext.Manipulator(DontTakeItemsILHook), new ILHookConfig
        {
            ManualApply = true
        });
        private bool _currentlyHooked = false;

        public override void ChangeState(bool enable)
        {
            if (enable != _currentlyHooked)
            {
                if (enable)
                    _takeItemsHook.Apply();
                else
                    _takeItemsHook.Undo();
                _currentlyHooked = enable;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MechaForge), "PredictTaskCount")]
        public static bool MechaForgePredictTaskCountPrefix(ref int __result, ref int maxShowing)
        {
            __result = maxShowing;
            return false;
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
}
