﻿using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.CustomInterfaces;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

public static class GlobalUniqueEffects
{
    public enum Group
    {
        None, Familiar, InventorSpellStoringItem, GrenadierGrenadeMode, WildMasterBeast, ArtilleristCannon
    }

    private static readonly Dictionary<Group, HashSet<BaseDefinition>> Groups = new();

    private static HashSet<BaseDefinition> GetGroup(Group group)
    {
        if (Groups.TryGetValue(group, out var value))
        {
            return value;
        }

        var newGroup = new HashSet<BaseDefinition>();
        Groups.Add(group, newGroup);

        return newGroup;
    }

    /**Returns copies*/
    private static (Group, HashSet<BaseDefinition>) GetSameGroupItems(BaseDefinition definition)
    {
        var result = new HashSet<BaseDefinition>();

        var group = Groups.FirstOrDefault(e => e.Value.Contains(definition));

        if (group.Value == null)
        {
            return (Group.None, result);
        }

        foreach (var item in group.Value)
        {
            result.Add(item);
        }

        return (group.Key, result);
    }

    internal static void AddToGroup(Group group, [NotNull] params BaseDefinition[] definitions)
    {
        foreach (var definition in definitions)
        {
            GetGroup(group).Add(definition);
        }
    }

    internal static void EnforceLimitedInstancePower(CharacterActionUsePower action)
    {
        var power = action.activePower.PowerDefinition;

        var limiter = power.GetFirstSubFeatureOfType<ILimitEffectInstances>();

        if (limiter == null)
        {
            return;
        }

        var character = action.ActingCharacter.RulesetCharacter;
        var effects = GetLimitedPowerEffects(character, limiter);

        effects.Sort((a, b) => a.Guid.CompareTo(b.Guid));

        var limit = limiter.GetLimit(character);
        var remove = effects.Count - limit;

        for (var i = 0; i < remove; i++)
        {
            character.TerminatePower(effects[i]);
        }
    }

    private static List<RulesetEffectPower> GetLimitedPowerEffects(
        RulesetEntity character,
        ILimitEffectInstances limit)
    {
        return EffectHelpers.GetAllEffectsBySourceGuid(character.Guid)
            .OfType<RulesetEffectPower>()
            .Where(powerEffect =>
            {
                var tmp = powerEffect.PowerDefinition.GetFirstSubFeatureOfType<ILimitEffectInstances>();

                if (tmp == null)
                {
                    return false;
                }

                return tmp.Name == limit.Name;
            })
            .ToList();
    }

    /**
     * Used in the patch to terminate all matching powers and spells of same group
     */
    internal static void TerminateMatchingUniqueEffect(RulesetCharacter character, RulesetEffect uniqueEffect)
    {
        var (groupKey, group) = GetSameGroupItems(uniqueEffect.SourceDefinition);

        if (uniqueEffect is RulesetEffectPower { PowerDefinition.UniqueInstance: true }
            or RulesetEffectSpell { SpellDefinition.UniqueInstance: true })
        {
            //ensure we try to properly terminate unique effects not in groups
            group.Add(uniqueEffect.SourceDefinition);
        }

        var allSubDefinitions = new HashSet<BaseDefinition>();

        foreach (var definition in group)
        {
            allSubDefinitions.Add(definition);

            switch (definition)
            {
                case FeatureDefinitionPower power:
                {
                    var bundles = PowerBundle.GetMasterPowersBySubPower(power);

                    foreach (var subPower in bundles.Select(PowerBundle.GetBundle)
                                 .Where(bundle => bundle.TerminateAll)
                                 .SelectMany(bundle => bundle.SubPowers))
                    {
                        allSubDefinitions.Add(subPower);
                    }

                    break;
                }
                case SpellDefinition spell:
                {
                    foreach (var allElement in DatabaseRepository.GetDatabase<SpellDefinition>())
                    {
                        if (!spell.IsSubSpellOf(allElement))
                        {
                            continue;
                        }

                        foreach (var subSpell in allElement.SubspellsList)
                        {
                            allSubDefinitions.Add(subSpell);
                        }
                    }

                    break;
                }
            }
        }

        var limit = character.GetSubFeaturesByType<IChangeGlobalUniqueEffectsLimit>()
            .Where(x => x.GroupKey == groupKey)
            .Select(x => x.Limit)
            .AddItem(0)
            .Max();

        var effects = EffectHelpers.GetAllEffectsBySourceGuid(character.Guid)
            .Where(e => e != uniqueEffect && allSubDefinitions.Contains(e.SourceDefinition))
            .ToList();

        for (var i = 0; i < effects.Count - limit; i++)
        {
            effects[i].DoTerminate(character);
        }
    }
}
