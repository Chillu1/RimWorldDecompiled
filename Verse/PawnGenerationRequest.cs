using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public struct PawnGenerationRequest
	{
		public PawnKindDef KindDef
		{
			get;
			set;
		}

		public PawnGenerationContext Context
		{
			get;
			set;
		}

		public Faction Faction
		{
			get;
			set;
		}

		public int Tile
		{
			get;
			set;
		}

		public bool ForceGenerateNewPawn
		{
			get;
			set;
		}

		public bool Newborn
		{
			get;
			set;
		}

		public bool AllowDead
		{
			get;
			set;
		}

		public bool AllowDowned
		{
			get;
			set;
		}

		public bool CanGeneratePawnRelations
		{
			get;
			set;
		}

		public bool MustBeCapableOfViolence
		{
			get;
			set;
		}

		public float ColonistRelationChanceFactor
		{
			get;
			set;
		}

		public bool ForceAddFreeWarmLayerIfNeeded
		{
			get;
			set;
		}

		public bool AllowGay
		{
			get;
			set;
		}

		public bool AllowFood
		{
			get;
			set;
		}

		public bool AllowAddictions
		{
			get;
			set;
		}

		public IEnumerable<TraitDef> ForcedTraits
		{
			get;
			set;
		}

		public IEnumerable<TraitDef> ProhibitedTraits
		{
			get;
			set;
		}

		public bool Inhabitant
		{
			get;
			set;
		}

		public bool CertainlyBeenInCryptosleep
		{
			get;
			set;
		}

		public bool ForceRedressWorldPawnIfFormerColonist
		{
			get;
			set;
		}

		public bool WorldPawnFactionDoesntMatter
		{
			get;
			set;
		}

		public float BiocodeWeaponChance
		{
			get;
			set;
		}

		public float BiocodeApparelChance
		{
			get;
			set;
		}

		public Pawn ExtraPawnForExtraRelationChance
		{
			get;
			set;
		}

		public float RelationWithExtraPawnChanceFactor
		{
			get;
			set;
		}

		public Predicate<Pawn> RedressValidator
		{
			get;
			set;
		}

		public Predicate<Pawn> ValidatorPreGear
		{
			get;
			set;
		}

		public Predicate<Pawn> ValidatorPostGear
		{
			get;
			set;
		}

		public float? MinChanceToRedressWorldPawn
		{
			get;
			set;
		}

		public float? FixedBiologicalAge
		{
			get;
			set;
		}

		public float? FixedChronologicalAge
		{
			get;
			set;
		}

		public Gender? FixedGender
		{
			get;
			set;
		}

		public float? FixedMelanin
		{
			get;
			set;
		}

		public string FixedLastName
		{
			get;
			set;
		}

		public string FixedBirthName
		{
			get;
			set;
		}

		public RoyalTitleDef FixedTitle
		{
			get;
			set;
		}

		public PawnGenerationRequest(PawnKindDef kind, Faction faction = null, PawnGenerationContext context = PawnGenerationContext.NonPlayer, int tile = -1, bool forceGenerateNewPawn = false, bool newborn = false, bool allowDead = false, bool allowDowned = false, bool canGeneratePawnRelations = true, bool mustBeCapableOfViolence = false, float colonistRelationChanceFactor = 1f, bool forceAddFreeWarmLayerIfNeeded = false, bool allowGay = true, bool allowFood = true, bool allowAddictions = true, bool inhabitant = false, bool certainlyBeenInCryptosleep = false, bool forceRedressWorldPawnIfFormerColonist = false, bool worldPawnFactionDoesntMatter = false, float biocodeWeaponChance = 0f, Pawn extraPawnForExtraRelationChance = null, float relationWithExtraPawnChanceFactor = 1f, Predicate<Pawn> validatorPreGear = null, Predicate<Pawn> validatorPostGear = null, IEnumerable<TraitDef> forcedTraits = null, IEnumerable<TraitDef> prohibitedTraits = null, float? minChanceToRedressWorldPawn = null, float? fixedBiologicalAge = null, float? fixedChronologicalAge = null, Gender? fixedGender = null, float? fixedMelanin = null, string fixedLastName = null, string fixedBirthName = null, RoyalTitleDef fixedTitle = null)
		{
			this = default(PawnGenerationRequest);
			if (context == PawnGenerationContext.All)
			{
				Log.Error("Should not generate pawns with context 'All'");
				context = PawnGenerationContext.NonPlayer;
			}
			if (inhabitant && (tile == -1 || Current.Game.FindMap(tile) == null))
			{
				Log.Error("Trying to generate an inhabitant but map is null.");
				inhabitant = false;
			}
			KindDef = kind;
			Context = context;
			Faction = faction;
			Tile = tile;
			ForceGenerateNewPawn = forceGenerateNewPawn;
			Newborn = newborn;
			AllowDead = allowDead;
			AllowDowned = allowDowned;
			CanGeneratePawnRelations = canGeneratePawnRelations;
			MustBeCapableOfViolence = mustBeCapableOfViolence;
			ColonistRelationChanceFactor = colonistRelationChanceFactor;
			ForceAddFreeWarmLayerIfNeeded = forceAddFreeWarmLayerIfNeeded;
			AllowGay = allowGay;
			AllowFood = allowFood;
			AllowAddictions = allowAddictions;
			ForcedTraits = forcedTraits;
			ProhibitedTraits = prohibitedTraits;
			Inhabitant = inhabitant;
			CertainlyBeenInCryptosleep = certainlyBeenInCryptosleep;
			ForceRedressWorldPawnIfFormerColonist = forceRedressWorldPawnIfFormerColonist;
			WorldPawnFactionDoesntMatter = worldPawnFactionDoesntMatter;
			ExtraPawnForExtraRelationChance = extraPawnForExtraRelationChance;
			RelationWithExtraPawnChanceFactor = relationWithExtraPawnChanceFactor;
			BiocodeWeaponChance = biocodeWeaponChance;
			ValidatorPreGear = validatorPreGear;
			ValidatorPostGear = validatorPostGear;
			MinChanceToRedressWorldPawn = minChanceToRedressWorldPawn;
			FixedBiologicalAge = fixedBiologicalAge;
			FixedChronologicalAge = fixedChronologicalAge;
			FixedGender = fixedGender;
			FixedMelanin = fixedMelanin;
			FixedLastName = fixedLastName;
			FixedBirthName = fixedBirthName;
			FixedTitle = fixedTitle;
		}

		public static PawnGenerationRequest MakeDefault()
		{
			PawnGenerationRequest result = default(PawnGenerationRequest);
			result.Context = PawnGenerationContext.NonPlayer;
			result.Tile = -1;
			result.CanGeneratePawnRelations = true;
			result.ColonistRelationChanceFactor = 1f;
			result.AllowGay = true;
			result.AllowFood = true;
			result.AllowAddictions = true;
			result.RelationWithExtraPawnChanceFactor = 1f;
			return result;
		}

		public void SetFixedLastName(string fixedLastName)
		{
			if (FixedLastName != null)
			{
				Log.Error("Last name is already a fixed value: " + FixedLastName + ".");
			}
			else
			{
				FixedLastName = fixedLastName;
			}
		}

		public void SetFixedBirthName(string fixedBirthName)
		{
			if (FixedBirthName != null)
			{
				Log.Error("birth name is already a fixed value: " + FixedBirthName + ".");
			}
			else
			{
				FixedBirthName = fixedBirthName;
			}
		}

		public void SetFixedMelanin(float fixedMelanin)
		{
			if (FixedMelanin.HasValue)
			{
				Log.Error("Melanin is already a fixed value: " + FixedMelanin + ".");
			}
			else
			{
				FixedMelanin = fixedMelanin;
			}
		}

		public override string ToString()
		{
			return "kindDef=" + KindDef + ", context=" + Context + ", faction=" + Faction + ", tile=" + Tile + ", forceGenerateNewPawn=" + ForceGenerateNewPawn.ToString() + ", newborn=" + Newborn.ToString() + ", allowDead=" + AllowDead.ToString() + ", allowDowned=" + AllowDowned.ToString() + ", canGeneratePawnRelations=" + CanGeneratePawnRelations.ToString() + ", mustBeCapableOfViolence=" + MustBeCapableOfViolence.ToString() + ", colonistRelationChanceFactor=" + ColonistRelationChanceFactor + ", forceAddFreeWarmLayerIfNeeded=" + ForceAddFreeWarmLayerIfNeeded.ToString() + ", allowGay=" + AllowGay.ToString() + ", prohibitedTraits=" + ProhibitedTraits + ", allowFood=" + AllowFood.ToString() + ", allowAddictions=" + AllowAddictions.ToString() + ", inhabitant=" + Inhabitant.ToString() + ", certainlyBeenInCryptosleep=" + CertainlyBeenInCryptosleep.ToString() + ", biocodeWeaponChance=" + BiocodeWeaponChance + ", validatorPreGear=" + ValidatorPreGear + ", validatorPostGear=" + ValidatorPostGear + ", fixedBiologicalAge=" + FixedBiologicalAge + ", fixedChronologicalAge=" + FixedChronologicalAge + ", fixedGender=" + FixedGender + ", fixedMelanin=" + FixedMelanin + ", fixedLastName=" + FixedLastName + ", fixedBirthName=" + FixedBirthName;
		}
	}
}
