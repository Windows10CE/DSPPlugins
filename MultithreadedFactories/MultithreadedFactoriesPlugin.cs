using System;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace MultithreadedFactories
{
    [BepInPlugin("com.Windows10CE.MultithreadedFactories", "MultithreadedFactories", "1.0.0")]
    public class MultithreadedFactoriesPlugin : BaseUnityPlugin
    {
        new internal static ManualLogSource Logger;

        public void Awake()
        {
            MultithreadedFactoriesPlugin.Logger = base.Logger;

            _ = new ILHook(typeof(GameData).GetMethod("GameTick"), ParallelizeFactoryGameTick);
        }

        public static void ParallelizeFactoryGameTick(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int forIndex = 3;
            var factoriesFld = typeof(GameData).GetField(nameof(GameData.factories), BindingFlags.Public | BindingFlags.Instance);

            Func<Instruction, bool>[] searchArray = new Func<Instruction, bool>[]
            {
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(out forIndex),
                x => x.MatchBr(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(factoriesFld),
                x => x.MatchLdloc(forIndex),
                x => x.MatchLdelemRef(),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(factoriesFld),
                x => x.MatchLdloc(forIndex),
                x => x.MatchLdelemRef(),
                x => x.MatchLdarg(1),
                x => x.MatchCallOrCallvirt(typeof(PlanetFactory).GetMethod(nameof(PlanetFactory.GameTick), BindingFlags.Public | BindingFlags.Instance)),
                x => x.MatchLdloc(forIndex),
                x => x.MatchLdcI4(1),
                x => x.MatchAdd(),
                x => x.MatchStloc(forIndex),
                x => x.MatchLdloc(forIndex),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(GameData).GetField(nameof(GameData.factoryCount), BindingFlags.Public | BindingFlags.Instance)),
                x => x.MatchBlt(out _)
            };

            c.GotoNext(MoveType.Before, searchArray);

            var retLabel = c.MarkLabel();
            c.Index -= 1;

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);

            c.EmitDelegate<Func<GameData, long, bool>>((data, time) =>
            {
                bool flag = false;
                int currentTech = data.history.currentTech;
                TechProto techProto = LDB.techs.Select(currentTech);
                TechState techState = default(TechState);
                if (currentTech > 0 && techProto != null && techProto.IsLabTech && GameMain.history.techStates.ContainsKey(currentTech))
                {
                    techState = data.history.techStates[currentTech];
                    flag = true;
                }
                if (flag && Math.Abs(techState.hashUploaded - techState.hashNeeded) < 7200L)
                {
                    return true;
                }
                else
                {
                    Parallel.ForEach(data.factories, (factory) => { if (factory is not null) factory.GameTick(time); });

                    return false;
                }
            });

            c.Index += searchArray.Length;
            var branchLabel = c.MarkLabel();

            c.GotoLabel(retLabel, MoveType.Before);

            c.Emit(OpCodes.Brfalse, branchLabel);
        }
    }
}
