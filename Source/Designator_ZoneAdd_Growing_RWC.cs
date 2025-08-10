using HarmonyLib;
using RimWorld;
using Verse;

namespace RWC_Code
{
    [HarmonyAfter(new[] {
    "net.quadric.biomes.core", "rim.world.realisticplanets", "m00nl1ght.GeologicalLandforms"/* etc if you know IDs */
})]
    [HarmonyPriority(Priority.VeryLow)]
    [HarmonyPatch(typeof(Designator_ZoneAdd_Growing))]
    [HarmonyPatch(nameof(Designator_ZoneAdd_Growing.CanDesignateCell))]
    public static class Designator_ZoneAdd_Growing_RWC
    {
        [HarmonyPostfix]
        public static void Postfix(Designator_ZoneAdd_Growing __instance, IntVec3 c, ref AcceptanceReport __result)
        {
            if (__result.Accepted) return;

            var map = __instance.Map;
            if (map == null || !c.InBounds(map)) return;

            var terrain = map.terrainGrid.TerrainAt(c);
            if (terrain == null) return;

            // Respect obvious no-sow terrains
            if (terrain.IsWater) return;
            if (terrain.passability == Traversability.Impassable) return;

            // Your low-fertility allowance
            const float MinFertility = 0.01f;
            float fert = map.fertilityGrid.FertilityAt(c);

            if (fert >= MinFertility)
            {
                __result = true; // accept locally without mutating defs
            }
        }
    }
}

