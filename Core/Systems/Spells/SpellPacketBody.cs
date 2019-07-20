using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.Spells
{
    public struct SpellObj
    {
        public GameObjectBody obj;
        public int partySysId;
        public int field_C;
    }

    [Flags]
    public enum SpellAnimationFlag
    {
        SAF_UNK8 = 0x8,
        SAF_ID_ATTEMPTED = 0x10,
    }

    [Flags]
    public enum UiPickerIncFlags : ulong
    {
	    UIPI_None = 0,
	    UIPI_Self = 0x1,
	    UIPI_Other = 0x2,
	    UIPI_NonCritter = 0x4,
	    UIPI_Dead = 0x8,
	    UIPI_Undead = 0x10,
	    UIPI_Unconscious = 0x20,
	    UIPI_Hostile = 0x40,
	    UIPI_Friendly = 0x80,
	    UIPI_Potion = 0x100,
	    UIPI_Scroll = 0x200
    }

    [Flags]
    public enum UiPickerFlagsTarget : ulong
    {
	    None = 0,
	    Min = 0x1,
	    Max = 0x2,
	    Radius = 0x4,
	    Range = 0x8,
	    Exclude1st = 0x10,
	    Degrees = 0x20,
	    FixedRadius = 0x40,
	    Unknown80h = 0x80, // these are not supported by the spell rules importer, but apparently used in at least one place (the py cast_spell function)
	    LosNotRequired = 0x100
    }

    [Flags]
    public enum PickerStatusFlags
    {
	    PSF_OutOfRange = 0x1,
	    PSF_Invalid = 0x2
    }

    [Flags]
    public enum PickerResultFlags {
	    PRF_HAS_SINGLE_OBJ = 0x1,
	    PRF_HAS_MULTI_OBJ = 0x2,
	    PRF_HAS_LOCATION = 0x4,
	    PRF_UNK8 = 0x8,
	    PRF_CANCELLED = 0x10, // User pressed escape or RMB
	    PRF_HAS_SELECTED_OBJECT = 0x20,
    }

    public struct PickerResult
    {
	    public PickerResultFlags flags; // see PickerResultFlags
		public int field4;
		public GameObjectBody handle;
		public List<GameObjectBody> objList;
		public LocAndOffsets location;
		public float offsetz;
		public int fieldbc;
    }

    public class SpellPacketBody
    {
	    private static readonly ILogger Logger = new ConsoleLogger();

	    private const int INV_IDX_INVALID = 255;

        public int spellEnum;
        public int spellEnumOriginal; // used for spontaneous casting in order to debit the "original" spell
        public SpellAnimationFlag animFlags; // See SpellAnimationFlag
        public object pSthg;
        public GameObjectBody caster;
        public uint casterPartsysId;
        public int spellClass; // aka spellClass
        public int spellKnownSlotLevel; // aka spellLevel
        public int casterLevel;
        public int dc;
        public int numSpellObjs => spellObjs.Length;
        public GameObjectBody aoeObj;
        public SpellObj[] spellObjs = Array.Empty<SpellObj>();
        public uint orgTargetCount;
        public int targetCount => targetListHandles.Length;
        public GameObjectBody[] targetListHandles = Array.Empty<GameObjectBody>();
        public int[] targetListPartsysIds = Array.Empty<int>();
        public int projectileCount => projectiles.Length;
        public uint field_9C4;
        public GameObjectBody[] projectiles = Array.Empty<GameObjectBody>();
        public LocAndOffsets aoeCenter;
        public float aoeCenterZ; // TODO consolidate with aoeCenter

        public uint field_A04;

        public PickerResult pickerResult;
        public int duration;
        public int durationRemaining;
        public int spellRange;
        public uint savingThrowResult;

        // inventory index, used for casting spells from items e.g. scrolls; it is 0xFF for non-item spells
        public int invIdx;

        public MetaMagicData metaMagicData;
        public int spellId;
        public uint field_AE4;

        public SpellPacketBody()
        {
            Reset();
        }

        [TempleDllLocation(0x1008A350)]
        public void Reset()
        {
	        spellId = 0;
	        spellEnum = 0;
	        spellEnumOriginal = 0;
	        caster = null;
	        casterPartsysId = 0;
	        casterLevel = 0;
	        dc = 0;
	        animFlags = 0;
	        aoeCenter = LocAndOffsets.Zero;
	        aoeCenterZ = 0;
	        targetListHandles = Array.Empty<GameObjectBody>();
	        duration = 0;
	        durationRemaining = 0;
	        metaMagicData = new MetaMagicData();
	        spellClass = 0;
	        spellKnownSlotLevel = 0;

	        projectiles = Array.Empty<GameObjectBody>();

	        // TODO this.orgTargetListNumItems = 0;

	        aoeObj = null;

	        spellObjs = Array.Empty<SpellObj>();

	        spellRange = 0;
	        savingThrowResult = 0;
	        invIdx = INV_IDX_INVALID;

	        pickerResult = new PickerResult();
        }

        public bool IsVancian(){
	        if (GameSystems.Spell.IsDomainSpell(spellClass))
		        return true;

	        if (D20ClassSystem.IsVancianCastingClass(GameSystems.Spell.GetCastingClass(spellClass)))
		        return true;

	        return false;
        }

        public bool IsDivine(){
	        if (GameSystems.Spell.IsDomainSpell(spellClass))
		        return true;
	        var castingClass = GameSystems.Spell.GetCastingClass(spellClass);

	        if (D20ClassSystem.IsDivineCastingClass(castingClass))
		        return true;

	        return false;
        }

        [TempleDllLocation(0x10079550)]
        public void Debit()
        {
            // preamble
			if (caster == null) {
				Logger.Warn("SpellPacketBody.Debit() Null caster!");
				return;
			}

			if (IsItemSpell()) // this is handled separately
				return;

			var spellEnumDebited = this.spellEnumOriginal;

			// Spontaneous vs. Normal logging
			bool isSpont = (spellEnum != spellEnumOriginal) && spellEnumOriginal != 0;
			var spellName = GameSystems.Spell.GetSpellName(spellEnumOriginal);
			if (isSpont){
				Logger.Debug("Debiting Spontaneous casted spell. Original spell: {0}", spellName);
			} else	{
				Logger.Debug("Debiting casted spell {0}", spellName);
			}

			// Vancian spell handling - debit from the spells_memorized list
			if (IsVancian()){

				var numMem = caster.GetSpellArray(obj_f.critter_spells_memorized_idx).Count;
				var spellFound = false;
				for (var i = 0; i < numMem; i++){
					var spellMem = caster.GetSpell(obj_f.critter_spells_memorized_idx, i);
					spellMem.pad0 = (char) (spellMem.pad0 & 0x7F); // clear out metamagic indictor

					if (!GameSystems.Spell.IsDomainSpell(spellMem.classCode)){
						if (spellMem.spellEnum != spellEnumDebited)
							continue;
					}
					else if (spellMem.spellEnum != spellEnum){
						continue;
					}

					if (spellMem.spellLevel == spellKnownSlotLevel // todo: check if the spell level should be adjusted for MetaMagic
						&& spellMem.classCode == spellClass
						&& spellMem.spellStoreState.spellStoreType == SpellStoreType.spellStoreMemorized
						&& spellMem.spellStoreState.usedUp == 0
						&& spellMem.metaMagicData == metaMagicData)	{
						spellMem.spellStoreState.usedUp = 1;
						caster.SetSpell(obj_f.critter_spells_memorized_idx, i, spellMem);
						spellFound = true;
						break;
					}
				}

				if (!spellFound){
					Logger.Warn("Spell debit: Spell not found!");
				}

			}

			// add to casted list (so it shows up as used in the Spellbook / gets counted up for spells per day)
			var sd = new SpellStoreData(spellEnum, spellKnownSlotLevel, spellClass, metaMagicData);
			sd.spellStoreState.spellStoreType = SpellStoreType.spellStoreCast;
			var spellArraySize = caster.GetSpellArray(obj_f.critter_spells_cast_idx).Count;
			caster.SetSpell(obj_f.critter_spells_cast_idx, spellArraySize, sd);

        }

        private bool IsItemSpell()
        {
	        return invIdx != INV_IDX_INVALID;
        }

        public string GetName()
        {
	        return GameSystems.Spell.GetSpellName(spellEnum);
        }
    }
}