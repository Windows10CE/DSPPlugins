using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace DSPCheats.Cheats
{
    public class UnlockInstantly : Cheat
    {
        public override string GetCheatName() => "UnlockInstantly";
        public override string GetCheatDesc() => "Starting a research will instantly unlock it.";

        public static GameObject UnlockAllButton;

        public UnlockInstantly() : base()
        {
            if (!UnlockAllButton)
            {
                UnlockAllButton = GameObject.Instantiate(DSPCheatsPlugin.DSPCheatsAssets.LoadAsset<GameObject>("Assets/New Assets/UnlockAllButton.prefab"));
                UnlockAllButton.GetComponent<Button>().image.sprite = Resources.Load<Sprite>("ui/textures/sprites/sci-fi/window-content-4");
                UnlockAllButton.GetComponentInChildren<Text>().font = Resources.Load<Font>("ui/fonts/SAIRAB");
                UnlockAllButton.GetComponent<Button>().onClick.AddListener(UnlockAll);
            }
        }

        public static void UnlockAll()
        {
            foreach (TechProto tech in LDB.techs.dataArray.Where(x => x.Published))
            {
                if (!GameMain.history.TechUnlocked(tech.ID))
                    UnlockTechRecursive(tech.ID, GameMain.history);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITechTree), "_OnInit")]
        public static void TechTreeInitPostfix(ref UITechTree __instance)
        {
            UnlockAllButton.transform.SetParent(__instance.transform);
            UnlockAllButton.SetActive(true);
            var newPos = __instance.tabButton1.transform.position;
            newPos.x += 2.5f;
            newPos.y -= 0.2f;
            UnlockAllButton.transform.position = newPos;
            UnlockAllButton.transform.localScale = __instance.tabButton1.transform.localScale;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITechNode), "DoStartTech")]
        public static void StartTechPostfix(ref UITechNode __instance)
        {
            if ((__instance.techProto?.ID ?? 1) == 1)
                return;
            UnlockTechRecursive(__instance.techProto.ID, GameMain.history);
            GameMain.history.DequeueTech();
        }

        private static void UnlockTechRecursive(int techId, GameHistoryData history)
        {
            TechState state = history.TechState(techId);
            TechProto proto = LDB.techs.Select(techId);

            foreach (var techReq in proto.PreTechs)
            {
                if (!history.TechState(techReq).unlocked)
                    UnlockTechRecursive(techReq, history);
            }
            foreach (var techReq in proto.PreTechsImplicit)
            {
                if (!history.TechState(techReq).unlocked)
                    UnlockTechRecursive(techReq, history);
            }
            foreach (var itemReq in proto.itemArray)
            {
                if (itemReq.preTech is not null && !history.TechState(itemReq.preTech.ID).unlocked)
                    UnlockTechRecursive(itemReq.preTech.ID, history);
            }

            int current = state.curLevel;
            for (; current < state.maxLevel; current++)
                for (int j = 0; j < proto.UnlockFunctions.Length; j++)
                    history.UnlockTechFunction(proto.UnlockFunctions[j], proto.UnlockValues[j], current);

            history.UnlockTech(techId);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameHistoryData), "PreTechUnlocked")]
        public static bool PreTechUnlockedPrefix(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameHistoryData), "ImplicitPreTechRequired")]
        public static bool ImplicitPretechReqPrefix(ref int __result)
        {
            __result = 0;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameHistoryData), "CanEnqueueTech")]
        public static bool CanEnqueueTechPrefix(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameHistoryData), "CanEnqueueTechIgnoreFull")]
        public static bool CanEnqueueTechIgnoreFullPrefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}
