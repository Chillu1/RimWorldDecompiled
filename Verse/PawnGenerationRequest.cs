using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public struct PawnGenerationRequest
{
	private PawnKindDef kindDefInner;

	private bool _calledTheCorrectConstructor;

	public PawnKindDef KindDef
	{
		get
		{
			if (PawnKindDefGetter != null)
			{
				return PawnKindDefGetter(ForcedXenotype);
			}
			return kindDefInner;
		}
		set
		{
			kindDefInner = value;
		}
	}

	public PawnGenerationContext Context { get; set; }

	public Faction Faction { get; set; }

	public PlanetTile Tile { get; set; }

	public bool ForceGenerateNewPawn { get; set; }

	public BodyTypeDef ForceBodyType { get; set; }

	public bool AllowDead { get; set; }

	public bool AllowDowned { get; set; }

	public bool CanGeneratePawnRelations { get; set; }

	public bool MustBeCapableOfViolence { get; set; }

	public float ColonistRelationChanceFactor { get; set; }

	public bool ForceAddFreeWarmLayerIfNeeded { get; set; }

	public bool AllowGay { get; set; }

	public bool AllowPregnant { get; set; }

	public bool AllowFood { get; set; }

	public bool AllowAddictions { get; set; }

	public IEnumerable<TraitDef> ForcedTraits { get; set; }

	public IEnumerable<TraitDef> ProhibitedTraits { get; set; }

	public bool Inhabitant { get; set; }

	public bool CertainlyBeenInCryptosleep { get; set; }

	public bool ForceRedressWorldPawnIfFormerColonist { get; set; }

	public bool WorldPawnFactionDoesntMatter { get; set; }

	public float BiocodeWeaponChance { get; set; }

	public float BiocodeApparelChance { get; set; }

	public Pawn ExtraPawnForExtraRelationChance { get; set; }

	public float RelationWithExtraPawnChanceFactor { get; set; }

	public Predicate<Pawn> RedressValidator { get; set; }

	public bool DontGiveWeapon { get; set; }

	public bool OnlyUseForcedBackstories { get; set; }

	public Predicate<Pawn> ValidatorPreGear { get; set; }

	public Predicate<Pawn> ValidatorPostGear { get; set; }

	public float? MinChanceToRedressWorldPawn { get; set; }

	public float? FixedBiologicalAge { get; set; }

	public float? FixedChronologicalAge { get; set; }

	public Gender? FixedGender { get; set; }

	public string FixedLastName { get; set; }

	public string FixedBirthName { get; set; }

	public RoyalTitleDef FixedTitle { get; set; }

	public bool ForbidAnyTitle { get; set; }

	public Ideo FixedIdeo { get; set; }

	public bool ForceNoIdeo { get; set; }

	public bool ForceNoBackstory { get; set; }

	public bool ForceDead { get; set; }

	public List<GeneDef> ForcedXenogenes { get; set; }

	public List<GeneDef> ForcedEndogenes { get; set; }

	public XenotypeDef ForcedXenotype { get; set; }

	public List<XenotypeDef> AllowedXenotypes { get; set; }

	public float ForceBaselinerChance { get; set; }

	public CustomXenotype ForcedCustomXenotype { get; set; }

	public DevelopmentalStage AllowedDevelopmentalStages { get; set; }

	public Func<XenotypeDef, PawnKindDef> PawnKindDefGetter { get; set; }

	public FloatRange? ExcludeBiologicalAgeRange { get; set; }

	public FloatRange? BiologicalAgeRange { get; set; }

	public bool ForceRecruitable { get; set; }

	public int MinimumAgeTraits { get; set; }

	public int MaximumAgeTraits { get; set; }

	public bool ForceNoGear { get; set; }

	public MutantDef ForcedMutant { get; set; }

	public bool IsCreepJoiner { get; set; }

	public bool DontGivePreArrivalPathway { get; set; }

	public bool ForceNoIdeoGear { get; set; }

	public PawnGenerationRequest(PawnKindDef kind, Faction faction = null, PawnGenerationContext context = PawnGenerationContext.NonPlayer, PlanetTile? tile = null, bool forceGenerateNewPawn = false, bool allowDead = false, bool allowDowned = false, bool canGeneratePawnRelations = true, bool mustBeCapableOfViolence = false, float colonistRelationChanceFactor = 1f, bool forceAddFreeWarmLayerIfNeeded = false, bool allowGay = true, bool allowPregnant = false, bool allowFood = true, bool allowAddictions = true, bool inhabitant = false, bool certainlyBeenInCryptosleep = false, bool forceRedressWorldPawnIfFormerColonist = false, bool worldPawnFactionDoesntMatter = false, float biocodeWeaponChance = 0f, float biocodeApparelChance = 0f, Pawn extraPawnForExtraRelationChance = null, float relationWithExtraPawnChanceFactor = 1f, Predicate<Pawn> validatorPreGear = null, Predicate<Pawn> validatorPostGear = null, IEnumerable<TraitDef> forcedTraits = null, IEnumerable<TraitDef> prohibitedTraits = null, float? minChanceToRedressWorldPawn = null, float? fixedBiologicalAge = null, float? fixedChronologicalAge = null, Gender? fixedGender = null, string fixedLastName = null, string fixedBirthName = null, RoyalTitleDef fixedTitle = null, Ideo fixedIdeo = null, bool forceNoIdeo = false, bool forceNoBackstory = false, bool forbidAnyTitle = false, bool forceDead = false, List<GeneDef> forcedXenogenes = null, List<GeneDef> forcedEndogenes = null, XenotypeDef forcedXenotype = null, CustomXenotype forcedCustomXenotype = null, List<XenotypeDef> allowedXenotypes = null, float forceBaselinerChance = 0f, DevelopmentalStage developmentalStages = DevelopmentalStage.Adult, Func<XenotypeDef, PawnKindDef> pawnKindDefGetter = null, FloatRange? excludeBiologicalAgeRange = null, FloatRange? biologicalAgeRange = null, bool forceRecruitable = false, bool dontGiveWeapon = false, bool onlyUseForcedBackstories = false, int maximumAgeTraits = -1, int minimumAgeTraits = 0, bool forceNoGear = false)
	{
		this = default(PawnGenerationRequest);
		_calledTheCorrectConstructor = true;
		KindDef = kind;
		Context = context;
		Tile = tile ?? PlanetTile.Invalid;
		Faction = faction;
		ForceGenerateNewPawn = forceGenerateNewPawn;
		AllowDead = allowDead;
		AllowDowned = allowDowned;
		CanGeneratePawnRelations = canGeneratePawnRelations;
		MustBeCapableOfViolence = mustBeCapableOfViolence;
		ColonistRelationChanceFactor = colonistRelationChanceFactor;
		ForceAddFreeWarmLayerIfNeeded = forceAddFreeWarmLayerIfNeeded;
		AllowGay = allowGay;
		AllowPregnant = allowPregnant;
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
		BiocodeApparelChance = biocodeApparelChance;
		ForceNoIdeo = forceNoIdeo;
		ForceNoBackstory = forceNoBackstory;
		ForbidAnyTitle = forbidAnyTitle;
		ValidatorPreGear = validatorPreGear;
		ValidatorPostGear = validatorPostGear;
		MinChanceToRedressWorldPawn = minChanceToRedressWorldPawn;
		FixedBiologicalAge = fixedBiologicalAge;
		FixedChronologicalAge = fixedChronologicalAge;
		FixedGender = fixedGender;
		FixedLastName = fixedLastName;
		FixedBirthName = fixedBirthName;
		FixedTitle = fixedTitle;
		FixedIdeo = fixedIdeo;
		ForceDead = forceDead;
		ForcedXenotype = forcedXenotype;
		ForcedCustomXenotype = forcedCustomXenotype;
		AllowedXenotypes = allowedXenotypes;
		ForceBaselinerChance = forceBaselinerChance;
		AllowedDevelopmentalStages = developmentalStages;
		PawnKindDefGetter = pawnKindDefGetter;
		ExcludeBiologicalAgeRange = excludeBiologicalAgeRange;
		BiologicalAgeRange = biologicalAgeRange;
		ForceRecruitable = forceRecruitable;
		DontGiveWeapon = dontGiveWeapon;
		OnlyUseForcedBackstories = onlyUseForcedBackstories;
		MaximumAgeTraits = maximumAgeTraits;
		MinimumAgeTraits = minimumAgeTraits;
		ForceNoGear = forceNoGear;
		if (forcedXenogenes != null)
		{
			foreach (GeneDef forcedXenogene in forcedXenogenes)
			{
				AddForcedGene(forcedXenogene, xenogene: true);
			}
		}
		if (forcedEndogenes != null)
		{
			foreach (GeneDef forcedEndogene in forcedEndogenes)
			{
				AddForcedGene(forcedEndogene, xenogene: false);
			}
		}
		ValidateAndFix();
	}

	public void ValidateAndFix()
	{
		if (!_calledTheCorrectConstructor)
		{
			Log.Error("This PawnGenerationRequest was not created through the correct constructor.");
			_calledTheCorrectConstructor = true;
		}
		if (Context == PawnGenerationContext.All)
		{
			Log.Error("Should not generate pawns with context 'All'");
			Context = PawnGenerationContext.NonPlayer;
		}
		if (ForceNoIdeo && FixedIdeo != null)
		{
			Log.Error("Trying to generate a pawn with no ideo and a fixed ideo.");
			ForceNoIdeo = false;
		}
		if ((AllowedDevelopmentalStages.Newborn() || AllowedDevelopmentalStages.Baby()) && FixedIdeo != null)
		{
			Log.Error("Trying to generate baby with specific ideology (babies have no ideology).");
			FixedIdeo = null;
		}
		if (!AllowDowned && AlwaysDownedLifeStages(out var stages))
		{
			Log.Error($"Trying to generate a non-downed {AllowedDevelopmentalStages} {KindDef.label} pawn but that would include these downed lifestages: {stages}");
			AllowDowned = true;
		}
		if (AllowedDevelopmentalStages.Newborn() && AllowedDevelopmentalStages != DevelopmentalStage.Newborn)
		{
			Log.Error("Trying to generate a newborn and other developmental stages simultaneously.");
			AllowedDevelopmentalStages = DevelopmentalStage.Newborn;
		}
		if (ExcludeBiologicalAgeRange.HasValue && FixedBiologicalAge.HasValue)
		{
			Log.Error("Trying to generate a pawn with a fixed biological age and an excluded biological age range.");
			ExcludeBiologicalAgeRange = null;
		}
		if (BiologicalAgeRange.HasValue && FixedBiologicalAge.HasValue)
		{
			Log.Error("Trying to generate a pawn with a fixed biological age and a biological age range.");
			BiologicalAgeRange = null;
		}
		if (BiologicalAgeRange.HasValue && ExcludeBiologicalAgeRange.HasValue)
		{
			Log.Error("Trying to generate a pawn with both a include and exclude age range");
			BiologicalAgeRange = null;
			ExcludeBiologicalAgeRange = null;
		}
	}

	[Obsolete("Will be removed in 1.5; use the constructor instead.")]
	public static PawnGenerationRequest MakeDefault()
	{
		return new PawnGenerationRequest(null);
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

	public void AddForcedGene(GeneDef gene, bool xenogene)
	{
		if (xenogene)
		{
			if (ForcedXenogenes == null)
			{
				ForcedXenogenes = new List<GeneDef>();
			}
			ForcedXenogenes.Add(gene);
		}
		else
		{
			if (ForcedEndogenes == null)
			{
				ForcedEndogenes = new List<GeneDef>();
			}
			ForcedEndogenes.Add(gene);
		}
	}

	public override string ToString()
	{
		return "kindDef=" + KindDef?.ToString() + ", context=" + Context.ToString() + ", faction=" + Faction?.ToString() + ", tile=" + Tile.ToString() + ", forceGenerateNewPawn=" + ForceGenerateNewPawn + ", allowedDevelopmentalStages=" + AllowedDevelopmentalStages.ToString() + ", allowDead=" + AllowDead + ", allowDowned=" + AllowDowned + ", canGeneratePawnRelations=" + CanGeneratePawnRelations + ", mustBeCapableOfViolence=" + MustBeCapableOfViolence + ", colonistRelationChanceFactor=" + ColonistRelationChanceFactor + ", forceAddFreeWarmLayerIfNeeded=" + ForceAddFreeWarmLayerIfNeeded + ", allowGay=" + AllowGay + ", prohibitedTraits=" + ProhibitedTraits?.ToString() + ", allowFood=" + AllowFood + ", allowAddictions=" + AllowAddictions + ", inhabitant=" + Inhabitant + ", certainlyBeenInCryptosleep=" + CertainlyBeenInCryptosleep + ", biocodeWeaponChance=" + BiocodeWeaponChance + ", validatorPreGear=" + ValidatorPreGear?.ToString() + ", validatorPostGear=" + ValidatorPostGear?.ToString() + ", fixedBiologicalAge=" + FixedBiologicalAge + ", fixedChronologicalAge=" + FixedChronologicalAge + ", fixedGender=" + FixedGender.ToString() + ", fixedLastName=" + FixedLastName + ", fixedBirthName=" + FixedBirthName;
	}

	private bool AlwaysDownedLifeStages(out string stages)
	{
		DevelopmentalStage developmentalStage = AllowedDevelopmentalStages;
		if (AllowedDevelopmentalStages.Newborn())
		{
			developmentalStage |= DevelopmentalStage.Baby;
		}
		StringBuilder stringBuilder = null;
		foreach (LifeStageAge lifeStageAge in KindDef.RaceProps.lifeStageAges)
		{
			if (lifeStageAge.def.alwaysDowned && developmentalStage.Has(lifeStageAge.def.developmentalStage))
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(lifeStageAge.def.label, 64);
					continue;
				}
				stringBuilder.Append(", ");
				stringBuilder.Append(lifeStageAge.def.label);
			}
		}
		if (stringBuilder == null)
		{
			stages = null;
			return false;
		}
		stages = stringBuilder.ToString();
		return true;
	}
}
