using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace DSPCheats.Cheats
{
    class NoVeinUsage : Cheat
    {
        public override string GetCheatName() => "NoVeinUsage";
        public override string GetCheatDesc() => "Miners no longer use up resources in a vein.";

        private static ILHook _removeVeinDecrementHook = new ILHook(typeof(MinerComponent).GetMethod("InternalUpdate"), new ILContext.Manipulator(RemoveVeinDecrememtIL), new ILHookConfig 
        {
            ManualApply = true
        });
        private bool _currentlyHooked = false;

        public override void ChangeState(bool enable)
        {
            if (enable != _currentlyHooked)
            {
                if (enable)
                    _removeVeinDecrementHook.Apply();
                else
                    _removeVeinDecrementHook.Undo();
                _currentlyHooked = enable;
            }
        }

        public static void RemoveVeinDecrememtIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(2),
                x => x.MatchLdloc(out _),
                x => x.MatchLdelema<VeinData>(),
                x => x.MatchDup(),
                x => x.MatchLdfld(typeof(VeinData).GetField("amount")),
                x => x.MatchLdcI4(1),
                x => x.MatchSub(),
                x => x.MatchStfld(typeof(VeinData).GetField("amount"))
            );

            c.RemoveRange(8);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(1),
                x => x.MatchCallOrCallvirt(typeof(PlanetFactory).GetProperty("planet").GetGetMethod()),
                x => x.MatchLdfld(typeof(PlanetData).GetField("veinAmounts")),
                x => x.MatchLdarg(2),
                x => x.MatchLdloc(1),
                x => x.MatchLdelema<VeinData>(),
                x => x.MatchLdfld(typeof(VeinData).GetField("type")),
                x => x.MatchLdelema<long>(),
                x => x.MatchDup(),
                x => x.MatchLdindI8(),
                x => x.MatchLdcI4(1),
                x => x.MatchConvI8(),
                x => x.MatchSub(),
                x => x.MatchStindI8(),
                x => x.MatchLdarg(1),
                x => x.MatchCallOrCallvirt(typeof(PlanetFactory).GetProperty("planet").GetGetMethod()),
                x => x.MatchLdfld(out _),
                x => x.MatchLdarg(2),
                x => x.MatchLdloc(1),
                x => x.MatchLdelema<VeinData>(),
                x => x.MatchLdfld(typeof(VeinData).GetField("groupIndex")),
                x => x.MatchLdelema<PlanetData.VeinGroup>(),
                x => x.MatchDup(),
                x => x.MatchLdfld(typeof(PlanetData.VeinGroup).GetField("amount")),
                x => x.MatchLdcI4(1),
                x => x.MatchConvI8(),
                x => x.MatchSub(),
                x => x.MatchStfld(typeof(PlanetData.VeinGroup).GetField("amount"))
            );

            var labelsToChange = c.IncomingLabels;

            c.RemoveRange(28);

            foreach (var label in labelsToChange)
                label.Target = c.Next;
        }
    }
}
