using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class RelationsUtility
{
	public static bool PawnsKnowEachOther(Pawn p1, Pawn p2)
	{
		if (p1.Faction != null && p1.Faction == p2.Faction)
		{
			return true;
		}
		if (p1.RaceProps.IsFlesh && p1.relations.DirectRelations.Find((DirectPawnRelation x) => x.otherPawn == p2) != null)
		{
			return true;
		}
		if (p2.RaceProps.IsFlesh && p2.relations.DirectRelations.Find((DirectPawnRelation x) => x.otherPawn == p1) != null)
		{
			return true;
		}
		if (HasAnySocialMemoryWith(p1, p2))
		{
			return true;
		}
		if (HasAnySocialMemoryWith(p2, p1))
		{
			return true;
		}
		return false;
	}

	public static bool IsDisfigured(Pawn pawn, Pawn forPawn = null, bool ignoreSightSources = false)
	{
		bool flag = forPawn == null || forPawn.Ideo == null || forPawn.Ideo.RequiredScars == 0;
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if ((hediffs[i].def == HediffDefOf.Scarification && !flag) || hediffs[i].Part == null || !hediffs[i].Part.def.beautyRelated)
			{
				continue;
			}
			if (hediffs[i] is Hediff_MissingPart hediff_MissingPart)
			{
				if ((ignoreSightSources && hediff_MissingPart.Part.def.tags.Contains(BodyPartTagDefOf.SightSource)) || pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(hediff_MissingPart.Part))
				{
					continue;
				}
			}
			else if (!(hediffs[i] is Hediff_Injury))
			{
				continue;
			}
			return true;
		}
		return false;
	}

	public static bool TryDevelopBondRelation(Pawn humanlike, Pawn animal, float baseChance)
	{
		if (!animal.IsAnimal)
		{
			return false;
		}
		if (animal.Faction == Faction.OfPlayer && humanlike.IsQuestLodger())
		{
			return false;
		}
		TrainabilityDef trainability = TrainableUtility.GetTrainability(animal);
		if (trainability == null || trainability.intelligenceOrder < TrainabilityDefOf.Intermediate.intelligenceOrder)
		{
			return false;
		}
		if (humanlike.relations.DirectRelationExists(PawnRelationDefOf.Bond, animal))
		{
			return false;
		}
		if (animal.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, (Pawn x) => x.Spawned) != null)
		{
			return false;
		}
		if (humanlike.story.traits.HasTrait(TraitDefOf.Psychopath) || humanlike.Inhumanized())
		{
			return false;
		}
		if (!new HistoryEvent(HistoryEventDefOf.Bonded, humanlike.Named(HistoryEventArgsNames.Doer), animal.Named(HistoryEventArgsNames.Victim)).DoerWillingToDo())
		{
			return false;
		}
		int num = 0;
		List<DirectPawnRelation> directRelations = animal.relations.DirectRelations;
		for (int num2 = 0; num2 < directRelations.Count; num2++)
		{
			if (directRelations[num2].def == PawnRelationDefOf.Bond && !directRelations[num2].otherPawn.Dead)
			{
				num++;
			}
		}
		int num3 = 0;
		List<DirectPawnRelation> directRelations2 = humanlike.relations.DirectRelations;
		for (int num4 = 0; num4 < directRelations2.Count; num4++)
		{
			if (directRelations2[num4].def == PawnRelationDefOf.Bond && !directRelations2[num4].otherPawn.Dead)
			{
				num3++;
			}
		}
		if (num > 0)
		{
			baseChance *= Mathf.Pow(0.2f, num);
		}
		if (num3 > 0)
		{
			baseChance *= Mathf.Pow(0.55f, num3);
		}
		baseChance *= humanlike.GetStatValue(StatDefOf.BondAnimalChanceFactor);
		if (Rand.Value < baseChance)
		{
			humanlike.relations.AddDirectRelation(PawnRelationDefOf.Bond, animal);
			if (humanlike.Faction == Faction.OfPlayer || animal.Faction == Faction.OfPlayer)
			{
				TaleRecorder.RecordTale(TaleDefOf.BondedWithAnimal, humanlike, animal);
			}
			bool flag = false;
			string text = null;
			if (animal.Name == null || animal.Name.Numerical)
			{
				flag = true;
				text = ((animal.Name == null) ? animal.LabelIndefinite() : animal.Name.ToStringFull);
				animal.Name = PawnBioAndNameGenerator.GeneratePawnName(animal);
			}
			if (PawnUtility.ShouldSendNotificationAbout(humanlike) || PawnUtility.ShouldSendNotificationAbout(animal))
			{
				string text2 = ((!flag) ? ((string)"MessageNewBondRelation".Translate(humanlike.LabelShort, animal.LabelShort, humanlike.Named("HUMAN"), animal.Named("ANIMAL")).CapitalizeFirst()) : ((string)"MessageNewBondRelationNewName".Translate(humanlike.LabelShort, text, animal.Name.ToStringFull, humanlike.Named("HUMAN"), animal.Named("ANIMAL")).AdjustedFor(animal).CapitalizeFirst()));
				Messages.Message(text2, humanlike, MessageTypeDefOf.PositiveEvent);
			}
			return true;
		}
		return false;
	}

	public static AcceptanceReport RomanceEligiblePair(Pawn initiator, Pawn target, bool forOpinionExplanation)
	{
		if (initiator == target)
		{
			return false;
		}
		if (ChildcareUtility.CanSuckle(target, out var _))
		{
			return false;
		}
		DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingLoveRealtionshipBetween(initiator, target, allowDead: false);
		if (directPawnRelation != null)
		{
			string genderSpecificLabel = directPawnRelation.def.GetGenderSpecificLabel(target);
			return "RomanceChanceExistingRelation".Translate(initiator.Named("PAWN"), genderSpecificLabel.Named("RELATION"));
		}
		if (!RomanceEligible(initiator, initiator: true, forOpinionExplanation))
		{
			return false;
		}
		if (target.ageTracker.AgeBiologicalYearsFloat < 16f)
		{
			if (!forOpinionExplanation)
			{
				return AcceptanceReport.WasRejected;
			}
			return "CantRomanceTargetYoung".Translate();
		}
		if (Incestuous(initiator, target))
		{
			if (!forOpinionExplanation)
			{
				return AcceptanceReport.WasRejected;
			}
			return "CantRomanceTargetIncest".Translate();
		}
		if (target.IsPrisoner)
		{
			if (!forOpinionExplanation)
			{
				return AcceptanceReport.WasRejected;
			}
			return "CantRomanceTargetPrisoner".Translate();
		}
		if (!AttractedToGender(initiator, target.gender) || !AttractedToGender(target, initiator.gender))
		{
			if (!forOpinionExplanation)
			{
				return AcceptanceReport.WasRejected;
			}
			return "CantRomanceTargetSexuality".Translate();
		}
		AcceptanceReport acceptanceReport = RomanceEligible(target, initiator: false, forOpinionExplanation);
		if (!acceptanceReport)
		{
			return acceptanceReport;
		}
		if (target.relations.OpinionOf(initiator) <= 5)
		{
			return "CantRomanceTargetOpinion".Translate();
		}
		if (!forOpinionExplanation && InteractionWorker_RomanceAttempt.SuccessChance(initiator, target, 1f) <= 0f)
		{
			return "CantRomanceTargetZeroChance".Translate();
		}
		if ((!forOpinionExplanation && !initiator.CanReach(target, PathEndMode.Touch, Danger.Deadly)) || target.IsForbidden(initiator))
		{
			return "CantRomanceTargetUnreachable".Translate();
		}
		if (initiator.relations.IsTryRomanceOnCooldown)
		{
			return "RomanceOnCooldown".Translate();
		}
		return true;
	}

	public static bool AttractedToGender(Pawn pawn, Gender gender)
	{
		Pawn_StoryTracker story = pawn.story;
		if (story != null && story.traits?.HasTrait(TraitDefOf.Asexual) == true)
		{
			return false;
		}
		Pawn_StoryTracker story2 = pawn.story;
		if (story2 != null && story2.traits?.HasTrait(TraitDefOf.Bisexual) == true)
		{
			return true;
		}
		Pawn_StoryTracker story3 = pawn.story;
		if (story3 != null && story3.traits?.HasTrait(TraitDefOf.Gay) == true)
		{
			return pawn.gender == gender;
		}
		return pawn.gender != gender;
	}

	public static AcceptanceReport RomanceEligible(Pawn pawn, bool initiator, bool forOpinionExplanation)
	{
		if (pawn.ageTracker.AgeBiologicalYearsFloat < 16f)
		{
			return false;
		}
		if (pawn.IsPrisoner)
		{
			if (!initiator || forOpinionExplanation)
			{
				return AcceptanceReport.WasRejected;
			}
			return "CantRomanceInitiateMessagePrisoner".Translate(pawn).CapitalizeFirst();
		}
		if (pawn.Downed && !forOpinionExplanation)
		{
			return initiator ? "CantRomanceInitiateMessageDowned".Translate(pawn).CapitalizeFirst() : "CantRomanceTargetDowned".Translate();
		}
		Pawn_StoryTracker story = pawn.story;
		if (story != null && story.traits?.HasTrait(TraitDefOf.Asexual) == true)
		{
			if (!initiator || forOpinionExplanation)
			{
				return AcceptanceReport.WasRejected;
			}
			return "CantRomanceInitiateMessageAsexual".Translate(pawn).CapitalizeFirst();
		}
		if (initiator && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
		{
			if (!forOpinionExplanation)
			{
				return "CantRomanceInitiateMessageTalk".Translate(pawn).CapitalizeFirst();
			}
			return AcceptanceReport.WasRejected;
		}
		if (pawn.Drafted && !forOpinionExplanation)
		{
			return initiator ? "CantRomanceInitiateMessageDrafted".Translate(pawn).CapitalizeFirst() : "CantRomanceTargetDrafted".Translate();
		}
		if (initiator && pawn.IsSlave)
		{
			if (!forOpinionExplanation)
			{
				return "CantRomanceInitiateMessageSlave".Translate(pawn).CapitalizeFirst();
			}
			return AcceptanceReport.WasRejected;
		}
		if (pawn.MentalState != null)
		{
			return (initiator && !forOpinionExplanation) ? "CantRomanceInitiateMessageMentalState".Translate(pawn).CapitalizeFirst() : "CantRomanceTargetMentalState".Translate();
		}
		return true;
	}

	private static void GiveRomanceJob(Pawn romancer, Pawn romanceTarget)
	{
		Job job = JobMaker.MakeJob(JobDefOf.TryRomance, romanceTarget);
		job.interaction = InteractionDefOf.RomanceAttempt;
		romancer.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}

	private static string GetRelationshipWarning(Pawn pawn)
	{
		int count = pawn.GetLoveRelations(includeDead: false).Count;
		bool flag = count >= 1;
		if (flag && ModsConfig.IdeologyActive)
		{
			flag = !new HistoryEvent(pawn.GetHistoryEventForLoveRelationCountPlusOne(), pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo();
		}
		if (!flag)
		{
			return "";
		}
		if (count <= 1)
		{
			return " - " + "RomanceWarningMonogamous".Translate(pawn) + "\n";
		}
		return " - " + "RomanceWarningPolygamous".Translate(pawn, count) + "\n";
	}

	private static void GiveRomanceJobWithWarning(Pawn romancer, Pawn romanceTarget)
	{
		string text = GetRelationshipWarning(romancer) + GetRelationshipWarning(romanceTarget);
		if (!text.NullOrEmpty())
		{
			Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("RomanceExistingRelationshipWarning".Translate(romancer.Named("INITIATOR"), romanceTarget.Named("TARGET")) + "\n\n" + text + "\n" + "StillWishContinue".Translate(), delegate
			{
				GiveRomanceJob(romancer, romanceTarget);
			}, destructive: true);
			Find.WindowStack.Add(window);
		}
		else
		{
			GiveRomanceJob(romancer, romanceTarget);
		}
	}

	public static bool RomanceOption(Pawn initiator, Pawn romanceTarget, out FloatMenuOption option, out float chance)
	{
		if (!AttractedToGender(initiator, romanceTarget.gender))
		{
			option = null;
			chance = 0f;
			return false;
		}
		AcceptanceReport acceptanceReport = RomanceEligiblePair(initiator, romanceTarget, forOpinionExplanation: false);
		if (!acceptanceReport.Accepted && acceptanceReport.Reason.NullOrEmpty())
		{
			option = null;
			chance = 0f;
			return false;
		}
		if (acceptanceReport.Accepted)
		{
			chance = InteractionWorker_RomanceAttempt.SuccessChance(initiator, romanceTarget, 1f);
			string label = string.Format("{0} ({1} {2})", romanceTarget.LabelShort, chance.ToStringPercent(), "chance".Translate());
			option = new FloatMenuOption(label, delegate
			{
				GiveRomanceJobWithWarning(initiator, romanceTarget);
			}, MenuOptionPriority.Low);
			return true;
		}
		chance = 0f;
		option = new FloatMenuOption(romanceTarget.LabelShort + " (" + acceptanceReport.Reason + ")", null);
		return false;
	}

	private static bool Incestuous(Pawn one, Pawn two)
	{
		foreach (PawnRelationDef relation in one.GetRelations(two))
		{
			if (relation.romanceChanceFactor != 1f)
			{
				return true;
			}
		}
		return false;
	}

	public static string LabelWithBondInfo(Pawn humanlike, Pawn animal)
	{
		string text = humanlike.LabelShort;
		if (humanlike.relations.DirectRelationExists(PawnRelationDefOf.Bond, animal))
		{
			text += " " + "BondBrackets".Translate();
		}
		return text;
	}

	private static bool HasAnySocialMemoryWith(Pawn p, Pawn otherPawn)
	{
		if (p.Dead)
		{
			return false;
		}
		if (!p.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike || p.needs == null || p.needs.mood == null)
		{
			return false;
		}
		List<Thought_Memory> memories = p.needs.mood.thoughts.memories.Memories;
		for (int i = 0; i < memories.Count; i++)
		{
			if (memories[i] is Thought_MemorySocial thought_MemorySocial && thought_MemorySocial.OtherPawn() == otherPawn)
			{
				return true;
			}
		}
		return false;
	}
}
