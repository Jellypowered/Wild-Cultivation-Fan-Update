using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RWC_Code
{
    [HarmonyPatch(typeof(Designator_ZoneAdd_Growing))]
    [HarmonyPatch(nameof(Designator_ZoneAdd_Growing.CanDesignateCell))]
    public static class Designator_ZoneAdd_Growing_RWC
    {
        [HarmonyPrefix]
        public static bool CanDesignateCellPrefix(ref float __state)
        {
            __state = ThingDefOf.Plant_Potato.plant.fertilityMin;
            ThingDefOf.Plant_Potato.plant.fertilityMin = 0.01f;
            return true;
        }

        [HarmonyPostfix]
        public static void CanDesignateCellPostfix(ref float __state)
        {
            ThingDefOf.Plant_Potato.plant.fertilityMin = __state;
        }
    }
}
