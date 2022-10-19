﻿using System;
using JetBrains.Annotations;
using static FeatureDefinitionAttributeModifier;

namespace SolastaUnfinishedBusiness.Builders.Features;

[UsedImplicitly]
internal class FeatureDefinitionAttributeModifierBuilder : DefinitionBuilder<FeatureDefinitionAttributeModifier,
    FeatureDefinitionAttributeModifierBuilder>
{
    internal FeatureDefinitionAttributeModifierBuilder SetModifier(
        AttributeModifierOperation modifierType,
        string attribute,
        int amount)
    {
        Definition.modifierOperation = modifierType;
        Definition.modifiedAttribute = attribute;
        Definition.modifierValue = amount;
        return this;
    }

    internal FeatureDefinitionAttributeModifierBuilder SetModifierAbilityScore(
        string attribute,
        string abilityScore,
        bool minimum1 = true)
    {
        Definition.modifierOperation = AttributeModifierOperation.AddAbilityScoreBonus;
        Definition.modifiedAttribute = attribute;
        Definition.modifierAbilityScore = abilityScore;
        Definition.minimum1 = minimum1;
        return this;
    }
    
    internal FeatureDefinitionAttributeModifierBuilder SetDexPlusAbilityScore(
        string attribute,
        string abilityScore,
        int value = 10,
        bool minimum1 = true)
    {
        Definition.modifierOperation = AttributeModifierOperation.SetWithDexPlusOtherAbilityScoreBonusIfBetter;
        Definition.modifiedAttribute = attribute;
        Definition.modifierAbilityScore = abilityScore;
        Definition.modifierValue = value;
        Definition.minimum1 = minimum1;
        return this;
    }

    internal FeatureDefinitionAttributeModifierBuilder SetSituationalContext(
        RuleDefinitions.SituationalContext situationalContext)
    {
        Definition.situationalContext = situationalContext;
        return this;
    }

    #region Constructors

    protected FeatureDefinitionAttributeModifierBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    protected FeatureDefinitionAttributeModifierBuilder(FeatureDefinitionAttributeModifier original, string name,
        Guid namespaceGuid) : base(original, name, namespaceGuid)
    {
    }

    #endregion
}
