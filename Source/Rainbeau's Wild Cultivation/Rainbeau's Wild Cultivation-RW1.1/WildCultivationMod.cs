using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using System.Diagnostics;

namespace RWC_Code
{
	public class WildCultivationMod : Mod
	{
		public WildCultivationMod(ModContentPack content) : base(content)
		{
			const string Id = "net.saucypigeon.rimworld.mod.wildcultivation";
			var harmony = new Harmony(Id);
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			LongEventHandler.QueueLongEvent(Setup, "LibraryStartup", false, null);
        }

		private static void Setup()
		{
			// Update wild cotton harvest to standard cotton
			ThingDef.Named("PlantWildCotton").plant.harvestedThingDef = ThingDefOf.Plant_Cotton.plant.harvestedThingDef;
			ThingDef.Named("PlantWildCotton").plant.harvestYield = ThingDefOf.Plant_Cotton.plant.harvestYield;

			// Update wild devilstrand harvest to standard devilstrand
			ThingDef.Named("PlantWildDevilstrand").plant.harvestedThingDef = ThingDefOf.Plant_Devilstrand.plant.harvestedThingDef;
			ThingDef.Named("PlantWildDevilstrand").plant.harvestYield = ThingDefOf.Plant_Devilstrand.plant.harvestYield;
		}
	}
}
