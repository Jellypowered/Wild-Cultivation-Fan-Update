// RWC_DistinctAllWildPlants.cs
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using RimWorld;

namespace RWC_Code
{
    [HarmonyPatch(typeof(BiomeDef), "get_AllWildPlants")]
    [HarmonyAfter(new[] { "m00nl1ght.GeologicalLandforms" })]
    [HarmonyPriority(Priority.VeryLow)]
    public static class RWC_DistinctAllWildPlants
    {
        public static void Postfix(BiomeDef __instance, ref List<ThingDef> __result)
        {
            if (__result == null || __result.Count <= 1) return;

            // In-place distinct, keep first occurrence
            HashSet<ThingDef> seen = new HashSet<ThingDef>();
            for (int i = __result.Count - 1; i >= 0; i--)
            {
                ThingDef p = __result[i];
                if (p == null || !seen.Add(p))
                    __result.RemoveAt(i);
            }
        }
    }
}
