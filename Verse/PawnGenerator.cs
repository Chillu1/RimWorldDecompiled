using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public static class PawnGenerator
{
	private struct PawnGenerationStatus
	{
		public Pawn Pawn { get; private set; }

		public bool AllowsDead { get; private set; }

		public List<Pawn> PawnsGeneratedInTheMeantime { get; private set; }

		public PawnGenerationStatus(Pawn pawn, List<Pawn> pawnsGeneratedInTheMeantime, bool allowsDead)
		{
			this = default(PawnGenerationStatus);
			Pawn = pawn;
			PawnsGeneratedInTheMeantime = pawnsGeneratedInTheMeantime;
			AllowsDead = allowsDead;
		}
	}

	private static List<PawnGenerationStatus> pawnsBeingGenerated = new List<PawnGenerationStatus>();

	private static PawnRelationDef[] relationsGeneratableBlood = DefDatabase<PawnRelationDef>.AllDefsListForReading.Where((PawnRelationDef rel) => rel.familyByBloodRelation && rel.generationChanceFactor > 0f).ToArray();

	private static PawnRelationDef[] relationsGeneratableNonblood = DefDatabase<PawnRelationDef>.AllDefsListForReading.Where((PawnRelationDef rel) => !rel.familyByBloodRelation && rel.generationChanceFactor > 0f).ToArray();

	public const float MaxStartMinorMentalBreakThreshold = 0.5f;

	public const float JoinAsSlaveChance = 0.75f;

	public const float GenerateAllRequiredScarsChance = 0.5f;

	private const int MaxTraitGenAttempts = 500;

	private const float HumanlikeSterilizationChance = 0.005f;

	private const float HumanlikeFemaleIUDChance = 0.005f;

	private const float HumanlikeMaleVasectomyChance = 0.005f;

	private const int HumanlikeFertilityHediffStartingAge = 20;

	private static readonly IntRange TraitsCountRange = new IntRange(1, 3);

	private static readonly SimpleCurve ScarChanceFromAgeYearsCurve = new SimpleCurve
	{
		new CurvePoint(5f, 0f),
		new CurvePoint(18f, 1f)
	};

	private static List<Hediff_MissingPart> tmpMissingParts = new List<Hediff_MissingPart>();

	private static SimpleCurve DefaultAgeGenerationCurve = new SimpleCurve
	{
		new CurvePoint(0.05f, 0f),
		new CurvePoint(0.1f, 100f),
		new CurvePoint(0.675f, 100f),
		new CurvePoint(0.75f, 30f),
		new CurvePoint(0.875f, 18f),
		new CurvePoint(1f, 10f),
		new CurvePoint(1.125f, 3f),
		new CurvePoint(1.25f, 0f)
	};

	public const float MaxGeneratedMechanoidAge = 2500f;

	private static List<Pair<TraitDef, float>> tmpTraitChances = new List<Pair<TraitDef, float>>();

	private static Dictionary<XenotypeDef, float> tmpXenotypeChances = new Dictionary<XenotypeDef, float>();

	private static HashSet<BodyTypeDef> tmpBodyTypes = new HashSet<BodyTypeDef>();

	private static readonly SimpleCurve AgeSkillMaxFactorCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(10f, 0.7f),
		new CurvePoint(35f, 1f),
		new CurvePoint(60f, 1.6f)
	};

	private static readonly SimpleCurve AgeSkillFactor = new SimpleCurve
	{
		new CurvePoint(3f, 0.2f),
		new CurvePoint(18f, 1f)
	};

	private static readonly SimpleCurve LevelFinalAdjustmentCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(10f, 10f),
		new CurvePoint(20f, 16f),
		new CurvePoint(27f, 20f)
	};

	private static readonly SimpleCurve LevelRandomCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(0.5f, 150f),
		new CurvePoint(4f, 150f),
		new CurvePoint(5f, 25f),
		new CurvePoint(10f, 5f),
		new CurvePoint(15f, 0f)
	};

	public static void Reset()
	{
		relationsGeneratableBlood = DefDatabase<PawnRelationDef>.AllDefsListForReading.Where((PawnRelationDef rel) => rel.familyByBloodRelation && rel.generationChanceFactor > 0f).ToArray();
		relationsGeneratableNonblood = DefDatabase<PawnRelationDef>.AllDefsListForReading.Where((PawnRelationDef rel) => !rel.familyByBloodRelation && rel.generationChanceFactor > 0f).ToArray();
	}

	public static Pawn GeneratePawn(PawnKindDef kindDef, Faction faction = null, PlanetTile? tile = null)
	{
		PawnGenerationRequest request = new PawnGenerationRequest(kindDef, faction);
		if (tile.HasValue)
		{
			request.Tile = tile.Value;
		}
		if (faction != null && faction.deactivated)
		{
			request.AllowDowned = true;
		}
		if (kindDef is CreepJoinerFormKindDef)
		{
			request.IsCreepJoiner = true;
		}
		return GeneratePawn(request);
	}

	public static Pawn GeneratePawn(PawnGenerationRequest request)
	{
		request.ValidateAndFix();
		try
		{
			Pawn pawn = GenerateOrRedressPawnInternal(request);
			if (pawn != null && !request.AllowDead && !request.ForceDead && pawn.health.hediffSet.hediffs.Any())
			{
				bool dead = pawn.Dead;
				bool downed = pawn.Downed;
				pawn.health.hediffSet.DirtyCache();
				pawn.health.CheckForStateChange(null, null);
				if (pawn.Dead)
				{
					string[] obj = new string[10]
					{
						"Pawn was generated dead but the pawn generation request specified the pawn must be alive. This shouldn't ever happen even if we ran out of tries because null pawn should have been returned instead in this case. Resetting health...\npawn.Dead=",
						pawn.Dead.ToString(),
						" pawn.Downed=",
						pawn.Downed.ToString(),
						" deadBefore=",
						dead.ToString(),
						" downedBefore=",
						downed.ToString(),
						"\nrequest=",
						null
					};
					PawnGenerationRequest pawnGenerationRequest = request;
					obj[9] = pawnGenerationRequest.ToString();
					Log.Error(string.Concat(obj));
					pawn.health.Reset();
				}
			}
			if (pawn.guest != null)
			{
				if (request.ForceRecruitable)
				{
					pawn.guest.Recruitable = true;
				}
				else
				{
					pawn.guest.SetupRecruitable();
				}
			}
			if (pawn.Faction == Faction.OfPlayerSilentFail && !pawn.IsQuestLodger())
			{
				Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedColonist);
			}
			return pawn;
		}
		catch (Exception ex)
		{
			Log.Error("Error while generating pawn. Rethrowing. Exception: " + ex);
			throw;
		}
		finally
		{
		}
	}

	private static Pawn GenerateOrRedressPawnInternal(PawnGenerationRequest request)
	{
		Pawn result = null;
		if (!request.AllowedDevelopmentalStages.Newborn() && !request.ForceGenerateNewPawn)
		{
			if (request.ForceRedressWorldPawnIfFormerColonist && GetValidCandidatesToRedress(request).Where(PawnUtility.EverBeenColonistOrTameAnimal).TryRandomElementByWeight(WorldPawnSelectionWeight, out result))
			{
				RedressPawn(result, request);
				Find.WorldPawns.RemovePawn(result);
			}
			if (result == null && request.Inhabitant && request.Tile.Valid)
			{
				Settlement settlement = Find.WorldObjects.WorldObjectAt<Settlement>(request.Tile);
				if (settlement != null && settlement.previouslyGeneratedInhabitants.Any() && (from x in GetValidCandidatesToRedress(request)
					where settlement.previouslyGeneratedInhabitants.Contains(x)
					select x).TryRandomElementByWeight(WorldPawnSelectionWeight, out result))
				{
					RedressPawn(result, request);
					Find.WorldPawns.RemovePawn(result);
				}
			}
			if (result == null && Rand.Chance(ChanceToRedressAnyWorldPawn(request)) && GetValidCandidatesToRedress(request).TryRandomElementByWeight(WorldPawnSelectionWeight, out result))
			{
				RedressPawn(result, request);
				Find.WorldPawns.RemovePawn(result);
			}
		}
		bool redressed;
		if (result == null)
		{
			redressed = false;
			result = GenerateNewPawnInternal(ref request);
			if (result == null)
			{
				return null;
			}
			if (request.Inhabitant && !request.Tile.Valid)
			{
				Find.WorldObjects.WorldObjectAt<Settlement>(request.Tile)?.previouslyGeneratedInhabitants.Add(result);
			}
		}
		else
		{
			redressed = true;
		}
		if (result.Ideo != null)
		{
			result.Ideo.Notify_MemberGenerated(result, request.AllowedDevelopmentalStages.Newborn(), request.ForceNoIdeoGear);
		}
		if (Find.Scenario != null)
		{
			Find.Scenario.Notify_PawnGenerated(result, request.Context, redressed);
		}
		if (request.DontGivePreArrivalPathway)
		{
			result.dontGivePreArrivalPathway = true;
		}
		return result;
	}

	public static void RedressPawn(Pawn pawn, PawnGenerationRequest request)
	{
		try
		{
			if (pawn.becameWorldPawnTickAbs != -1 && pawn.health != null)
			{
				float x = (GenTicks.TicksAbs - pawn.becameWorldPawnTickAbs).TicksToDays();
				List<Hediff> list = SimplePool<List<Hediff>>.Get();
				list.Clear();
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					if (!hediff.def.removeOnRedressIfNotOfKind.NullOrEmpty() && !hediff.def.removeOnRedressIfNotOfKind.Contains(request.KindDef))
					{
						list.Add(hediff);
					}
					else if (Rand.Chance(hediff.def.removeOnRedressChanceByDaysCurve.Evaluate(x)))
					{
						list.Add(hediff);
					}
				}
				foreach (Hediff item in list)
				{
					pawn.health.RemoveHediff(item);
				}
				list.Clear();
				SimplePool<List<Hediff>>.Return(list);
			}
			pawn.ChangeKind(request.KindDef);
			if (pawn.royalty != null)
			{
				pawn.royalty.allowRoomRequirements = pawn.kindDef.allowRoyalRoomRequirements;
				pawn.royalty.allowApparelRequirements = pawn.kindDef.allowRoyalApparelRequirements;
			}
			if (ModsConfig.BiotechActive && pawn.genes != null)
			{
				List<Gene> genesListForReading = pawn.genes.GenesListForReading;
				for (int num = genesListForReading.Count - 1; num >= 0; num--)
				{
					if (genesListForReading[num].def.removeOnRedress)
					{
						pawn.genes.RemoveGene(genesListForReading[num]);
					}
				}
			}
			if (pawn.Faction != request.Faction)
			{
				pawn.SetFaction(request.Faction);
				if (request.FixedIdeo != null)
				{
					pawn.ideo.SetIdeo(request.FixedIdeo);
				}
				else if (pawn.ideo != null && request.Faction?.ideos != null && !request.Faction.ideos.Has(pawn.Ideo))
				{
					pawn.ideo.SetIdeo(request.Faction.ideos.GetRandomIdeoForNewPawn());
				}
			}
			GenerateGearFor(pawn, request);
			AddRequiredScars(pawn);
			if (ModsConfig.AnomalyActive)
			{
				if (request.ForcedMutant != null)
				{
					MutantUtility.SetFreshPawnAsMutant(pawn, request.ForcedMutant);
				}
				else if (request.KindDef.mutant != null)
				{
					MutantUtility.SetFreshPawnAsMutant(pawn, request.KindDef.mutant);
				}
			}
			if (pawn.guest != null)
			{
				pawn.guest.SetGuestStatus(null);
				pawn.guest.RandomizeJoinStatus();
			}
			pawn.needs?.SetInitialLevels();
			pawn.mindState?.Notify_PawnRedressed();
			pawn.surroundings?.Clear();
			pawn.genes?.Reset();
			pawn.wasLeftBehindStartingPawn = false;
		}
		finally
		{
		}
	}

	public static bool IsBeingGenerated(Pawn pawn)
	{
		for (int i = 0; i < pawnsBeingGenerated.Count; i++)
		{
			if (pawnsBeingGenerated[i].Pawn == pawn)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPawnBeingGeneratedAndNotAllowsDead(Pawn pawn)
	{
		for (int i = 0; i < pawnsBeingGenerated.Count; i++)
		{
			if (pawnsBeingGenerated[i].Pawn == pawn && !pawnsBeingGenerated[i].AllowsDead)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsValidCandidateToRedress(Pawn pawn, PawnGenerationRequest request)
	{
		if (pawn.def != request.KindDef.race)
		{
			return false;
		}
		if (!request.WorldPawnFactionDoesntMatter && pawn.Faction != request.Faction)
		{
			return false;
		}
		if (!request.AllowDead)
		{
			if (pawn.Dead || pawn.Destroyed)
			{
				return false;
			}
			if (pawn.health.hediffSet.GetBrain() == null)
			{
				return false;
			}
		}
		if (!request.AllowDowned && pawn.Downed)
		{
			return false;
		}
		if (pawn.health.hediffSet.BleedRateTotal > 0.001f)
		{
			return false;
		}
		if (!request.CanGeneratePawnRelations && pawn.RaceProps.IsFlesh && pawn.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
		{
			return false;
		}
		if (!request.AllowGay && pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
		{
			return false;
		}
		if (!request.AllowAddictions && AddictionUtility.AddictedToAnything(pawn))
		{
			return false;
		}
		if (request.ProhibitedTraits != null && request.ProhibitedTraits.Any((TraitDef t) => pawn.story.traits.HasTrait(t)))
		{
			return false;
		}
		if (request.KindDef.forcedHair != null && pawn.story.hairDef != request.KindDef.forcedHair)
		{
			return false;
		}
		if (ModsConfig.BiotechActive && !request.AllowPregnant && pawn.RaceProps.Humanlike && pawn.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman))
		{
			return false;
		}
		if (pawn.IsMutant)
		{
			return false;
		}
		List<SkillRange> skills = request.KindDef.skills;
		if (skills != null)
		{
			for (int num = 0; num < skills.Count; num++)
			{
				SkillRecord skill = pawn.skills.GetSkill(skills[num].Skill);
				if (skill.TotallyDisabled)
				{
					return false;
				}
				if (skill.Level < skills[num].Range.min || skill.Level > skills[num].Range.max)
				{
					return false;
				}
			}
		}
		if (request.KindDef.missingParts != null)
		{
			foreach (MissingPart missingPart in request.KindDef.missingParts)
			{
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					if (hediff is Hediff_MissingPart hediff_MissingPart)
					{
						if (missingPart.BodyPart == hediff_MissingPart.Part.def && !tmpMissingParts.Contains(hediff_MissingPart))
						{
							tmpMissingParts.Add(hediff_MissingPart);
							break;
						}
						tmpMissingParts.Clear();
						return false;
					}
				}
			}
			tmpMissingParts.Clear();
		}
		if (request.KindDef.forcedTraits != null)
		{
			foreach (TraitRequirement forcedTrait in request.KindDef.forcedTraits)
			{
				if (!forcedTrait.HasTrait(pawn))
				{
					return false;
				}
			}
		}
		if (request.ForcedTraits != null)
		{
			foreach (TraitDef forcedTrait2 in request.ForcedTraits)
			{
				if (!pawn.story.traits.HasTrait(forcedTrait2))
				{
					return false;
				}
			}
		}
		if (ModsConfig.BiotechActive)
		{
			if (request.ForcedXenogenes != null)
			{
				if (pawn.genes == null)
				{
					return false;
				}
				foreach (GeneDef forcedXenogene in request.ForcedXenogenes)
				{
					if (!pawn.genes.HasXenogene(forcedXenogene))
					{
						return false;
					}
				}
			}
			if (request.ForcedEndogenes != null)
			{
				if (pawn.genes == null)
				{
					return false;
				}
				foreach (GeneDef forcedEndogene in request.ForcedEndogenes)
				{
					if (!pawn.genes.HasEndogene(forcedEndogene))
					{
						return false;
					}
				}
			}
			if (request.ForcedXenotype != null && (pawn.genes == null || pawn.genes.Xenotype != request.ForcedXenotype))
			{
				return false;
			}
			if (pawn.genes != null)
			{
				if (request.KindDef.useFactionXenotypes && request.Faction != null && request.Faction.def.xenotypeSet != null && !request.Faction.def.xenotypeSet.Contains(pawn.genes.Xenotype))
				{
					return false;
				}
				if (request.KindDef.xenotypeSet != null && !request.KindDef.xenotypeSet.Contains(pawn.genes.Xenotype))
				{
					return false;
				}
				if (request.MustBeCapableOfViolence && !pawn.genes.Xenotype.canGenerateAsCombatant)
				{
					return false;
				}
			}
		}
		if (!request.AllowedDevelopmentalStages.HasAny(pawn.DevelopmentalStage))
		{
			return false;
		}
		if (request.KindDef.fixedGender.HasValue && pawn.gender != request.KindDef.fixedGender.Value)
		{
			return false;
		}
		if (request.ValidatorPreGear != null && !request.ValidatorPreGear(pawn))
		{
			return false;
		}
		if (request.ValidatorPostGear != null && !request.ValidatorPostGear(pawn))
		{
			return false;
		}
		if (request.FixedBiologicalAge.HasValue && pawn.ageTracker.AgeBiologicalYearsFloat != request.FixedBiologicalAge)
		{
			return false;
		}
		if (request.FixedChronologicalAge.HasValue && (float)pawn.ageTracker.AgeChronologicalYears != request.FixedChronologicalAge)
		{
			return false;
		}
		if (request.KindDef.chronologicalAgeRange.HasValue && !request.KindDef.chronologicalAgeRange.Value.Includes(pawn.ageTracker.AgeChronologicalYears))
		{
			return false;
		}
		if (request.FixedGender.HasValue && pawn.gender != request.FixedGender)
		{
			return false;
		}
		if (request.FixedLastName != null && (!(pawn.Name is NameTriple nameTriple) || nameTriple.Last != request.FixedLastName))
		{
			return false;
		}
		if (request.FixedTitle != null && (pawn.royalty == null || !pawn.royalty.HasTitle(request.FixedTitle)))
		{
			return false;
		}
		if (request.ForceNoIdeo && pawn.Ideo != null)
		{
			return false;
		}
		if (request.ForceNoBackstory && pawn.story != null && (pawn.story.Adulthood != null || pawn.story.Childhood != null))
		{
			return false;
		}
		if (request.KindDef.minTitleRequired != null)
		{
			if (pawn.royalty == null)
			{
				return false;
			}
			RoyalTitleDef royalTitleDef = pawn.royalty.MainTitle();
			if (royalTitleDef == null || royalTitleDef.seniority < request.KindDef.minTitleRequired.seniority)
			{
				return false;
			}
		}
		if (request.Context == PawnGenerationContext.PlayerStarter && Find.Scenario != null && !Find.Scenario.AllowPlayerStartingPawn(pawn, tryingToRedress: true, request))
		{
			return false;
		}
		if (request.MustBeCapableOfViolence)
		{
			if (pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				return false;
			}
			if (pawn.RaceProps.ToolUser && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return false;
			}
		}
		if (request.KindDef.requiredWorkTags != WorkTags.None && pawn.kindDef != request.KindDef && (pawn.CombinedDisabledWorkTags & request.KindDef.requiredWorkTags) != WorkTags.None)
		{
			return false;
		}
		if (!HasCorrectMinBestSkillLevel(pawn, request.KindDef))
		{
			return false;
		}
		if (!HasCorrectMinTotalSkillLevels(pawn, request.KindDef))
		{
			return false;
		}
		if (pawn.royalty != null && pawn.royalty.AllTitlesForReading.Any() && request.KindDef.titleRequired == null && request.KindDef.titleSelectOne.NullOrEmpty() && request.KindDef != pawn.kindDef)
		{
			return false;
		}
		if (pawn.royalty != null && request.KindDef == pawn.kindDef && !request.KindDef.titleSelectOne.NullOrEmpty() && !pawn.royalty.AllTitlesForReading.Any())
		{
			return false;
		}
		if (request.RedressValidator != null && !request.RedressValidator(pawn))
		{
			return false;
		}
		if (request.KindDef.requiredWorkTags != WorkTags.None && pawn.kindDef != request.KindDef && (pawn.CombinedDisabledWorkTags & request.KindDef.requiredWorkTags) != WorkTags.None)
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && (pawn.IsCreepJoiner || pawn.kindDef is CreepJoinerFormKindDef))
		{
			return false;
		}
		if (request.ForceDead && !pawn.Dead)
		{
			return false;
		}
		return true;
	}

	private static bool HasCorrectMinBestSkillLevel(Pawn pawn, PawnKindDef kind)
	{
		if (kind.minBestSkillLevel <= 0)
		{
			return true;
		}
		int num = 0;
		for (int i = 0; i < pawn.skills.skills.Count; i++)
		{
			num = Mathf.Max(num, pawn.skills.skills[i].Level);
			if (num >= kind.minBestSkillLevel)
			{
				return true;
			}
		}
		return false;
	}

	private static bool HasCorrectMinTotalSkillLevels(Pawn pawn, PawnKindDef kind)
	{
		if (kind.minTotalSkillLevels <= 0)
		{
			return true;
		}
		int num = 0;
		for (int i = 0; i < pawn.skills.skills.Count; i++)
		{
			num += pawn.skills.skills[i].Level;
			if (num >= kind.minTotalSkillLevels)
			{
				return true;
			}
		}
		return false;
	}

	private static Pawn GenerateNewPawnInternal(ref PawnGenerationRequest request)
	{
		Pawn pawn = null;
		string error = null;
		bool ignoreScenarioRequirements = false;
		bool ignoreValidator = false;
		for (int i = 0; i < 120; i++)
		{
			if (i == 70)
			{
				Log.Error("Could not generate a pawn after " + 70 + " tries. Last error: " + error + " Ignoring scenario requirements.");
				ignoreScenarioRequirements = true;
			}
			if (i == 100)
			{
				Log.Error("Could not generate a pawn after " + 100 + " tries. Last error: " + error + " Ignoring validator.");
				ignoreValidator = true;
			}
			PawnGenerationRequest request2 = request;
			pawn = TryGenerateNewPawnInternal(ref request2, out error, ignoreScenarioRequirements, ignoreValidator);
			if (pawn != null)
			{
				request = request2;
				break;
			}
		}
		if (pawn == null)
		{
			string[] obj = new string[6]
			{
				"Pawn generation error: ",
				error,
				" Too many tries (",
				120.ToString(),
				"), returning null. Generation request: ",
				null
			};
			PawnGenerationRequest pawnGenerationRequest = request;
			obj[5] = pawnGenerationRequest.ToString();
			Log.Error(string.Concat(obj));
			return null;
		}
		return pawn;
	}

	private static Pawn TryGenerateNewPawnInternal(ref PawnGenerationRequest request, out string error, bool ignoreScenarioRequirements, bool ignoreValidator)
	{
		error = null;
		Pawn pawn = (Pawn)ThingMaker.MakeThing(request.KindDef.race);
		pawnsBeingGenerated.Add(new PawnGenerationStatus(pawn, null, request.ForceDead || request.AllowDead));
		try
		{
			pawn.kindDef = request.KindDef;
			pawn.SetFactionDirect(request.Faction);
			PawnComponentsUtility.CreateInitialComponents(pawn);
			if (pawn.IsCreepJoiner && (!request.IsCreepJoiner || !ModsConfig.AnomalyActive))
			{
				error = $"Attempted to generate creepjoiner kindDef ({pawn.kindDef}) when request didn't enable creepjoiners";
				return null;
			}
			if (request.FixedGender.HasValue)
			{
				pawn.gender = request.FixedGender.Value;
			}
			else if (request.KindDef.fixedGender.HasValue)
			{
				pawn.gender = request.KindDef.fixedGender.Value;
			}
			else if (pawn.RaceProps.hasGenders)
			{
				if (pawn.RaceProps.forceGender != Gender.None)
				{
					pawn.gender = pawn.RaceProps.forceGender;
				}
				else if (Rand.Value < 0.5f)
				{
					pawn.gender = Gender.Male;
				}
				else
				{
					pawn.gender = Gender.Female;
				}
			}
			else
			{
				pawn.gender = Gender.None;
			}
			GenerateRandomAge(pawn, request);
			pawn.needs.SetInitialLevels();
			if (request.AllowedDevelopmentalStages.Newborn())
			{
				if (pawn.needs?.food != null)
				{
					pawn.needs.food.CurLevelPercentage = Mathf.Lerp(pawn.needs.food.PercentageThreshHungry, pawn.needs.food.PercentageThreshUrgentlyHungry, 0.5f);
				}
				if (pawn.needs?.rest != null)
				{
					pawn.needs.rest.CurLevelPercentage = Mathf.Lerp(0.28f, 0.14f, 0.5f);
				}
			}
			if (pawn.RaceProps.Humanlike)
			{
				Faction faction2;
				Faction faction = ((request.Faction != null) ? request.Faction : ((!Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction2, tryMedievalOrBetter: false, allowDefeated: true)) ? Faction.OfAncients : faction2));
				pawn.story.skinColorOverride = pawn.kindDef.skinColorOverride;
				pawn.story.TryGetRandomHeadFromSet(DefDatabase<HeadTypeDef>.AllDefs.Where((HeadTypeDef x) => x.randomChosen));
				if (ModsConfig.IdeologyActive)
				{
					if (request.KindDef?.favoriteColor != null)
					{
						pawn.story.favoriteColor = request.KindDef.favoriteColor;
					}
					else
					{
						pawn.story.favoriteColor = DefDatabase<ColorDef>.AllDefs.Where(delegate(ColorDef x)
						{
							ColorType colorType = x.colorType;
							return colorType == ColorType.Ideo || colorType == ColorType.Misc;
						}).RandomElement();
					}
				}
				XenotypeDef xenotype = (ModsConfig.BiotechActive ? GetXenotypeForGeneratedPawn(request) : null);
				if (ModsConfig.BiotechActive && request.Faction == null && xenotype == XenotypeDefOf.Baseliner && request.ForcedCustomXenotype == null)
				{
					AdjustXenotypeForFactionlessPawn(pawn, ref request, ref xenotype);
				}
				PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo(pawn, faction.def, request, xenotype);
				if (pawn.story != null)
				{
					if (request.FixedBirthName != null)
					{
						pawn.story.birthLastName = request.FixedBirthName;
					}
					else if (pawn.Name is NameTriple nameTriple)
					{
						pawn.story.birthLastName = nameTriple.Last;
					}
				}
				GenerateTraits(pawn, request);
				GenerateBodyType(pawn, request);
				GenerateGenes(pawn, xenotype, request);
				GenerateSkills(pawn, request);
			}
			if (!request.AllowedDevelopmentalStages.Newborn() && request.CanGeneratePawnRelations)
			{
				GeneratePawnRelations(pawn, ref request);
			}
			if (pawn.RaceProps.Animal)
			{
				Faction faction3 = request.Faction;
				if (faction3 != null && faction3.IsPlayer)
				{
					pawn.training.SetWantedRecursive(TrainableDefOf.Tameness, checkOn: true);
					pawn.training.Train(TrainableDefOf.Tameness, null, complete: true);
				}
			}
			if (!request.ForbidAnyTitle && pawn.Faction != null && !request.AllowedDevelopmentalStages.Newborn())
			{
				RoyalTitleDef royalTitleDef = request.FixedTitle;
				if (royalTitleDef == null)
				{
					if (request.KindDef.titleRequired != null)
					{
						royalTitleDef = request.KindDef.titleRequired;
					}
					else if (!request.KindDef.titleSelectOne.NullOrEmpty() && Rand.Chance(request.KindDef.royalTitleChance))
					{
						royalTitleDef = request.KindDef.titleSelectOne.RandomElementByWeight((RoyalTitleDef t) => t.commonality);
					}
				}
				if (request.KindDef.minTitleRequired != null && (royalTitleDef == null || royalTitleDef.seniority < request.KindDef.minTitleRequired.seniority))
				{
					royalTitleDef = request.KindDef.minTitleRequired;
				}
				if (royalTitleDef != null)
				{
					Faction faction4 = ((request.Faction != null && request.Faction.def.HasRoyalTitles) ? request.Faction : Find.FactionManager.RandomRoyalFaction());
					pawn.royalty.SetTitle(faction4, royalTitleDef, grantRewards: false);
					if (request.Faction != null && !request.Faction.IsPlayer)
					{
						PurchasePermits(pawn, faction4);
					}
					int amount = 0;
					if (royalTitleDef.GetNextTitle(faction4) != null)
					{
						amount = Rand.Range(0, royalTitleDef.GetNextTitle(faction4).favorCost - 1);
					}
					pawn.royalty.SetFavor(faction4, amount);
					if (royalTitleDef.maxPsylinkLevel > 0)
					{
						Hediff_Level hediff_Level = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_Level;
						pawn.health.AddHediff(hediff_Level);
						hediff_Level.SetLevelTo(royalTitleDef.maxPsylinkLevel);
					}
				}
			}
			if (pawn.royalty != null)
			{
				pawn.royalty.allowRoomRequirements = request.KindDef.allowRoyalRoomRequirements;
				pawn.royalty.allowApparelRequirements = request.KindDef.allowRoyalApparelRequirements;
			}
			if (pawn.guest != null)
			{
				pawn.guest.RandomizeJoinStatus();
			}
			if (pawn.workSettings != null)
			{
				Faction faction5 = request.Faction;
				if (faction5 != null && faction5.IsPlayer)
				{
					pawn.workSettings.EnableAndInitialize();
				}
			}
			if (request.Faction != null && (pawn.RaceProps.Animal || (ModsConfig.BiotechActive && pawn.RaceProps.IsMechanoid)))
			{
				pawn.GenerateNecessaryName();
			}
			if (!pawn.kindDef.preventIdeo && pawn.ideo != null && !pawn.DevelopmentalStage.Baby())
			{
				Ideo result;
				if (request.FixedIdeo != null)
				{
					pawn.ideo.SetIdeo(request.FixedIdeo);
				}
				else if (request.Faction?.ideos != null)
				{
					pawn.ideo.SetIdeo(request.Faction.ideos.GetRandomIdeoForNewPawn());
				}
				else if (Find.IdeoManager.IdeosListForReading.TryRandomElementByWeight((Ideo x) => IdeoUtility.IdeoChangeToWeight(pawn, x), out result))
				{
					pawn.ideo.SetIdeo(result);
				}
			}
			if (pawn.mindState != null)
			{
				pawn.mindState.SetupLastHumanMeatTick();
			}
			if (pawn.surroundings != null)
			{
				pawn.surroundings.Clear();
			}
			GenerateInitialHediffs(pawn, request);
			if (request.ForceDead && !pawn.Dead)
			{
				pawn.Kill(null, null);
			}
			if (pawn.RaceProps.Humanlike)
			{
				pawn.story.hairDef = PawnStyleItemChooser.RandomHairFor(pawn);
				if (pawn.style != null)
				{
					pawn.style.beardDef = PawnStyleItemChooser.RandomBeardFor(pawn);
					if (ModsConfig.IdeologyActive && !pawn.DevelopmentalStage.Baby())
					{
						pawn.style.FaceTattoo = PawnStyleItemChooser.RandomTattooFor(pawn, TattooType.Face);
						pawn.style.BodyTattoo = PawnStyleItemChooser.RandomTattooFor(pawn, TattooType.Body);
					}
					else
					{
						pawn.style.SetupTattoos_NoIdeology();
					}
				}
			}
			if (!request.KindDef.abilities.NullOrEmpty())
			{
				for (int num = 0; num < request.KindDef.abilities.Count; num++)
				{
					pawn.abilities.GainAbility(request.KindDef.abilities[num]);
				}
			}
			if (Find.Scenario != null)
			{
				Find.Scenario.Notify_NewPawnGenerating(pawn, request.Context);
			}
			if (!request.AllowDead && !request.ForceDead && (pawn.Dead || pawn.Destroyed))
			{
				DiscardGeneratedPawn(pawn);
				error = "Generated dead pawn.";
				return null;
			}
			if (!request.AllowDowned && !request.ForceDead && pawn.Downed)
			{
				DiscardGeneratedPawn(pawn);
				error = "Generated downed pawn.";
				return null;
			}
			if (request.MustBeCapableOfViolence && !pawn.RaceProps.alwaysViolent && ((pawn.story != null && pawn.WorkTagIsDisabled(WorkTags.Violent)) || (!pawn.RaceProps.IsMechanoid && pawn.RaceProps.ToolUser && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))))
			{
				DiscardGeneratedPawn(pawn);
				error = "Generated pawn incapable of violence.";
				return null;
			}
			if (request.KindDef != null && !request.KindDef.skills.NullOrEmpty())
			{
				List<SkillRange> skills = request.KindDef.skills;
				for (int num2 = 0; num2 < skills.Count; num2++)
				{
					if (pawn.skills.GetSkill(skills[num2].Skill).TotallyDisabled)
					{
						error = "Generated pawn incapable of required skill: " + skills[num2].Skill.defName;
						return null;
					}
				}
			}
			if (request.KindDef.requiredWorkTags != WorkTags.None && (pawn.CombinedDisabledWorkTags & request.KindDef.requiredWorkTags) != WorkTags.None)
			{
				DiscardGeneratedPawn(pawn);
				error = "Generated pawn with disabled requiredWorkTags.";
				return null;
			}
			if (!HasCorrectMinBestSkillLevel(pawn, request.KindDef))
			{
				DiscardGeneratedPawn(pawn);
				error = "Generated pawn with too low best skill level.";
				return null;
			}
			if (!HasCorrectMinTotalSkillLevels(pawn, request.KindDef))
			{
				DiscardGeneratedPawn(pawn);
				error = "Generated pawn with bad skills.";
				return null;
			}
			if (!ignoreScenarioRequirements && request.Context == PawnGenerationContext.PlayerStarter && Find.Scenario != null && !Find.Scenario.AllowPlayerStartingPawn(pawn, tryingToRedress: false, request))
			{
				DiscardGeneratedPawn(pawn);
				error = "Generated pawn doesn't meet scenario requirements.";
				return null;
			}
			if (!ignoreValidator && request.ValidatorPreGear != null && !request.ValidatorPreGear(pawn))
			{
				DiscardGeneratedPawn(pawn);
				error = "Generated pawn didn't pass validator check (pre-gear).";
				return null;
			}
			if (!request.ForceNoGear && (!request.AllowedDevelopmentalStages.Newborn() || pawn.RaceProps.IsMechanoid))
			{
				GenerateGearFor(pawn, request);
			}
			if (request.ForceDead && pawn.Dead)
			{
				pawn.apparel.Notify_PawnKilled(null);
			}
			if (!ignoreValidator && request.ValidatorPostGear != null && !request.ValidatorPostGear(pawn))
			{
				DiscardGeneratedPawn(pawn);
				error = "Generated pawn didn't pass validator check (post-gear).";
				return null;
			}
			if (ModsConfig.AnomalyActive)
			{
				if (request.ForcedMutant != null)
				{
					MutantUtility.SetFreshPawnAsMutant(pawn, request.ForcedMutant);
				}
				else if (request.KindDef.mutant != null)
				{
					MutantUtility.SetFreshPawnAsMutant(pawn, request.KindDef.mutant);
				}
			}
			for (int num3 = 0; num3 < pawnsBeingGenerated.Count - 1; num3++)
			{
				if (pawnsBeingGenerated[num3].PawnsGeneratedInTheMeantime == null)
				{
					pawnsBeingGenerated[num3] = new PawnGenerationStatus(pawnsBeingGenerated[num3].Pawn, new List<Pawn>(), pawnsBeingGenerated[num3].AllowsDead);
				}
				pawnsBeingGenerated[num3].PawnsGeneratedInTheMeantime.Add(pawn);
			}
			if (pawn.Faction != null)
			{
				pawn.Faction.Notify_PawnJoined(pawn);
			}
			ApplyMiscDamage(pawn, request.KindDef);
			pawn.Drawer?.renderer?.SetAllGraphicsDirty();
			return pawn;
		}
		finally
		{
			pawnsBeingGenerated.RemoveLast();
		}
	}

	private static void ApplyMiscDamage(Pawn pawn, PawnKindDef def)
	{
		foreach (MiscDamage item in def.existingDamage)
		{
			if (item.chance.HasValue && !Rand.Chance(item.chance.Value))
			{
				continue;
			}
			foreach (BodyPartGroupDef group in item.groups)
			{
				foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
				{
					if (notMissingPart.def.canScarify && (group == null || notMissingPart.IsInGroup(group)))
					{
						Hediff hediff = HediffMaker.MakeHediff(item.damage.hediff, pawn, notMissingPart);
						HediffComp_GetsPermanent hediffComp_GetsPermanent = hediff.TryGetComp<HediffComp_GetsPermanent>();
						hediffComp_GetsPermanent.IsPermanent = true;
						hediffComp_GetsPermanent.SetPainCategory(PainCategory.Painless);
						pawn.health.AddHediff(hediff);
						break;
					}
				}
			}
		}
	}

	private static void PurchasePermits(Pawn pawn, Faction faction)
	{
		int num = 200;
		while (true)
		{
			IEnumerable<RoyalTitlePermitDef> source = DefDatabase<RoyalTitlePermitDef>.AllDefs.Where((RoyalTitlePermitDef x) => x.permitPointCost > 0 && x.AvailableForPawn(pawn, faction) && !x.IsPrerequisiteOfHeldPermit(pawn, faction));
			if (source.Any())
			{
				pawn.royalty.AddPermit(source.RandomElement(), faction);
				num--;
				if (num <= 0)
				{
					Log.ErrorOnce("PurchasePermits exceeded max iterations.", 947492);
					break;
				}
				continue;
			}
			break;
		}
	}

	private static void DiscardGeneratedPawn(Pawn pawn)
	{
		if (Find.WorldPawns.Contains(pawn))
		{
			Find.WorldPawns.RemovePawn(pawn);
		}
		Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
		List<Pawn> pawnsGeneratedInTheMeantime = pawnsBeingGenerated.Last().PawnsGeneratedInTheMeantime;
		if (pawnsGeneratedInTheMeantime == null)
		{
			return;
		}
		for (int i = 0; i < pawnsGeneratedInTheMeantime.Count; i++)
		{
			Pawn pawn2 = pawnsGeneratedInTheMeantime[i];
			if (Find.WorldPawns.Contains(pawn2))
			{
				Find.WorldPawns.RemovePawn(pawn2);
			}
			Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
			for (int j = 0; j < pawnsBeingGenerated.Count; j++)
			{
				pawnsBeingGenerated[j].PawnsGeneratedInTheMeantime.Remove(pawn2);
			}
		}
	}

	private static IEnumerable<Pawn> GetValidCandidatesToRedress(PawnGenerationRequest request)
	{
		IEnumerable<Pawn> enumerable = Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.Free);
		if (request.KindDef.factionLeader)
		{
			enumerable = enumerable.Concat(Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.FactionLeader));
		}
		return enumerable.Where((Pawn x) => IsValidCandidateToRedress(x, request));
	}

	private static float ChanceToRedressAnyWorldPawn(PawnGenerationRequest request)
	{
		int pawnsBySituationCount = Find.WorldPawns.GetPawnsBySituationCount(WorldPawnSituation.Free);
		float num = Mathf.Min(0.02f + 0.01f * ((float)pawnsBySituationCount / 10f), 0.8f);
		if (request.MinChanceToRedressWorldPawn.HasValue)
		{
			num = Mathf.Max(num, request.MinChanceToRedressWorldPawn.Value);
		}
		return num;
	}

	private static float WorldPawnSelectionWeight(Pawn p)
	{
		if (p.RaceProps.IsFlesh && !p.relations.everSeenByPlayer && p.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
		{
			return 0.1f;
		}
		return 1f;
	}

	private static void GenerateGearFor(Pawn pawn, PawnGenerationRequest request)
	{
		PawnApparelGenerator.GenerateStartingApparelFor(pawn, request);
		PawnInventoryGenerator.GenerateInventoryFor(pawn, request);
		if (!request.DontGiveWeapon)
		{
			PawnWeaponGenerator.TryGenerateWeaponFor(pawn, request);
		}
	}

	private static void GenerateInitialHediffs(Pawn pawn, PawnGenerationRequest request)
	{
		int num = 0;
		while (true)
		{
			if (!request.AllowedDevelopmentalStages.Newborn())
			{
				AgeInjuryUtility.GenerateRandomOldAgeInjuries(pawn, !request.AllowDead && !request.ForceDead);
				PawnTechHediffsGenerator.GenerateTechHediffsFor(pawn);
			}
			if (!pawn.kindDef.missingParts.NullOrEmpty())
			{
				foreach (MissingPart t in pawn.kindDef.missingParts)
				{
					if (!t.Chance.HasValue || Rand.Chance(t.Chance.Value))
					{
						Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn);
						if (t.Injury != null)
						{
							hediff_MissingPart.lastInjury = t.Injury;
						}
						List<BodyPartRecord> list = (from x in pawn.health.hediffSet.GetNotMissingParts()
							where x.depth == BodyPartDepth.Outside && (x.def.permanentInjuryChanceFactor != 0f || x.def.pawnGeneratorCanAmputate) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(x) && x.def == t.BodyPart
							select x).ToList();
						if (list.Any())
						{
							hediff_MissingPart.Part = list.RandomElement();
							pawn.health.AddHediff(hediff_MissingPart);
						}
					}
				}
			}
			if (request.AllowAddictions && pawn.DevelopmentalStage.Adult())
			{
				PawnAddictionHediffsGenerator.GenerateAddictionsAndTolerancesFor(pawn);
			}
			if (!request.AllowedDevelopmentalStages.Newborn() && !request.AllowedDevelopmentalStages.Baby())
			{
				AddRequiredScars(pawn);
				AddBlindness(pawn);
			}
			if (ModsConfig.BiotechActive && !pawn.Dead && pawn.gender == Gender.Female)
			{
				float chance = pawn.kindDef.humanPregnancyChance * PregnancyUtility.PregnancyChanceForPawn(pawn);
				if (Find.Storyteller.difficulty.ChildrenAllowed && pawn.ageTracker.AgeBiologicalYears >= 16 && request.AllowPregnant && Rand.Chance(chance))
				{
					Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, pawn);
					hediff_Pregnant.Severity = PregnancyUtility.GeneratedPawnPregnancyProgressRange.RandomInRange;
					Pawn father = null;
					if (!Rand.Chance(0.2f) && pawn.relations.DirectRelations.Where((DirectPawnRelation r) => PregnancyUtility.BeingFatherWeightPerRelation.ContainsKey(r.def)).TryRandomElementByWeight((DirectPawnRelation r) => PregnancyUtility.BeingFatherWeightPerRelation[r.def], out var result))
					{
						father = result.otherPawn;
					}
					bool success;
					GeneSet inheritedGeneSet = PregnancyUtility.GetInheritedGeneSet(father, pawn, out success);
					if (success)
					{
						hediff_Pregnant.SetParents(null, father, inheritedGeneSet);
						pawn.health.AddHediff(hediff_Pregnant);
					}
				}
				else if (pawn.RaceProps.Humanlike && pawn.ageTracker.AgeBiologicalYears >= 20)
				{
					BackstoryDef adulthood = pawn.story.Adulthood;
					if (adulthood == null || !adulthood.spawnCategories.Contains("Tribal"))
					{
						if (Rand.Chance(0.005f))
						{
							pawn.health.AddHediff(HediffDefOf.Sterilized);
						}
						else if (Rand.Chance(0.005f))
						{
							pawn.health.AddHediff(HediffDefOf.ImplantedIUD);
						}
					}
				}
			}
			if (ModsConfig.BiotechActive && pawn.RaceProps.Humanlike && !pawn.Dead && pawn.gender == Gender.Male && Find.Storyteller.difficulty.ChildrenAllowed && pawn.ageTracker.AgeBiologicalYears >= 20)
			{
				BackstoryDef adulthood2 = pawn.story.Adulthood;
				if (adulthood2 == null || !adulthood2.spawnCategories.Contains("Tribal"))
				{
					if (Rand.Chance(0.005f))
					{
						pawn.health.AddHediff(HediffDefOf.Sterilized);
					}
					else if (Rand.Chance(0.005f))
					{
						pawn.health.AddHediff(HediffDefOf.Vasectomy);
					}
				}
			}
			if (!pawn.kindDef.startingHediffs.NullOrEmpty() && !request.AllowedDevelopmentalStages.Newborn())
			{
				HealthUtility.AddStartingHediffs(pawn, pawn.kindDef.startingHediffs);
			}
			if (((request.AllowDead || request.ForceDead) && pawn.Dead) || request.AllowDowned || request.ForceDead || !pawn.Downed)
			{
				break;
			}
			pawn.health.Reset();
			num++;
			if (num > 80)
			{
				string[] obj = new string[8]
				{
					"Could not generate old age injuries for ",
					pawn.ThingID,
					" of age ",
					pawn.ageTracker.AgeBiologicalYears.ToString(),
					" that allow pawn to move after ",
					80.ToString(),
					" tries. request=",
					null
				};
				PawnGenerationRequest pawnGenerationRequest = request;
				obj[7] = pawnGenerationRequest.ToString();
				Log.Warning(string.Concat(obj));
				break;
			}
		}
		if (!pawn.Dead && (request.Faction == null || !request.Faction.IsPlayer))
		{
			int num2 = 0;
			while (pawn.health.HasHediffsNeedingTend())
			{
				num2++;
				if (num2 > 10000)
				{
					Log.Error("Too many iterations.");
					break;
				}
				TendUtility.DoTend(null, pawn, null);
			}
		}
		pawn.health.CheckForStateChange(null, null);
	}

	private static void GenerateRandomAge(Pawn pawn, PawnGenerationRequest request)
	{
		float years;
		if (request.AllowedDevelopmentalStages.Newborn())
		{
			pawn.ageTracker.AgeBiologicalTicks = 0L;
			pawn.babyNamingDeadline = Find.TickManager.TicksGame + 60000;
		}
		else if (request.FixedBiologicalAge.HasValue)
		{
			pawn.ageTracker.AgeBiologicalTicks = (long)(request.FixedBiologicalAge.Value * 3600000f);
		}
		else
		{
			years = 0f;
			int num = 0;
			do
			{
				if (request.AllowedDevelopmentalStages == DevelopmentalStage.Baby)
				{
					years = Rand.Range(0f, LifeStageUtility.GetMaxBabyAge(pawn.RaceProps));
				}
				else if (pawn.RaceProps.ageGenerationCurve != null)
				{
					years = Rand.ByCurve(pawn.RaceProps.ageGenerationCurve);
				}
				else if (pawn.RaceProps.IsMechanoid)
				{
					years = Rand.Range(0f, 2500f);
				}
				else
				{
					years = Rand.ByCurve(DefaultAgeGenerationCurve) * pawn.RaceProps.lifeExpectancy;
				}
				num++;
				if (num > 300)
				{
					Log.Error("Tried 300 times to generate age for " + pawn);
					break;
				}
			}
			while (!AgeAllowed(pawn, years));
			pawn.ageTracker.AgeBiologicalTicks = (long)(years * 3600000f);
		}
		if (request.AllowedDevelopmentalStages.Newborn())
		{
			pawn.ageTracker.AgeChronologicalTicks = 0L;
		}
		else if (request.FixedChronologicalAge.HasValue)
		{
			pawn.ageTracker.AgeChronologicalTicks = (long)(request.FixedChronologicalAge.Value * 3600000f);
		}
		else if (request.KindDef.chronologicalAgeRange.HasValue)
		{
			pawn.ageTracker.AgeChronologicalTicks = (long)(request.KindDef.chronologicalAgeRange.Value.RandomInRange * 3600000f);
		}
		else
		{
			int num2;
			if (request.CertainlyBeenInCryptosleep || Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
			{
				float value = Rand.Value;
				if (value < 0.7f)
				{
					num2 = Rand.Range(0, 100);
				}
				else if (value < 0.95f)
				{
					num2 = Rand.Range(100, 1000);
				}
				else
				{
					int maxExclusive = GenDate.Year(GenTicks.TicksAbs, 0f) - 2026 - pawn.ageTracker.AgeBiologicalYears;
					num2 = Rand.Range(1000, maxExclusive);
				}
			}
			else
			{
				num2 = 0;
			}
			long num3 = GenTicks.TicksAbs - pawn.ageTracker.AgeBiologicalTicks;
			num3 -= (long)num2 * 3600000L;
			pawn.ageTracker.BirthAbsTicks = num3;
		}
		if (pawn.ageTracker.AgeBiologicalTicks > pawn.ageTracker.AgeChronologicalTicks)
		{
			pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
		}
		pawn.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.Initial, cancelInitialization: true);
		bool AgeAllowed(Pawn p, float y)
		{
			if (y > (float)p.kindDef.maxGenerationAge)
			{
				return false;
			}
			if (y < (float)p.kindDef.minGenerationAge)
			{
				return false;
			}
			if (!request.AllowedDevelopmentalStages.Has(LifeStageUtility.CalculateDevelopmentalStage(pawn, years)))
			{
				return false;
			}
			if (request.ExcludeBiologicalAgeRange.HasValue && request.ExcludeBiologicalAgeRange.Value.Includes(y))
			{
				return false;
			}
			if (request.BiologicalAgeRange.HasValue && !request.BiologicalAgeRange.Value.Includes(y))
			{
				return false;
			}
			return true;
		}
	}

	public static int RandomTraitDegree(TraitDef traitDef)
	{
		if (traitDef.degreeDatas.Count == 1)
		{
			return traitDef.degreeDatas[0].degree;
		}
		return traitDef.degreeDatas.RandomElementByWeight((TraitDegreeData dd) => dd.commonality).degree;
	}

	private static void GenerateTraits(Pawn pawn, PawnGenerationRequest request)
	{
		if (pawn.story == null || request.AllowedDevelopmentalStages.Newborn())
		{
			return;
		}
		if (pawn.kindDef.forcedTraits != null)
		{
			foreach (TraitRequirement forcedTrait in pawn.kindDef.forcedTraits)
			{
				pawn.story.traits.GainTrait(new Trait(forcedTrait.def, forcedTrait.degree.GetValueOrDefault(), forced: true));
			}
		}
		if (request.ForcedTraits != null)
		{
			foreach (TraitDef forcedTrait2 in request.ForcedTraits)
			{
				if (forcedTrait2 != null && !pawn.story.traits.HasTrait(forcedTrait2))
				{
					pawn.story.traits.GainTrait(new Trait(forcedTrait2, 0, forced: true));
				}
			}
		}
		if (pawn.story.Childhood?.forcedTraits != null)
		{
			List<BackstoryTrait> forcedTraits = pawn.story.Childhood.forcedTraits;
			for (int i = 0; i < forcedTraits.Count; i++)
			{
				BackstoryTrait te = forcedTraits[i];
				if (te.def == null)
				{
					Log.Error("Null forced trait def on " + pawn.story.Childhood);
				}
				else if (!request.KindDef.disallowedTraits.NotNullAndContains(te.def) && (request.KindDef.disallowedTraitsWithDegree == null || !request.KindDef.disallowedTraitsWithDegree.Any((TraitRequirement t) => t.def == te.def && !t.degree.HasValue)) && !pawn.story.traits.HasTrait(te.def) && (request.ProhibitedTraits == null || !request.ProhibitedTraits.Contains(te.def)))
				{
					pawn.story.traits.GainTrait(new Trait(te.def, te.degree));
				}
			}
		}
		if (pawn.story.Adulthood != null && pawn.story.Adulthood.forcedTraits != null)
		{
			List<BackstoryTrait> forcedTraits2 = pawn.story.Adulthood.forcedTraits;
			for (int num = 0; num < forcedTraits2.Count; num++)
			{
				BackstoryTrait te2 = forcedTraits2[num];
				if (te2.def == null)
				{
					Log.Error("Null forced trait def on " + pawn.story.Adulthood);
				}
				else if (!request.KindDef.disallowedTraits.NotNullAndContains(te2.def) && (request.KindDef.disallowedTraitsWithDegree == null || !request.KindDef.disallowedTraitsWithDegree.Any((TraitRequirement t) => t.def == te2.def && !t.degree.HasValue)) && !pawn.story.traits.HasTrait(te2.def) && (request.ProhibitedTraits == null || !request.ProhibitedTraits.Contains(te2.def)))
				{
					pawn.story.traits.GainTrait(new Trait(te2.def, te2.degree));
				}
			}
		}
		int minimumAgeTraits = request.MinimumAgeTraits;
		int maximumAgeTraits = request.MaximumAgeTraits;
		int count = pawn.story.traits.allTraits.Count;
		int num2 = Mathf.Min(GrowthUtility.GrowthMomentAges.Length, TraitsCountRange.RandomInRange);
		int ageBiologicalYears = pawn.ageTracker.AgeBiologicalYears;
		if (minimumAgeTraits > 0)
		{
			num2 = Mathf.Max(num2, count + minimumAgeTraits);
		}
		if (maximumAgeTraits >= 0)
		{
			num2 = Mathf.Min(num2, count + maximumAgeTraits);
		}
		for (int num3 = 3; num3 <= ageBiologicalYears; num3++)
		{
			if (pawn.story.traits.allTraits.Count >= num2)
			{
				break;
			}
			if (GrowthUtility.IsGrowthBirthday(num3))
			{
				Trait trait = GenerateTraitsFor(pawn, 1, request, growthMomentTrait: true).FirstOrFallback();
				if (trait != null)
				{
					pawn.story.traits.GainTrait(trait);
				}
			}
		}
		if (request.AllowGay && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn)))
		{
			Trait trait2 = new Trait(TraitDefOf.Gay, RandomTraitDegree(TraitDefOf.Gay));
			pawn.story.traits.GainTrait(trait2);
		}
		if (!ModsConfig.BiotechActive || pawn.ageTracker.AgeBiologicalYears >= 13)
		{
			TryGenerateSexualityTraitFor(pawn, request.AllowGay);
		}
	}

	private static bool HasSexualityTrait(Pawn pawn)
	{
		if (!pawn.story.traits.HasTrait(TraitDefOf.Gay) && !pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
		{
			return pawn.story.traits.HasTrait(TraitDefOf.Asexual);
		}
		return true;
	}

	public static void TryGenerateSexualityTraitFor(Pawn pawn, bool allowGay)
	{
		if (!HasSexualityTrait(pawn))
		{
			tmpTraitChances.Clear();
			float second = DefDatabase<TraitDef>.AllDefsListForReading.Where((TraitDef x) => !pawn.story.traits.HasTrait(x) && x != TraitDefOf.Gay && x != TraitDefOf.Asexual && x != TraitDefOf.Bisexual).Sum((TraitDef x) => x.GetGenderSpecificCommonality(pawn.gender));
			tmpTraitChances.Add(new Pair<TraitDef, float>(null, second));
			if (allowGay)
			{
				tmpTraitChances.Add(new Pair<TraitDef, float>(TraitDefOf.Gay, TraitDefOf.Gay.GetGenderSpecificCommonality(pawn.gender)));
			}
			tmpTraitChances.Add(new Pair<TraitDef, float>(TraitDefOf.Bisexual, TraitDefOf.Bisexual.GetGenderSpecificCommonality(pawn.gender)));
			tmpTraitChances.Add(new Pair<TraitDef, float>(TraitDefOf.Asexual, TraitDefOf.Asexual.GetGenderSpecificCommonality(pawn.gender)));
			if (tmpTraitChances.TryRandomElementByWeight((Pair<TraitDef, float> x) => x.Second, out var result) && result.First != null)
			{
				Trait trait = new Trait(result.First, RandomTraitDegree(result.First));
				pawn.story.traits.GainTrait(trait);
			}
			tmpTraitChances.Clear();
		}
	}

	public static List<Trait> GenerateTraitsFor(Pawn pawn, int traitCount, PawnGenerationRequest? req = null, bool growthMomentTrait = false)
	{
		List<Trait> list = new List<Trait>();
		int num = 0;
		while (list.Count < traitCount && ++num < traitCount + 500)
		{
			TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn.gender));
			if (pawn.story.traits.HasTrait(newTraitDef) || TraitListHasDef(list, newTraitDef) || (newTraitDef == TraitDefOf.Gay && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))) || (growthMomentTrait && ModsConfig.BiotechActive && (newTraitDef == TraitDefOf.Gay || newTraitDef == TraitDefOf.Bisexual || newTraitDef == TraitDefOf.Asexual)))
			{
				continue;
			}
			if (req.HasValue)
			{
				PawnGenerationRequest value = req.Value;
				if (value.KindDef.disallowedTraits.NotNullAndContains(newTraitDef) || (value.KindDef.disallowedTraitsWithDegree != null && value.KindDef.disallowedTraitsWithDegree.Any((TraitRequirement t) => t.def == newTraitDef && !t.degree.HasValue)) || (value.KindDef.requiredWorkTags != WorkTags.None && (newTraitDef.disabledWorkTags & value.KindDef.requiredWorkTags) != WorkTags.None) || (newTraitDef == TraitDefOf.Gay && !value.AllowGay) || (value.ProhibitedTraits != null && value.ProhibitedTraits.Contains(newTraitDef)) || (value.Faction != null && Faction.OfPlayerSilentFail != null && value.Faction.HostileTo(Faction.OfPlayer) && !newTraitDef.allowOnHostileSpawn))
				{
					continue;
				}
			}
			if (pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) || (newTraitDef.requiredWorkTypes != null && pawn.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes)) || pawn.WorkTagIsDisabled(newTraitDef.requiredWorkTags) || (newTraitDef.forcedPassions != null && pawn.workSettings != null && newTraitDef.forcedPassions.Any((SkillDef p) => p.IsDisabled(pawn.story.DisabledWorkTagsBackstoryTraitsAndGenes, pawn.GetDisabledWorkTypes(permanentOnly: true)))))
			{
				continue;
			}
			int degree = RandomTraitDegree(newTraitDef);
			if ((pawn.story.Childhood == null || !pawn.story.Childhood.DisallowsTrait(newTraitDef, degree)) && (pawn.story.Adulthood == null || !pawn.story.Adulthood.DisallowsTrait(newTraitDef, degree)))
			{
				Trait trait = new Trait(newTraitDef, degree);
				if ((pawn.kindDef.disallowedTraitsWithDegree.NullOrEmpty() || !pawn.kindDef.disallowedTraitsWithDegree.Any((TraitRequirement t) => t.Matches(trait))) && (pawn.mindState == null || pawn.mindState.mentalBreaker == null || !((pawn.mindState.mentalBreaker.BreakThresholdMinor + trait.OffsetOfStat(StatDefOf.MentalBreakThreshold)) * trait.MultiplierOfStat(StatDefOf.MentalBreakThreshold) > 0.5f)))
				{
					list.Add(trait);
				}
			}
		}
		if (num >= traitCount + 500)
		{
			Log.Warning($"Tried to generate {traitCount} traits for {pawn} over {500} extra times and failed.");
		}
		return list;
	}

	private static bool TraitListHasDef(List<Trait> traits, TraitDef traitDef)
	{
		if (traits.NullOrEmpty())
		{
			return false;
		}
		foreach (Trait trait in traits)
		{
			if (trait.def == traitDef)
			{
				return true;
			}
		}
		return false;
	}

	private static void GenerateGenes(Pawn pawn, XenotypeDef xenotype, PawnGenerationRequest request)
	{
		if (pawn.genes == null)
		{
			return;
		}
		if (ModsConfig.BiotechActive)
		{
			if (!xenotype.doubleXenotypeChances.NullOrEmpty() && Rand.Value < xenotype.doubleXenotypeChances.Sum((XenotypeChance x) => x.chance) && xenotype.doubleXenotypeChances.TryRandomElementByWeight((XenotypeChance x) => x.chance, out var result))
			{
				pawn.genes.SetXenotype(result.xenotype);
			}
			pawn.genes.SetXenotype(xenotype);
			if (Rand.Value < xenotype.generateWithXenogermReplicatingHediffChance && xenotype.xenogermReplicatingDurationLeftDaysRange != FloatRange.Zero)
			{
				Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.XenogermReplicating, pawn);
				HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
				if (hediffComp_Disappears != null)
				{
					hediffComp_Disappears.ticksToDisappear = Mathf.FloorToInt(xenotype.xenogermReplicatingDurationLeftDaysRange.RandomInRange * 60000f);
				}
				pawn.health.AddHediff(hediff);
			}
			if (request.ForcedCustomXenotype != null)
			{
				pawn.genes.xenotypeName = request.ForcedCustomXenotype.name;
				pawn.genes.iconDef = request.ForcedCustomXenotype.IconDef;
				foreach (GeneDef gene in request.ForcedCustomXenotype.genes)
				{
					pawn.genes.AddGene(gene, !request.ForcedCustomXenotype.inheritable);
				}
			}
			if (request.ForcedXenogenes != null)
			{
				foreach (GeneDef forcedXenogene in request.ForcedXenogenes)
				{
					pawn.genes.AddGene(forcedXenogene, xenogene: true);
				}
			}
			if (request.ForcedEndogenes != null)
			{
				foreach (GeneDef forcedEndogene in request.ForcedEndogenes)
				{
					pawn.genes.AddGene(forcedEndogene, xenogene: false);
				}
			}
		}
		if (pawn.genes.GetMelaninGene() == null)
		{
			GeneDef geneDef = PawnSkinColors.RandomSkinColorGene(pawn);
			if (geneDef != null)
			{
				pawn.genes.AddGene(geneDef, xenogene: false);
			}
		}
		if (pawn.genes.GetHairColorGene() == null)
		{
			GeneDef geneDef2 = PawnHairColors.RandomHairColorGeneFor(pawn);
			if (geneDef2 != null)
			{
				pawn.genes.AddGene(geneDef2, xenogene: false);
			}
			else
			{
				pawn.story.HairColor = PawnHairColors.RandomHairColor(pawn, pawn.story.SkinColorBase, pawn.ageTracker.AgeBiologicalYears);
				Log.Error("No hair color gene for " + pawn.LabelShort + ". Getting random color as fallback.");
			}
		}
		if (pawn.kindDef.forcedHairColor.HasValue)
		{
			pawn.story.HairColor = pawn.kindDef.forcedHairColor.Value;
		}
		else if (PawnHairColors.HasGreyHair(pawn, pawn.ageTracker.AgeBiologicalYears))
		{
			pawn.story.HairColor = PawnHairColors.RandomGreyHairColor();
		}
	}

	public static XenotypeDef GetXenotypeForGeneratedPawn(PawnGenerationRequest request)
	{
		if (request.ForcedXenotype != null)
		{
			return request.ForcedXenotype;
		}
		if (request.ForcedCustomXenotype != null)
		{
			return XenotypeDefOf.Baseliner;
		}
		if (Rand.Chance(request.ForceBaselinerChance))
		{
			return XenotypeDefOf.Baseliner;
		}
		if (request.AllowedXenotypes != null && request.AllowedXenotypes.TryRandomElement(out var result))
		{
			return result;
		}
		XenotypesAvailableFor(request.KindDef, null, request.Faction);
		if (request.MustBeCapableOfViolence)
		{
			tmpXenotypeChances.RemoveAll((KeyValuePair<XenotypeDef, float> x) => !x.Key.canGenerateAsCombatant);
		}
		if (tmpXenotypeChances.TryRandomElementByWeight((KeyValuePair<XenotypeDef, float> x) => x.Value, out var result2))
		{
			tmpXenotypeChances.Clear();
			return result2.Key;
		}
		tmpXenotypeChances.Clear();
		return XenotypeDefOf.Baseliner;
	}

	public static void AdjustXenotypeForFactionlessPawn(Pawn pawn, ref PawnGenerationRequest request, ref XenotypeDef xenotype)
	{
		List<CustomXenotype> customXenotypes = Current.Game.customXenotypeDatabase.customXenotypes;
		float num = DefDatabase<XenotypeDef>.AllDefs.Sum((XenotypeDef x) => x.factionlessGenerationWeight);
		float num2 = (float)customXenotypes.Count * 2f;
		XenotypeDef result2;
		if (Rand.Range(0f, num + num2) <= num2 && customXenotypes.TryRandomElement(out var result))
		{
			request.ForcedCustomXenotype = result;
			xenotype = XenotypeDefOf.Baseliner;
		}
		else if (DefDatabase<XenotypeDef>.AllDefs.TryRandomElementByWeight((XenotypeDef x) => x.factionlessGenerationWeight, out result2))
		{
			xenotype = result2;
		}
	}

	public static Dictionary<XenotypeDef, float> XenotypesAvailableFor(PawnKindDef kind, FactionDef factionDef = null, Faction faction = null)
	{
		tmpXenotypeChances.Clear();
		FactionDef factionDef2 = faction?.def ?? factionDef;
		if (kind.useFactionXenotypes && factionDef2?.xenotypeSet != null)
		{
			for (int i = 0; i < factionDef2.xenotypeSet.Count; i++)
			{
				AddOrAdjust(factionDef2.xenotypeSet[i]);
			}
		}
		if (faction?.ideos?.PrimaryIdeo?.memes != null)
		{
			for (int j = 0; j < faction.ideos.PrimaryIdeo.memes.Count; j++)
			{
				MemeDef memeDef = faction.ideos.PrimaryIdeo.memes[j];
				if (memeDef.xenotypeSet != null)
				{
					for (int k = 0; k < memeDef.xenotypeSet.Count; k++)
					{
						AddOrAdjust(memeDef.xenotypeSet[k]);
					}
				}
			}
		}
		if (kind.xenotypeSet != null)
		{
			for (int l = 0; l < kind.xenotypeSet.Count; l++)
			{
				AddOrAdjust(kind.xenotypeSet[l]);
			}
		}
		float num = 1f - tmpXenotypeChances.Sum((KeyValuePair<XenotypeDef, float> x) => x.Value);
		if (num > 0f)
		{
			tmpXenotypeChances.Add(XenotypeDefOf.Baseliner, num);
		}
		return tmpXenotypeChances;
		static void AddOrAdjust(XenotypeChance xenotypeChance)
		{
			if (xenotypeChance.xenotype != XenotypeDefOf.Baseliner)
			{
				if (tmpXenotypeChances.ContainsKey(xenotypeChance.xenotype))
				{
					tmpXenotypeChances[xenotypeChance.xenotype] += xenotypeChance.chance;
				}
				else
				{
					tmpXenotypeChances.Add(xenotypeChance.xenotype, xenotypeChance.chance);
				}
			}
		}
	}

	private static void GenerateBodyType(Pawn pawn, PawnGenerationRequest request)
	{
		pawn.story.bodyType = request.ForceBodyType ?? GetBodyTypeFor(pawn);
	}

	public static BodyTypeDef GetBodyTypeFor(Pawn pawn)
	{
		tmpBodyTypes.Clear();
		if (ModsConfig.BiotechActive && pawn.DevelopmentalStage.Juvenile())
		{
			if (pawn.DevelopmentalStage == DevelopmentalStage.Baby)
			{
				return BodyTypeDefOf.Baby;
			}
			return BodyTypeDefOf.Child;
		}
		if (ModsConfig.BiotechActive && pawn.genes != null)
		{
			List<Gene> genesListForReading = pawn.genes.GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i].def.bodyType.HasValue)
				{
					tmpBodyTypes.Add(genesListForReading[i].def.bodyType.Value.ToBodyType(pawn));
				}
			}
			if (tmpBodyTypes.TryRandomElement(out var result))
			{
				return result;
			}
		}
		if (pawn.story.Adulthood != null)
		{
			return pawn.story.Adulthood.BodyTypeFor(pawn.gender);
		}
		if (Rand.Value < 0.5f)
		{
			return BodyTypeDefOf.Thin;
		}
		if (pawn.gender != Gender.Female)
		{
			return BodyTypeDefOf.Male;
		}
		return BodyTypeDefOf.Female;
	}

	private static void GenerateSkills(Pawn pawn, PawnGenerationRequest request)
	{
		List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			SkillDef skillDef = allDefsListForReading[i];
			int level = FinalLevelOfSkill(pawn, skillDef, request);
			pawn.skills.GetSkill(skillDef).Level = level;
		}
		int minorPassions = 0;
		int majorPassions = 0;
		float num = 5f + Mathf.Clamp(Rand.Gaussian(), -4f, 4f);
		while (num >= 1f)
		{
			if (num >= 1.5f && Rand.Bool)
			{
				majorPassions++;
				num -= 1.5f;
			}
			else
			{
				minorPassions++;
				num -= 1f;
			}
		}
		foreach (SkillRecord skill2 in pawn.skills.skills)
		{
			if (skill2.TotallyDisabled)
			{
				continue;
			}
			foreach (Trait allTrait in pawn.story.traits.allTraits)
			{
				if (allTrait.def.RequiresPassion(skill2.def))
				{
					CreatePassion(skill2, force: true);
				}
			}
		}
		int ageBiologicalYears = pawn.ageTracker.AgeBiologicalYears;
		if (ageBiologicalYears < 13)
		{
			for (int j = 3; j <= ageBiologicalYears; j++)
			{
				if (!GrowthUtility.IsGrowthBirthday(j))
				{
					continue;
				}
				int num2 = Rand.RangeInclusive(0, 3);
				for (int k = 0; k < num2; k++)
				{
					SkillDef skillDef2 = ChoiceLetter_GrowthMoment.PassionOptions(pawn, 1, checkGenes: true).FirstOrDefault();
					if (skillDef2 != null)
					{
						SkillRecord skill = pawn.skills.GetSkill(skillDef2);
						skill.passion = skill.passion.IncrementPassion();
					}
				}
			}
			if (ModsConfig.BiotechActive)
			{
				pawn.ageTracker.TrySimulateGrowthPoints();
			}
			return;
		}
		foreach (SkillRecord item in pawn.skills.skills.OrderByDescending((SkillRecord sr) => sr.GetLevel(includeAptitudes: false)))
		{
			if (item.TotallyDisabled)
			{
				continue;
			}
			bool flag = false;
			foreach (Trait allTrait2 in pawn.story.traits.allTraits)
			{
				if (allTrait2.def.ConflictsWithPassion(item.def))
				{
					flag = true;
					break;
				}
			}
			if (ModsConfig.BiotechActive && pawn.genes != null)
			{
				foreach (Gene item2 in pawn.genes.GenesListForReading)
				{
					if (item2.Active && item2.def.passionMod != null && item2.def.passionMod.modType == PassionMod.PassionModType.DropAll && item2.def.passionMod.skill == item.def)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				CreatePassion(item);
			}
		}
		void CreatePassion(SkillRecord record, bool force = false)
		{
			if (majorPassions > 0)
			{
				record.passion = Passion.Major;
				majorPassions--;
			}
			else if (minorPassions > 0 || force)
			{
				record.passion = Passion.Minor;
				minorPassions--;
			}
		}
	}

	private static int FinalLevelOfSkill(Pawn pawn, SkillDef sk, PawnGenerationRequest request)
	{
		if (request.AllowedDevelopmentalStages.Newborn())
		{
			return 0;
		}
		float num = ((!sk.usuallyDefinedInBackstories) ? Rand.ByCurve(LevelRandomCurve) : ((float)Rand.RangeInclusive(0, 4)));
		foreach (BackstoryDef item in pawn.story.AllBackstories.Where((BackstoryDef bs) => bs != null))
		{
			foreach (SkillGain skillGain in item.skillGains)
			{
				if (skillGain.skill == sk)
				{
					num += (float)skillGain.amount * Rand.Range(1f, 1.4f);
				}
			}
		}
		for (int num2 = 0; num2 < pawn.story.traits.allTraits.Count; num2++)
		{
			Trait trait = pawn.story.traits.allTraits[num2];
			if (trait.Suppressed)
			{
				continue;
			}
			TraitDegreeData currentData = trait.CurrentData;
			if (currentData.skillGains.NullOrEmpty())
			{
				continue;
			}
			for (int num3 = 0; num3 < currentData.skillGains.Count; num3++)
			{
				if (currentData.skillGains[num3].skill == sk)
				{
					num += (float)currentData.skillGains[num3].amount;
					break;
				}
			}
		}
		num *= Rand.Range(1f, AgeSkillMaxFactorCurve.Evaluate(pawn.ageTracker.AgeBiologicalYears));
		num *= AgeSkillFactor.Evaluate(pawn.ageTracker.AgeBiologicalYears);
		num = LevelFinalAdjustmentCurve.Evaluate(num);
		if (num > 0f)
		{
			num += (float)pawn.kindDef.extraSkillLevels;
		}
		if (pawn.kindDef.skills != null)
		{
			foreach (SkillRange skill in pawn.kindDef.skills)
			{
				if (skill.Skill == sk)
				{
					if (num < (float)skill.Range.min || num > (float)skill.Range.max)
					{
						num = skill.Range.RandomInRange;
					}
					break;
				}
			}
		}
		return Mathf.Clamp(Mathf.RoundToInt(num), 0, 20);
	}

	public static void PostProcessGeneratedGear(Thing gear, Pawn pawn)
	{
		CompQuality compQuality = gear.TryGetComp<CompQuality>();
		if (compQuality != null)
		{
			QualityCategory qualityCategory = QualityUtility.GenerateQualityGeneratingPawn(pawn.kindDef, gear.def);
			if (pawn.royalty != null && pawn.Faction != null)
			{
				RoyalTitleDef currentTitle = pawn.royalty.GetCurrentTitle(pawn.Faction);
				if (currentTitle != null)
				{
					qualityCategory = (QualityCategory)Mathf.Clamp((int)qualityCategory, (int)currentTitle.requiredMinimumApparelQuality, 6);
				}
			}
			compQuality.SetQuality(qualityCategory, ArtGenerationContext.Outsider);
		}
		if (gear.def.useHitPoints)
		{
			float randomInRange = pawn.kindDef.gearHealthRange.RandomInRange;
			if (randomInRange < 1f)
			{
				int b = Mathf.RoundToInt(randomInRange * (float)gear.MaxHitPoints);
				b = Mathf.Max(1, b);
				gear.HitPoints = b;
			}
		}
	}

	private static void GeneratePawnRelations(Pawn pawn, ref PawnGenerationRequest request)
	{
		if (!pawn.RaceProps.Humanlike)
		{
			return;
		}
		Pawn[] array = PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead.Where((Pawn x) => x.def == pawn.def).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		int num = 0;
		Pawn[] array2 = array;
		foreach (Pawn pawn2 in array2)
		{
			if (pawn2.Discarded)
			{
				Log.Warning("Warning during generating pawn relations for " + pawn?.ToString() + ": Pawn " + pawn2?.ToString() + " is discarded, yet he was yielded by PawnUtility. Discarding a pawn means that he is no longer managed by anything.");
			}
			else if (pawn2.Faction != null && pawn2.Faction.IsPlayer)
			{
				num++;
			}
		}
		float num3 = 45f;
		num3 += (float)num * 2.7f;
		PawnGenerationRequest localReq = request;
		Pair<Pawn, PawnRelationDef> pair = GenerateSamples(array, relationsGeneratableBlood, 40).RandomElementByWeightWithDefault((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num3 * 40f / (float)(array.Length * relationsGeneratableBlood.Length));
		if (pair.First != null)
		{
			pair.Second.Worker.CreateRelation(pawn, pair.First, ref request);
		}
		if (pawn.kindDef.generateInitialNonFamilyRelations)
		{
			Pair<Pawn, PawnRelationDef> pair2 = GenerateSamples(array, relationsGeneratableNonblood, 40).RandomElementByWeightWithDefault((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num3 * 40f / (float)(array.Length * relationsGeneratableNonblood.Length));
			if (pair2.First != null)
			{
				pair2.Second.Worker.CreateRelation(pawn, pair2.First, ref request);
			}
		}
	}

	private static Pair<Pawn, PawnRelationDef>[] GenerateSamples(Pawn[] pawns, PawnRelationDef[] relations, int count)
	{
		Pair<Pawn, PawnRelationDef>[] array = new Pair<Pawn, PawnRelationDef>[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = new Pair<Pawn, PawnRelationDef>(pawns[Rand.Range(0, pawns.Length)], relations[Rand.Range(0, relations.Length)]);
		}
		return array;
	}

	private static void AddRequiredScars(Pawn pawn)
	{
		if (pawn.ideo == null || pawn.ideo.Ideo == null || pawn.health == null || (pawn.story != null && pawn.story.traits != null && pawn.story.traits.HasTrait(TraitDefOf.Wimp)) || !Rand.Chance(ScarChanceFromAgeYearsCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat)))
		{
			return;
		}
		int num = 0;
		foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
		{
			if (hediff.def == HediffDefOf.Scarification)
			{
				num++;
			}
		}
		int num2 = pawn.ideo.Ideo.RequiredScars;
		if (pawn.Faction != null && pawn.Faction.IsPlayer && !Rand.Chance(0.5f))
		{
			num2 = Rand.RangeInclusive(0, num2 - 1);
		}
		for (int i = num; i < num2; i++)
		{
			List<BodyPartRecord> list = (from p in JobDriver_Scarify.GetPartsToApplyOn(pawn)
				where JobDriver_Scarify.AvailableOnNow(pawn, p)
				select p).ToList();
			if (list.Count != 0)
			{
				BodyPartRecord part = list.RandomElement();
				JobDriver_Scarify.Scarify(pawn, part);
				continue;
			}
			break;
		}
	}

	private static void AddBlindness(Pawn pawn)
	{
		if (pawn.ideo == null || pawn.ideo.Ideo == null || pawn.health == null || !Rand.Chance(pawn.ideo.Ideo.BlindPawnChance))
		{
			return;
		}
		foreach (BodyPartRecord item in pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.SightSource))
		{
			if (!pawn.health.hediffSet.PartIsMissing(item))
			{
				Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn);
				hediff_MissingPart.lastInjury = HediffDefOf.Cut;
				hediff_MissingPart.Part = item;
				hediff_MissingPart.IsFresh = false;
				pawn.health.AddHediff(hediff_MissingPart, item);
			}
		}
	}

	[DebugOutput("Performance", false)]
	public static void PawnGenerationHistogram()
	{
		DebugHistogram debugHistogram = new DebugHistogram((from x in Enumerable.Range(1, 20)
			select (float)x * 10f).ToArray());
		for (int num = 0; num < 100; num++)
		{
			long timestamp = Stopwatch.GetTimestamp();
			Pawn pawn = GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true));
			debugHistogram.Add((Stopwatch.GetTimestamp() - timestamp) * 1000 / Stopwatch.Frequency);
			pawn.Destroy();
		}
		debugHistogram.Display();
	}
}
