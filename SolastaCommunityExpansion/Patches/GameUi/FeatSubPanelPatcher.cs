﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;

namespace SolastaCommunityExpansion.Patches.GameUi
{
    [HarmonyPatch(typeof(FeatSubPanel), "RuntimeLoaded")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class FeatSubPanell_RuntimeLoaded
    {
        internal static void Sort(List<FeatDefinition> relevantFeats)
        {
            relevantFeats?.Sort((a, b) => a.FormatTitle().CompareTo(b.FormatTitle()));
        }

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (Main.Settings.EnableFeatsSorting && instruction.opcode == OpCodes.Pop)
                {
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
                    System.Reflection.FieldInfo relevantFeatsField = typeof(FeatSubPanel).GetField("relevantFeats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    System.Reflection.MethodInfo sortMethod = typeof(FeatSubPanell_RuntimeLoaded).GetMethod("Sort", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, relevantFeatsField);
                    yield return new CodeInstruction(OpCodes.Call, sortMethod);
                }
            }
        }
    }
}
