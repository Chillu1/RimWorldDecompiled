using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class PawnGenerator
	{
		private struct PawnGenerationStatus
		{
			public Pawn Pawn
			{
				get;
				private set;
			}

			public List<Pawn> PawnsGeneratedInTheMeantime
			{
				get;
				private set;
			}

			public PawnGenerationStatus(Pawn pawn, List<Pawn> pawnsGeneratedInTheMeantime)
			{
				this = default(PawnGenerationStatus);
				Pawn = pawn;
				PawnsGeneratedInTheMeantime = pawnsGeneratedInTheMeantime;
			}
		}

		private static List<PawnGenerationStatus> pawnsBeingGenerated = new List<PawnGenerationStatus>();

		private static PawnRelationDef[] relationsGeneratableBlood = DefDatabase<PawnRelationDef>.AllDefsListForReading.Where((PawnRelationDef rel) => rel.familyByBloodRelation && rel.generationChanceFactor > 0f).ToArray();

		private static PawnRelationDef[] relationsGeneratableNonblood = DefDatabase<PawnRelationDef>.AllDefsListForReading.Where((PawnRelationDef rel) => !rel.familyByBloodRelation && rel.generationChanceFactor > 0f).ToArray();

		public const float MaxStartMinorMentalBreakThreshold = 0.5f;

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

		private static readonly SimpleCurve AgeSkillMaxFactorCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(10f, 0.7f),
			new CurvePoint(35f, 1f),
			new CurvePoint(60f, 1.6f)
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

		public static Pawn GeneratePawn(PawnKindDef kindDef, Faction faction = null)
		{
			return GeneratePawn(new PawnGenerationRequest(kindDef, faction));
		}

		public static Pawn GeneratePawn(PawnGenerationRequest request)
		{
			try
			{
				Pawn pawn = GenerateOrRedressPawnInternal(request);
				if (pawn != null && !request.AllowDead && pawn.health.hediffSet.hediffs.Any())
				{
					bool dead = pawn.Dead;
					bool downed = pawn.Downed;
					pawn.health.hediffSet.DirtyCache();
					pawn.health.CheckForStateChange(null, null);
					if (pawn.Dead)
					{
						Log.Error("Pawn was generated dead but the pawn generation request specified the pawn must be alive. This shouldn't ever happen even if we ran out of tries because null pawn should have been returned instead in this case. Resetting health...\npawn.Dead=" + pawn.Dead.ToString() + " pawn.Downed=" + pawn.Downed.ToString() + " deadBefore=" + dead.ToString() + " downedBefore=" + downed.ToString() + "\nrequest=" + request);
						pawn.health.Reset();
					}
				}
				if (pawn.Faction == Faction.OfPlayerSilentFail && !pawn.IsQuestLodger())
				{
					Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedColonist);
				}
				return pawn;
			}
			catch (Exception arg)
			{
				Log.Error("Error while generating pawn. Rethrowing. Exception: " + arg);
				throw;
			}
			finally
			{
			}
		}

		private static Pawn GenerateOrRedressPawnInternal(PawnGenerationRequest request)
		{
			Pawn result = null;
			if (!request.Newborn && !request.ForceGenerateNewPawn)
			{
				if (request.ForceRedressWorldPawnIfFormerColonist && (from x in GetValidCandidatesToRedress(request)
					where PawnUtility.EverBeenColonistOrTameAnimal(x)
					select x).TryRandomElementByWeight((Pawn x) => WorldPawnSelectionWeight(x), out result))
				{
					RedressPawn(result, request);
					Find.WorldPawns.RemovePawn(result);
				}
				if (result == null && request.Inhabitant && request.Tile != -1)
				{
					Settlement settlement = Find.WorldObjects.WorldObjectAt<Settlement>(request.Tile);
					if (settlement != null && settlement.previouslyGeneratedInhabitants.Any() && (from x in GetValidCandidatesToRedress(request)
						where settlement.previouslyGeneratedInhabitants.Contains(x)
						select x).TryRandomElementByWeight((Pawn x) => WorldPawnSelectionWeight(x), out result))
					{
						RedressPawn(result, request);
						Find.WorldPawns.RemovePawn(result);
					}
				}
				if (result == null && Rand.Chance(ChanceToRedressAnyWorldPawn(request)) && GetValidCandidatesToRedress(request).TryRandomElementByWeight((Pawn x) => WorldPawnSelectionWeight(x), out result))
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
				if (request.Inhabitant && request.Tile != -1)
				{
					Find.WorldObjects.WorldObjectAt<Settlement>(request.Tile)?.previouslyGeneratedInhabitants.Add(result);
				}
			}
			else
			{
				redressed = true;
			}
			if (Find.Scenario != null)
			{
				Find.Scenario.Notify_PawnGenerated(result, request.Context, redressed);
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
						if (Rand.Chance(hediff.def.removeOnRedressChanceByDaysCurve.Evaluate(x)))
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
				if (pawn.Faction != request.Faction)
				{
					pawn.SetFaction(request.Faction);
				}
				GenerateGearFor(pawn, request);
				if (pawn.guest != null)
				{
					pawn.guest.SetGuestStatus(null);
				}
				if (pawn.needs != null)
				{
					pawn.needs.SetInitialLevels();
				}
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
			if (!request.AllowDead && (pawn.Dead || pawn.Destroyed))
			{
				return false;
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
			List<SkillRange> skills = request.KindDef.skills;
			if (skills != null)
			{
				for (int i = 0; i < skills.Count; i++)
				{
					SkillRecord skill = pawn.skills.GetSkill(skills[i].Skill);
					if (skill.TotallyDisabled)
					{
						return false;
					}
					if (skill.Level < skills[i].Range.min || skill.Level > skills[i].Range.max)
					{
						return false;
					}
				}
			}
			if (request.ForcedTraits != null)
			{
				foreach (TraitDef forcedTrait in request.ForcedTraits)
				{
					if (!pawn.story.traits.HasTrait(forcedTrait))
					{
						return false;
					}
				}
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
			if (request.FixedGender.HasValue && pawn.gender != request.FixedGender)
			{
				return false;
			}
			if (request.FixedLastName != null && (!(pawn.Name is NameTriple) || ((NameTriple)pawn.Name).Last != request.FixedLastName))
			{
				return false;
			}
			if (request.FixedMelanin.HasValue && pawn.story != null && pawn.story.melanin != request.FixedMelanin)
			{
				return false;
			}
			if (request.FixedTitle != null && (pawn.royalty == null || !pawn.royalty.HasTitle(request.FixedTitle)))
			{
				return false;
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
			if (request.RedressValidator != null && !request.RedressValidator(pawn))
			{
				return false;
			}
			return true;
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
				Log.Error("Pawn generation error: " + error + " Too many tries (" + 120 + "), returning null. Generation request: " + request);
				return null;
			}
			return pawn;
		}

		private static Pawn TryGenerateNewPawnInternal(ref PawnGenerationRequest request, out string error, bool ignoreScenarioRequirements, bool ignoreValidator)
		{
			error = null;
			Pawn pawn = (Pawn)ThingMaker.MakeThing(request.KindDef.race);
			pawnsBeingGenerated.Add(new PawnGenerationStatus(pawn, null));
			try
			{
				pawn.kindDef = request.KindDef;
				pawn.SetFactionDirect(request.Faction);
				PawnComponentsUtility.CreateInitialComponents(pawn);
				if (request.FixedGender.HasValue)
				{
					pawn.gender = request.FixedGender.Value;
				}
				else if (pawn.RaceProps.hasGenders)
				{
					if (Rand.Value < 0.5f)
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
				if (!request.Newborn && request.CanGeneratePawnRelations)
				{
					GeneratePawnRelations(pawn, ref request);
				}
				if (pawn.RaceProps.Humanlike)
				{
					Faction faction;
					FactionDef factionType = (request.Faction != null) ? request.Faction.def : ((!Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter: false, allowDefeated: true)) ? Faction.OfAncients.def : faction.def);
					pawn.story.melanin = (request.FixedMelanin.HasValue ? request.FixedMelanin.Value : PawnSkinColors.RandomMelanin(request.Faction));
					pawn.story.crownType = ((Rand.Value < 0.5f) ? CrownType.Average : CrownType.Narrow);
					pawn.story.hairColor = PawnHairColors.RandomHairColor(pawn.story.SkinColor, pawn.ageTracker.AgeBiologicalYears);
					PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo(pawn, request.FixedLastName, factionType);
					if (pawn.story != null)
					{
						if (request.FixedBirthName != null)
						{
							pawn.story.birthLastName = request.FixedBirthName;
						}
						else if (pawn.Name is NameTriple)
						{
							pawn.story.birthLastName = ((NameTriple)pawn.Name).Last;
						}
					}
					pawn.story.hairDef = PawnHairChooser.RandomHairDefFor(pawn, factionType);
					GenerateTraits(pawn, request);
					GenerateBodyType(pawn);
					GenerateSkills(pawn);
				}
				if (pawn.RaceProps.Animal && request.Faction != null && request.Faction.IsPlayer)
				{
					pawn.training.SetWantedRecursive(TrainableDefOf.Tameness, checkOn: true);
					pawn.training.Train(TrainableDefOf.Tameness, null, complete: true);
				}
				GenerateInitialHediffs(pawn, request);
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
				if (royalTitleDef != null)
				{
					Faction faction2 = (request.Faction != null && request.Faction.def.HasRoyalTitles) ? request.Faction : Find.FactionManager.RandomRoyalFaction();
					pawn.royalty.SetTitle(faction2, royalTitleDef, grantRewards: false);
					int amount = 0;
					if (royalTitleDef.GetNextTitle(faction2) != null)
					{
						amount = Rand.Range(0, royalTitleDef.GetNextTitle(faction2).favorCost - 1);
					}
					pawn.royalty.SetFavor(faction2, amount);
					int num = royalTitleDef.MaxAllowedPsychicAmplifierLevel(faction2.def);
					if (num > 0)
					{
						Hediff_ImplantWithLevel hediff_ImplantWithLevel = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_ImplantWithLevel;
						pawn.health.AddHediff(hediff_ImplantWithLevel);
						hediff_ImplantWithLevel.SetLevelTo(num);
					}
				}
				if (pawn.royalty != null)
				{
					pawn.royalty.allowRoomRequirements = request.KindDef.allowRoyalRoomRequirements;
					pawn.royalty.allowApparelRequirements = request.KindDef.allowRoyalApparelRequirements;
				}
				if (pawn.workSettings != null && request.Faction != null && request.Faction.IsPlayer)
				{
					pawn.workSettings.EnableAndInitialize();
				}
				if (request.Faction != null && pawn.RaceProps.Animal)
				{
					pawn.GenerateNecessaryName();
				}
				if (Find.Scenario != null)
				{
					Find.Scenario.Notify_NewPawnGenerating(pawn, request.Context);
				}
				if (!request.AllowDead && (pawn.Dead || pawn.Destroyed))
				{
					DiscardGeneratedPawn(pawn);
					error = "Generated dead pawn.";
					return null;
				}
				if (!request.AllowDowned && pawn.Downed)
				{
					DiscardGeneratedPawn(pawn);
					error = "Generated downed pawn.";
					return null;
				}
				if (request.MustBeCapableOfViolence && ((pawn.story != null && pawn.WorkTagIsDisabled(WorkTags.Violent)) || (pawn.RaceProps.ToolUser && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))))
				{
					DiscardGeneratedPawn(pawn);
					error = "Generated pawn incapable of violence.";
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
				if (!request.Newborn)
				{
					GenerateGearFor(pawn, request);
				}
				if (!ignoreValidator && request.ValidatorPostGear != null && !request.ValidatorPostGear(pawn))
				{
					DiscardGeneratedPawn(pawn);
					error = "Generated pawn didn't pass validator check (post-gear).";
					return null;
				}
				for (int i = 0; i < pawnsBeingGenerated.Count - 1; i++)
				{
					if (pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime == null)
					{
						pawnsBeingGenerated[i] = new PawnGenerationStatus(pawnsBeingGenerated[i].Pawn, new List<Pawn>());
					}
					pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime.Add(pawn);
				}
				return pawn;
			}
			finally
			{
				pawnsBeingGenerated.RemoveLast();
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
			PawnWeaponGenerator.TryGenerateWeaponFor(pawn, request);
			PawnInventoryGenerator.GenerateInventoryFor(pawn, request);
		}

		private static void GenerateInitialHediffs(Pawn pawn, PawnGenerationRequest request)
		{
			int num = 0;
			while (true)
			{
				AgeInjuryUtility.GenerateRandomOldAgeInjuries(pawn, !request.AllowDead);
				PawnTechHediffsGenerator.GenerateTechHediffsFor(pawn);
				if (request.AllowAddictions)
				{
					PawnAddictionHediffsGenerator.GenerateAddictionsAndTolerancesFor(pawn);
				}
				if ((request.AllowDead && pawn.Dead) || request.AllowDowned || !pawn.Downed)
				{
					break;
				}
				pawn.health.Reset();
				num++;
				if (num > 80)
				{
					Log.Warning("Could not generate old age injuries for " + pawn.ThingID + " of age " + pawn.ageTracker.AgeBiologicalYears + " that allow pawn to move after " + 80 + " tries. request=" + request);
					break;
				}
			}
			if (pawn.Dead || (request.Faction != null && request.Faction.IsPlayer))
			{
				return;
			}
			int num2 = 0;
			while (true)
			{
				if (pawn.health.HasHediffsNeedingTend())
				{
					num2++;
					if (num2 > 10000)
					{
						break;
					}
					TendUtility.DoTend(null, pawn, null);
					continue;
				}
				return;
			}
			Log.Error("Too many iterations.");
		}

		private static void GenerateRandomAge(Pawn pawn, PawnGenerationRequest request)
		{
			if (request.FixedBiologicalAge.HasValue && request.FixedChronologicalAge.HasValue && request.FixedBiologicalAge > request.FixedChronologicalAge)
			{
				Log.Warning("Tried to generate age for pawn " + pawn + ", but pawn generation request demands biological age (" + request.FixedBiologicalAge + ") to be greater than chronological age (" + request.FixedChronologicalAge + ").");
			}
			if (request.Newborn)
			{
				pawn.ageTracker.AgeBiologicalTicks = 0L;
			}
			else if (request.FixedBiologicalAge.HasValue)
			{
				pawn.ageTracker.AgeBiologicalTicks = (long)(request.FixedBiologicalAge.Value * 3600000f);
			}
			else
			{
				float num = 0f;
				int num2 = 0;
				do
				{
					num = ((pawn.RaceProps.ageGenerationCurve != null) ? ((float)Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.ageGenerationCurve))) : ((!pawn.RaceProps.IsMechanoid) ? (Rand.ByCurve(DefaultAgeGenerationCurve) * pawn.RaceProps.lifeExpectancy) : Rand.Range(0f, 2500f)));
					num2++;
					if (num2 > 300)
					{
						Log.Error("Tried 300 times to generate age for " + pawn);
						break;
					}
				}
				while (num > (float)pawn.kindDef.maxGenerationAge || num < (float)pawn.kindDef.minGenerationAge);
				pawn.ageTracker.AgeBiologicalTicks = (long)(num * 3600000f) + Rand.Range(0, 3600000);
			}
			if (request.Newborn)
			{
				pawn.ageTracker.AgeChronologicalTicks = 0L;
			}
			else if (request.FixedChronologicalAge.HasValue)
			{
				pawn.ageTracker.AgeChronologicalTicks = (long)(request.FixedChronologicalAge.Value * 3600000f);
			}
			else
			{
				int num3;
				if (request.CertainlyBeenInCryptosleep || Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
				{
					float value = Rand.Value;
					if (value < 0.7f)
					{
						num3 = Rand.Range(0, 100);
					}
					else if (value < 0.95f)
					{
						num3 = Rand.Range(100, 1000);
					}
					else
					{
						int max = GenDate.Year(GenTicks.TicksAbs, 0f) - 2026 - pawn.ageTracker.AgeBiologicalYears;
						num3 = Rand.Range(1000, max);
					}
				}
				else
				{
					num3 = 0;
				}
				long num4 = GenTicks.TicksAbs - pawn.ageTracker.AgeBiologicalTicks;
				num4 -= (long)num3 * 3600000L;
				pawn.ageTracker.BirthAbsTicks = num4;
			}
			if (pawn.ageTracker.AgeBiologicalTicks > pawn.ageTracker.AgeChronologicalTicks)
			{
				pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
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
			if (pawn.story == null)
			{
				return;
			}
			if (request.ForcedTraits != null)
			{
				foreach (TraitDef forcedTrait in request.ForcedTraits)
				{
					if (forcedTrait != null)
					{
						pawn.story.traits.GainTrait(new Trait(forcedTrait, 0, forced: true));
					}
				}
			}
			if (pawn.story.childhood.forcedTraits != null)
			{
				List<TraitEntry> forcedTraits = pawn.story.childhood.forcedTraits;
				for (int i = 0; i < forcedTraits.Count; i++)
				{
					TraitEntry traitEntry = forcedTraits[i];
					if (traitEntry.def == null)
					{
						Log.Error("Null forced trait def on " + pawn.story.childhood);
					}
					else if ((request.KindDef.disallowedTraits == null || !request.KindDef.disallowedTraits.Contains(traitEntry.def)) && !pawn.story.traits.HasTrait(traitEntry.def) && (request.ProhibitedTraits == null || !request.ProhibitedTraits.Contains(traitEntry.def)))
					{
						pawn.story.traits.GainTrait(new Trait(traitEntry.def, traitEntry.degree));
					}
				}
			}
			if (pawn.story.adulthood != null && pawn.story.adulthood.forcedTraits != null)
			{
				List<TraitEntry> forcedTraits2 = pawn.story.adulthood.forcedTraits;
				for (int j = 0; j < forcedTraits2.Count; j++)
				{
					TraitEntry traitEntry2 = forcedTraits2[j];
					if (traitEntry2.def == null)
					{
						Log.Error("Null forced trait def on " + pawn.story.adulthood);
					}
					else if ((request.KindDef.disallowedTraits == null || !request.KindDef.disallowedTraits.Contains(traitEntry2.def)) && !pawn.story.traits.HasTrait(traitEntry2.def) && (request.ProhibitedTraits == null || !request.ProhibitedTraits.Contains(traitEntry2.def)))
					{
						pawn.story.traits.GainTrait(new Trait(traitEntry2.def, traitEntry2.degree));
					}
				}
			}
			int num = Rand.RangeInclusive(2, 3);
			if (request.AllowGay && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn)))
			{
				Trait trait = new Trait(TraitDefOf.Gay, RandomTraitDegree(TraitDefOf.Gay));
				pawn.story.traits.GainTrait(trait);
			}
			while (pawn.story.traits.allTraits.Count < num)
			{
				TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn.gender));
				if (pawn.story.traits.HasTrait(newTraitDef) || (request.KindDef.disallowedTraits != null && request.KindDef.disallowedTraits.Contains(newTraitDef)) || (newTraitDef == TraitDefOf.Gay && (!request.AllowGay || LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))) || (request.ProhibitedTraits != null && request.ProhibitedTraits.Contains(newTraitDef)) || (request.Faction != null && Faction.OfPlayerSilentFail != null && request.Faction.HostileTo(Faction.OfPlayer) && !newTraitDef.allowOnHostileSpawn) || pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) || (newTraitDef.requiredWorkTypes != null && pawn.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes)) || pawn.WorkTagIsDisabled(newTraitDef.requiredWorkTags) || (newTraitDef.forcedPassions != null && pawn.workSettings != null && newTraitDef.forcedPassions.Any((SkillDef p) => p.IsDisabled(pawn.story.DisabledWorkTagsBackstoryAndTraits, pawn.GetDisabledWorkTypes(permanentOnly: true)))))
				{
					continue;
				}
				int degree = RandomTraitDegree(newTraitDef);
				if (!pawn.story.childhood.DisallowsTrait(newTraitDef, degree) && (pawn.story.adulthood == null || !pawn.story.adulthood.DisallowsTrait(newTraitDef, degree)))
				{
					Trait trait2 = new Trait(newTraitDef, degree);
					if (pawn.mindState == null || pawn.mindState.mentalBreaker == null || !((pawn.mindState.mentalBreaker.BreakThresholdMinor + trait2.OffsetOfStat(StatDefOf.MentalBreakThreshold)) * trait2.MultiplierOfStat(StatDefOf.MentalBreakThreshold) > 0.5f))
					{
						pawn.story.traits.GainTrait(trait2);
					}
				}
			}
		}

		private static void GenerateBodyType(Pawn pawn)
		{
			if (pawn.story.adulthood != null)
			{
				pawn.story.bodyType = pawn.story.adulthood.BodyTypeFor(pawn.gender);
			}
			else if (Rand.Value < 0.5f)
			{
				pawn.story.bodyType = BodyTypeDefOf.Thin;
			}
			else
			{
				pawn.story.bodyType = ((pawn.gender == Gender.Female) ? BodyTypeDefOf.Female : BodyTypeDefOf.Male);
			}
		}

		private static void GenerateSkills(Pawn pawn)
		{
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				SkillDef skillDef = allDefsListForReading[i];
				int num = FinalLevelOfSkill(pawn, skillDef);
				SkillRecord skill = pawn.skills.GetSkill(skillDef);
				skill.Level = num;
				if (skill.TotallyDisabled)
				{
					continue;
				}
				bool flag = false;
				bool flag2 = false;
				foreach (Trait allTrait in pawn.story.traits.allTraits)
				{
					if (allTrait.def.ConflictsWithPassion(skillDef))
					{
						flag = true;
						flag2 = false;
						break;
					}
					if (allTrait.def.RequiresPassion(skillDef))
					{
						flag2 = true;
					}
				}
				if (!flag)
				{
					float num2 = (float)num * 0.11f;
					float value = Rand.Value;
					if (flag2 || value < num2)
					{
						if (value < num2 * 0.2f)
						{
							skill.passion = Passion.Major;
						}
						else
						{
							skill.passion = Passion.Minor;
						}
					}
				}
				skill.xpSinceLastLevel = Rand.Range(skill.XpRequiredForLevelUp * 0.1f, skill.XpRequiredForLevelUp * 0.9f);
			}
		}

		private static int FinalLevelOfSkill(Pawn pawn, SkillDef sk)
		{
			float num = (!sk.usuallyDefinedInBackstories) ? Rand.ByCurve(LevelRandomCurve) : ((float)Rand.RangeInclusive(0, 4));
			foreach (Backstory item in pawn.story.AllBackstories.Where((Backstory bs) => bs != null))
			{
				foreach (KeyValuePair<SkillDef, int> item2 in item.skillGainsResolved)
				{
					if (item2.Key == sk)
					{
						num += (float)item2.Value * Rand.Range(1f, 1.4f);
					}
				}
			}
			for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
			{
				int value = 0;
				if (pawn.story.traits.allTraits[i].CurrentData.skillGains.TryGetValue(sk, out value))
				{
					num += (float)value;
				}
			}
			float num2 = Rand.Range(1f, AgeSkillMaxFactorCurve.Evaluate(pawn.ageTracker.AgeBiologicalYears));
			num *= num2;
			num = LevelFinalAdjustmentCurve.Evaluate(num);
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
				QualityCategory q = QualityUtility.GenerateQualityGeneratingPawn(pawn.kindDef);
				if (pawn.royalty != null && pawn.Faction != null)
				{
					RoyalTitleDef currentTitle = pawn.royalty.GetCurrentTitle(pawn.Faction);
					if (currentTitle != null)
					{
						q = (QualityCategory)Mathf.Clamp((int)QualityUtility.GenerateQualityGeneratingPawn(pawn.kindDef), (int)currentTitle.requiredMinimumApparelQuality, 6);
					}
				}
				compQuality.SetQuality(q, ArtGenerationContext.Outsider);
			}
			if (gear.def.useHitPoints)
			{
				float randomInRange = pawn.kindDef.gearHealthRange.RandomInRange;
				if (randomInRange < 1f)
				{
					int b = Mathf.RoundToInt(randomInRange * (float)gear.MaxHitPoints);
					b = (gear.HitPoints = Mathf.Max(1, b));
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
					Log.Warning("Warning during generating pawn relations for " + pawn + ": Pawn " + pawn2 + " is discarded, yet he was yielded by PawnUtility. Discarding a pawn means that he is no longer managed by anything.");
				}
				else if (pawn2.Faction != null && pawn2.Faction.IsPlayer)
				{
					num++;
				}
			}
			float num2 = 45f;
			num2 += (float)num * 2.7f;
			PawnGenerationRequest localReq = request;
			Pair<Pawn, PawnRelationDef> pair = GenerateSamples(array, relationsGeneratableBlood, 40).RandomElementByWeightWithDefault((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num2 * 40f / (float)(array.Length * relationsGeneratableBlood.Length));
			if (pair.First != null)
			{
				pair.Second.Worker.CreateRelation(pawn, pair.First, ref request);
			}
			Pair<Pawn, PawnRelationDef> pair2 = GenerateSamples(array, relationsGeneratableNonblood, 40).RandomElementByWeightWithDefault((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num2 * 40f / (float)(array.Length * relationsGeneratableNonblood.Length));
			if (pair2.First != null)
			{
				pair2.Second.Worker.CreateRelation(pawn, pair2.First, ref request);
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

		[DebugOutput("Performance", false)]
		public static void PawnGenerationHistogram()
		{
			DebugHistogram debugHistogram = new DebugHistogram((from x in Enumerable.Range(1, 20)
				select (float)x * 10f).ToArray());
			for (int i = 0; i < 100; i++)
			{
				long timestamp = Stopwatch.GetTimestamp();
				Pawn pawn = GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true));
				debugHistogram.Add((Stopwatch.GetTimestamp() - timestamp) * 1000 / Stopwatch.Frequency);
				pawn.Destroy();
			}
			debugHistogram.Display();
		}
	}
}
