using BepInEx.Configuration;
using HarmonyLib;

namespace DSPCheats.Cheats
{
    [HarmonyPatch]
    public abstract class Cheat
    {
        public abstract string GetCheatName();
        public abstract string GetCheatDesc();

        public ConfigEntry<bool> ConfigValue { get; private set; }
        
        public Cheat()
        {
            ConfigValue = DSPCheatsPlugin.Config.Bind<bool>("Cheat Toggles", GetCheatName(), false, GetCheatDesc());
        }

        public virtual void ChangeState(bool enable)
        {

        }
    }
}
