﻿using System.Linq;

namespace SolastaCommunityExpansion.Features;

public interface ICustomMissileDeflection
{
    public int GetDamageReduction(RulesetCharacter target, RulesetCharacter attacker);
}

public class CustomMissileDeflection : ICustomMissileDeflection
{
    public RuleDefinitions.DieType dieType = RuleDefinitions.DieType.D10;
    public int dieNumber = 1;
    public RuleDefinitions.AdvantageType advantage = RuleDefinitions.AdvantageType.None;
    public string attribute = AttributeDefinitions.Dexterity;
    public int proficiencyBonusMult = 0;
    public int characterLevelMult = 0;
    public int classLevelMult = 0;
    public string characterClass = null;


    public int GetDamageReduction(RulesetCharacter target, RulesetCharacter attacker)
    {
        var reduction = 0;

        for (var i = 0; i < dieNumber; i++)
        {
            reduction += RuleDefinitions.RollDie(dieType, advantage, out _, out _);
        }

        if (!string.IsNullOrEmpty(attribute))
        {
            var attr = target.GetAttribute(attribute, true);
            if (attr != null)
            {
                reduction += AttributeDefinitions.ComputeAbilityScoreModifier(attr.CurrentValue);
            }
        }

        var characterLevel = target.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue;

        if (characterLevelMult != 0)
        {
            reduction += characterLevel * characterLevelMult;
        }

        if (!string.IsNullOrEmpty(characterClass) && classLevelMult != 0 && target is RulesetCharacterHero hero)
        {
            var classLevel = hero.ClassesAndLevels.FirstOrDefault(e => e.Key.Name == characterClass).Value;
            reduction += classLevel * classLevelMult;
        }

        if (proficiencyBonusMult != 0)
        {
            reduction += AttributeDefinitions.ComputeProficiencyBonus(characterLevel) * proficiencyBonusMult;
        }


        return reduction;
    }
}