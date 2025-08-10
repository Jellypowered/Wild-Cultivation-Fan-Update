// RWC_NoDuplicateAdd_WithLogging.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RWC_Code
{
    [HarmonyPatch(typeof(WildPlantSpawner), "CachePlantCommonalitiesIfShould")]
    [HarmonyAfter(new[] { "m00nl1ght.GeologicalLandforms" })]
    [HarmonyPriority(Priority.VeryLow)]
    public static class RWC_NoDuplicateAdd_WithLogging
    {
        private static readonly MethodInfo AddMI =
            typeof(Dictionary<ThingDef, float>).GetMethod("Add", new[] { typeof(ThingDef), typeof(float) });

        private static readonly MethodInfo HelperMI =
            typeof(RWC_NoDuplicateAdd_WithLogging).GetMethod(nameof(AddOrReplaceWithLog),
                BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly FieldInfo MapFI =
            AccessTools.Field(typeof(WildPlantSpawner), "map");

        // Optional context cache
        private static FieldInfo _wildPlantsField;   // BiomeDef.wildPlants (list of BiomePlantRecord)
        private static FieldInfo _bprPlantField;     // BiomePlantRecord.plant
        private static readonly HashSet<string> _reportedOnce = new HashSet<string>();

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ci in instructions)
            {
                var mi = ci.operand as MethodInfo;

                if ((ci.opcode == OpCodes.Callvirt || ci.opcode == OpCodes.Call) && mi == AddMI)
                {
                    // Stack before: [..., dict, key, value]
                    // Need to push 'this' (WildPlantSpawner) => 4th arg
                    var ldThis = new CodeInstruction(OpCodes.Ldarg_0);
                    // preserve labels/blocks from the original call on the first new inst
                    if (ci.labels != null && ci.labels.Count > 0) ldThis.labels.AddRange(ci.labels);
                    if (ci.blocks != null && ci.blocks.Count > 0) ldThis.blocks.AddRange(ci.blocks);

                    yield return ldThis;
                    var callHelper = new CodeInstruction(OpCodes.Call, HelperMI);
                    yield return callHelper;
                }
                else
                {
                    yield return ci;
                }
            }
        }

        // Replacement for Dictionary.Add: log once (dev mode), then assign with indexer
        private static void AddOrReplaceWithLog(Dictionary<ThingDef, float> dict, ThingDef key, float value, WildPlantSpawner spawner)
        {
            bool isDup = dict.ContainsKey(key);

            if (isDup && Prefs.DevMode)
            {
                try
                {
                    Map map = (spawner != null && MapFI != null) ? (Map)MapFI.GetValue(spawner) : null;
                    string biomeName = (map != null && map.Biome != null) ? map.Biome.defName : "unknown-biome";
                    string plantName = (key != null) ? key.defName : "null-plant";

                    string sig = biomeName + "|" + plantName;
                    if (_reportedOnce.Add(sig))
                    {
                        // Try to count how many raw entries exist in BiomeDef.wildPlants for this plant
                        int occurrences = -1;
                        try
                        {
                            if (map != null && map.Biome != null)
                            {
                                if (_wildPlantsField == null)
                                    _wildPlantsField = typeof(BiomeDef).GetField("wildPlants",
                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                                IList list = (_wildPlantsField != null) ? (_wildPlantsField.GetValue(map.Biome) as IList) : null;
                                if (list != null && list.Count > 0)
                                {
                                    object sample = list[0];
                                    if (sample != null && _bprPlantField == null)
                                        _bprPlantField = sample.GetType().GetField("plant",
                                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                                    int count = 0;
                                    if (_bprPlantField != null)
                                    {
                                        for (int i = 0; i < list.Count; i++)
                                        {
                                            object rec = list[i];
                                            ThingDef p = (rec != null) ? (_bprPlantField.GetValue(rec) as ThingDef) : null;
                                            if (p == key) count++;
                                        }
                                    }
                                    occurrences = count;
                                }
                            }
                        }
                        catch { /* best effort only */ }

                        // Only build the stack string when we actually log
                        string stack = Environment.StackTrace;
                        if (dict.ContainsKey(key) && Prefs.DevMode)
                        {
                            Log.Warning("[WildCultivation] Duplicate wild plant during cache: " +
                                    plantName + " in " + biomeName +
                                    (occurrences >= 0 ? (" (raw biome entries: " + occurrences + ")") : "") +
                                    "\nOnce-per-plant/biome notice. Stack:\n" + stack);
                        }
                    }
                }
                catch
                {
                    // Never let logging break gen
                }
            }

            // Safe write-through (handles dupes by overwrite)
            dict[key] = value;
        }
    }
}
