using System.IO;
using System.Reflection;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Dumper
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInProcess("DSPGAME.exe")]
    public class DumperPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.Windows10CE.Dumper";
        public const string ModName = "Dumper";
        public const string ModVer = "1.0.0";

        new internal static ManualLogSource Logger;
        internal static string PluginDir;
        internal static string PrefabDescDir;

        public void Awake()
        {
            DumperPlugin.Logger = base.Logger;

            PluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            PrefabDescDir = Path.Combine(PluginDir, "PrefabDescs");
            if (Directory.Exists(PrefabDescDir))
                Directory.Delete(PrefabDescDir, true);
            Directory.CreateDirectory(PrefabDescDir);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            foreach (var property in typeof(LDB).GetProperties(BindingFlags.Public | BindingFlags.Static).Where(x => x.PropertyType.BaseType.Name.Contains("ProtoSet")))
            {
                Proto[] protoArray = property.PropertyType.GetField("dataArray").GetValue(property.GetValue(null, new object[0])) as Proto[];

                string outputPath = Path.Combine(PluginDir, property.PropertyType.Name);
                if (Directory.Exists(outputPath))
                    Directory.Delete(outputPath, true);
                Directory.CreateDirectory(outputPath);

                foreach (Proto proto in protoArray)
                {
                    try
                    {
                        File.WriteAllText(Path.Combine(outputPath, proto.Name.Translate().ValidFileName()) + ".json", JsonUtility.ToJson(proto, true));
                    } catch { }
                }
            }
        }
    }

    [HarmonyPatch]
    public class PrefabDescDumper
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PrefabDesc), "ReadPrefab")]
        public static void ReadPrefabPostfix(ref PrefabDesc __instance)
        {
            File.WriteAllText(Path.Combine(DumperPlugin.PrefabDescDir, __instance.prefab.name) + ".json", JsonUtility.ToJson(__instance, true));
        }
    }
}
