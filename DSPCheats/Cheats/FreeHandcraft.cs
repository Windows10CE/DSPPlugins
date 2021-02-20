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
        private ILHook _okButtonHook = new ILHook(typeof(UIReplicatorWindow).GetMethod("OnOkButtonClick", BindingFlags.NonPublic | BindingFlags.Instance), new ILContext.Manipulator(OkButtonHandcraftILHook), new ILHookConfig
        {
            ManualApply = true
        });
        private ILHook _refreshIconsHook = new ILHook(typeof(UIReplicatorWindow).GetMethod("RefreshRecipeIcons", BindingFlags.NonPublic | BindingFlags.Instance), new ILContext.Manipulator(RefreshIconsILHook), new ILHookConfig
        {
            ManualApply = true
        });
        private ILHook _replicatorWindowUpdateHook = new ILHook(typeof(UIReplicatorWindow).GetMethod("_OnUpdate", BindingFlags.NonPublic | BindingFlags.Instance), new ILContext.Manipulator(ReplicatorUpdateILHook), new ILHookConfig
        {
            ManualApply = true
        });
        private bool _currentlyHooked = false;

        public override void ChangeState(bool enable)
        {
            if (enable != _currentlyHooked)
            {
                if (enable)
                {
                    _takeItemsHook.Apply();
                    _okButtonHook.Apply();
                    _refreshIconsHook.Apply();
                    _replicatorWindowUpdateHook.Apply();
                }
                else
                {
                    _takeItemsHook.Undo();
                    _okButtonHook.Undo();
                    _refreshIconsHook.Undo();
                    _replicatorWindowUpdateHook.Undo();
                }
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

        public static void OkButtonHandcraftILHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel placeToSkipTo = null;

            c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(UIReplicatorWindow).GetField("selectedRecipe", BindingFlags.NonPublic | BindingFlags.Instance)),
                x => x.MatchLdfld(typeof(RecipeProto).GetField("Handcraft", BindingFlags.Public | BindingFlags.Instance)),
                x => x.MatchBrtrue(out placeToSkipTo)
            );

            c.Emit(OpCodes.Br, placeToSkipTo);
        }

        public static void RefreshIconsILHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int handcraftLoc = 7;

            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(1),
                x => x.MatchLdloc(3),
                x => x.MatchLdelemRef(),
                x => x.MatchLdfld(typeof(RecipeProto).GetField("Handcraft", BindingFlags.Public | BindingFlags.Instance)),
                x => x.MatchStloc(out handcraftLoc)
            );

            c.Emit(OpCodes.Ldc_I4_1);
            c.Emit(OpCodes.Stloc, handcraftLoc);
        }

        public static void ReplicatorUpdateILHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(UIReplicatorWindow).GetField("selectedRecipe", BindingFlags.NonPublic | BindingFlags.Instance)),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(UIReplicatorWindow).GetField("selectedRecipe", BindingFlags.NonPublic | BindingFlags.Instance)),
                x => x.MatchLdfld(typeof(RecipeProto).GetField("Handcraft", BindingFlags.Public | BindingFlags.Instance)),
                x => x.MatchBrfalse(out _)
            );

            c.Index += 3;
            c.RemoveRange(4);
        }
    }
}
