using BepInEx.Configuration;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;

namespace DSPCheats.Cheats
{
    class MechaSpeedMultiplierCheat : Cheat
    {
        public override string GetCheatName() => "MechaSpeedMultiplier";
        public override string GetCheatDesc() => "Increases the mecha's speed by a multiplier";

        public static readonly ConfigEntry<float> MultiplierConfig = DSPCheatsPlugin.Config.Bind<float>("MechaSpeedMultiplier", "Multiplier", 1.0f, "What the mecha's speed should be multiplied by.");

        private static ILHook _changeWalkSpeedHook = new ILHook(typeof(PlayerMove_Walk).GetMethod("GameTick"), new ILContext.Manipulator(IncreaseMechaSpeedIL));
        private static bool _shouldChange = false;

        public override void ChangeState(bool enable)
        {
            _shouldChange = enable;
            _changeWalkSpeedHook.Dispose();
            _changeWalkSpeedHook = new ILHook(typeof(PlayerMove_Walk).GetMethod("GameTick"), new ILContext.Manipulator(IncreaseMechaSpeedIL));
        }

        public static void IncreaseMechaSpeedIL(ILContext il)
        {
            if (!_shouldChange)
                return;
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchLdfld(typeof(Mecha).GetField("walkSpeed"))
            );

            c.Emit(OpCodes.Ldc_R4, MultiplierConfig.Value);
            c.Emit(OpCodes.Mul);
        }
    }
}
