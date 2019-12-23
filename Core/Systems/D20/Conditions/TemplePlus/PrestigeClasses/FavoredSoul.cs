using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class FavoredSoul
    {
        private static readonly Stat ClassId = Stat.level_favored_soul;

        public const string AcidResistance = "Favored Soul Acid Resistance";
        public static readonly FeatId AcidResistanceId = (FeatId) ElfHash.Hash(AcidResistance);

        public const string ColdResistance = "Favored Soul Cold Resistance";
        public static readonly FeatId ColdResistanceId = (FeatId) ElfHash.Hash(ColdResistance);

        public const string ElectricityResistance = "Favored Soul Electricity Resistance";
        public static readonly FeatId ElectricityResistanceId = (FeatId) ElfHash.Hash(ElectricityResistance);

        public const string FireResistance = "Favored Soul Fire Resistance";
        public static readonly FeatId FireResistanceId = (FeatId) ElfHash.Hash(FireResistance);

        public const string SonicResistance = "Favored Soul Sonic Resistance";
        public static readonly FeatId SonicResistanceId = (FeatId) ElfHash.Hash(SonicResistance);

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec
        {
            classEnum = ClassId,
            helpTopic = "TAG_FAVORED_SOULS",
            conditionName = "Favored Soul",
            flags = ClassDefinitionFlag.CDF_BaseClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
            hitDice = 8,
            FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
            ReflexSaveProgression = SavingThrowProgressionType.HIGH,
            WillSaveProgression = SavingThrowProgressionType.HIGH,
            skillPts = 2,
            spellListType = SpellListType.Clerical,
            hasArmoredArcaneCasterFeature = false,
            spellMemorizationType = SpellReadyingType.Innate,
            spellSourceType = SpellSourceType.Divine,
            spellCastingConditionName = null,
            spellsPerDay = new Dictionary<int, IImmutableList<int>>
            {
                [1] = ImmutableList.Create(5, 3),
                [2] = ImmutableList.Create(6, 4),
                [3] = ImmutableList.Create(6, 5),
                [4] = ImmutableList.Create(6, 6, 3),
                [5] = ImmutableList.Create(6, 6, 4),
                [6] = ImmutableList.Create(6, 6, 5, 3),
                [7] = ImmutableList.Create(6, 6, 6, 4),
                [8] = ImmutableList.Create(6, 6, 6, 5, 3),
                [9] = ImmutableList.Create(6, 6, 6, 6, 4),
                [10] = ImmutableList.Create(6, 6, 6, 6, 5, 3),
                [11] = ImmutableList.Create(6, 6, 6, 6, 6, 4),
                [12] = ImmutableList.Create(6, 6, 6, 6, 6, 5, 3),
                [13] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 4),
                [14] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 5, 3),
                [15] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 4),
                [16] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 5, 3),
                [17] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 4),
                [18] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 5, 3),
                [19] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 4),
                [20] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 6)
            }.ToImmutableDictionary(),
            classSkills = new HashSet<SkillId>
            {
                SkillId.concentration,
                SkillId.diplomacy,
                SkillId.heal,
                SkillId.sense_motive,
                SkillId.spellcraft,
                SkillId.alchemy,
                SkillId.craft,
                SkillId.knowledge_arcana,
                SkillId.profession,
            }.ToImmutableHashSet(),
            classFeats = new Dictionary<FeatId, int>
            {
                {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                {FeatId.SHIELD_PROFICIENCY, 1},
                {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                {FeatId.DOMAIN_POWER, 1},
                // TODO: Investigate these feats
                {(FeatId) ElfHash.Hash("Deity's Weapon Focus"), 1},
                {(FeatId) ElfHash.Hash("Energy Resistance (Favored Soul)"), 1},
                {(FeatId) ElfHash.Hash("Deity's Weapon Specialization"), 1},
                {(FeatId) ElfHash.Hash("Damage Reduction (Favored Soul)"), 1}
            }.ToImmutableDictionary(),
            deityClass = Stat.level_cleric
        };

        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .AddHandler(DispatcherType.GetBaseCasterLevel, OnGetBaseCasterLevel)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Activate,
                OnInitLevelupSpellSelection)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize,
                OnLevelupSpellsFinalize)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Check_Complete,
                OnLevelupSpellsCheckComplete)
            .AddHandler(DispatcherType.TakingDamage2, FavoredSoulDR)
            .Build();

        // Spell casting
        public static void OnGetBaseCasterLevel(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 != ClassId)
            {
                return;
            }

            var classLvl = evt.objHndCaller.GetStat(ClassId);
            dispIo.bonlist.AddBonus(classLvl, 0, 137);
            return;
        }

        public static void OnInitLevelupSpellSelection(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 != ClassId)
            {
                return;
            }

            throw new NotImplementedException();
            // classSpecModule.InitSpellSelection(evt.objHndCaller);
            return;
        }

        public static void OnLevelupSpellsCheckComplete(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 != ClassId)
            {
                return;
            }

            throw new NotImplementedException();
            // if (!classSpecModule.LevelupCheckSpells(evt.objHndCaller))
            // {
            //     dispIo.bonlist.AddBonus(-1, 0, 137); // denotes incomplete spell selection
            // }
        }

        public static void OnLevelupSpellsFinalize(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 != ClassId)
            {
                return;
            }

            throw new NotImplementedException();
            // classSpecModule.LevelupSpellsFinalize(evt.objHndCaller);
        }

        // Damage reduction
        public static void FavoredSoulDR(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var fav_soul_lvl = evt.objHndCaller.GetStat(ClassId);
            if (fav_soul_lvl < 20)
            {
                return;
            }

            var bonval = 10;
            var align = evt.objHndCaller.GetAlignment();
            if ((align.IsChaotic()))
            {
                dispIo.damage.AddPhysicalDR(bonval, D20AttackPower.COLD, 126); // DR 10/Cold Iron
            }
            else
            {
                dispIo.damage.AddPhysicalDR(bonval, D20AttackPower.SILVER, 126); // DR 10/Silver
            }

            // skipped implementing the choice for neutrals, partly because there's no Cold Iron in ToEE (and Silver is pretty rare too)
            return;
        }

        #region Energy Resistance

        public static void FavSoulEnergyRes(in DispatcherCallbackArgs evt, DamageType damageType)
        {
            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddDR(10, damageType, 124);
        }

        [FeatCondition(AcidResistance)]
        public static readonly ConditionSpec EnergyResistanceAcidCondition = ConditionSpec
            .Create("Favored Soul Acid Resistance", 3)
            .SetUnique()
            .AddHandler(DispatcherType.TakingDamage, FavSoulEnergyRes, DamageType.Acid)
            .Build();

        [FeatCondition(ColdResistance)]
        public static readonly ConditionSpec EnergyResistanceColdCondition = ConditionSpec
            .Create("Favored Soul Cold Resistance", 3)
            .SetUnique()
            .AddHandler(DispatcherType.TakingDamage, FavSoulEnergyRes, DamageType.Cold)
            .Build();

        [FeatCondition(ElectricityResistance)]
        public static readonly ConditionSpec EnergyResistanceElectricityCondition = ConditionSpec
            .Create("Favored Soul Electricity Resistance", 3)
            .SetUnique()
            .AddHandler(DispatcherType.TakingDamage, FavSoulEnergyRes, DamageType.Electricity)
            .Build();

        [FeatCondition(FireResistance)]
        public static readonly ConditionSpec EnergyResistanceFireCondition = ConditionSpec
            .Create("Favored Soul Fire Resistance", 3)
            .SetUnique()
            .AddHandler(DispatcherType.TakingDamage, FavSoulEnergyRes, DamageType.Fire)
            .Build();

        [FeatCondition(SonicResistance)]
        public static readonly ConditionSpec EnergyResistanceSonicCondition = ConditionSpec
            .Create("Favored Soul Sonic Resistance", 3)
            .SetUnique()
            .AddHandler(DispatcherType.TakingDamage, FavSoulEnergyRes, DamageType.Sonic)
            .Build();

        #endregion
    }
}