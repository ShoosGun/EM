using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace EM
{
    [BepInPlugin("locochoco.OWA.EnhancedMallows", "Enhanced Mallows", "1.0.0")]
    [BepInProcess("OuterWilds_Alpha_1_2.exe")]
    [HarmonyPatch]
    public class EnhancedMallows : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmonyInstance = new Harmony("com.locochoco.EnhancedMallows");
            harmonyInstance.PatchAll();
        }


        public static void DefaultMallowScale(Renderer mallowRender)
        {
            mallowRender.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        public static void ChangeMallowScale(Renderer mallowRender, float reductionFactor)
        {
            mallowRender.transform.localScale = new Vector3(4 - 4 * reductionFactor, 4 - 4 * reductionFactor, 4 - 4 * reductionFactor);
        }

        public static FieldInfo mallowRenderer = AccessTools.Field(typeof(Marshmallow), "_mallowRenderer");
        public static FieldInfo toastLevel = AccessTools.Field(typeof(Marshmallow), "_toastLevel");
        public static MethodInfo changeMallowScale = AccessTools.Method(typeof(EnhancedMallows), "ChangeMallowScale");
        public static MethodInfo defaultMallowScale = AccessTools.Method(typeof(EnhancedMallows), "DefaultMallowScale");


        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Marshmallow),"Update")]
        public static IEnumerable<CodeInstruction> Marshmallow_Update(IEnumerable<CodeInstruction> instructions)
        {
            int index = -1;
            var codes = new List<CodeInstruction>(instructions);

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i + 1].opcode == OpCodes.Add && codes[i + 2].opcode == OpCodes.Stfld)//ldc.r4
                {
                    index = i + 3;
                    break;
                }
            }

            if (index > -1)
            {
                codes.Insert(index, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldfld, mallowRenderer));
                codes.Insert(index + 2, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(index + 3, new CodeInstruction(OpCodes.Ldfld, toastLevel));
                codes.Insert(index + 4, new CodeInstruction(OpCodes.Call, changeMallowScale));
            }

            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Marshmallow), "ResetMarshmallow")]
        public static IEnumerable<CodeInstruction> Marshmallow_ResetMarshmallow(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index1 = -1;
            int index2 = codes.Count - 1;

            //Find where to put the instructions
            for (var i = 0; i < index2 + 1; i++)
            {
                if (codes[i].opcode == OpCodes.Ret)
                {
                    index1 = i - 1;
                    break;
                }
            }

            //If found put them there
            if (index1 > -1 && index2 > -1)
            {
                codes.Insert(index1, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(index1 + 1, new CodeInstruction(OpCodes.Ldfld, mallowRenderer));
                codes.Insert(index1 + 2, new CodeInstruction(OpCodes.Call, defaultMallowScale));

                codes.Insert(index2, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(index2 + 1, new CodeInstruction(OpCodes.Ldfld, mallowRenderer));
                codes.Insert(index2 + 2, new CodeInstruction(OpCodes.Call, defaultMallowScale));
            }

            return codes;
        }

    }
}
