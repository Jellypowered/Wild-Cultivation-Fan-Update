using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RWC_Code
{
    public class WildCultivationMod : Mod
    {
        public WildCultivationMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("net.saucypigeon.rimworld.mod.wildcultivation");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Run after all defs are loaded
            LongEventHandler.ExecuteWhenFinished(Setup);
        }


        private static void Setup()
        {
            TryAlignWildHarvest("PlantWildCotton", RWC_ThingDefOf.Plant_Cotton);
            TryAlignWildHarvest("PlantWildDevilstrand", RWC_ThingDefOf.Plant_Devilstrand);
        }

        private static void TryAlignWildHarvest(string wildDefName, ThingDef cultivated)
        {
            var wild = DefDatabase<ThingDef>.GetNamedSilentFail(wildDefName);
            if (wild?.plant == null || cultivated?.plant == null) return;

            bool changed = false;

            if (wild.plant.harvestedThingDef == null)
            {
                wild.plant.harvestedThingDef = cultivated.plant.harvestedThingDef;
                changed = true;
            }

            if (wild.plant.harvestYield <= 0.0001f)
            {
                wild.plant.harvestYield = cultivated.plant.harvestYield;
                changed = true;
            }

            if (changed && Prefs.DevMode)
                Log.Message($"[WildCultivation] Aligned {wildDefName} harvest -> {cultivated.defName}");
        }

    }
}
