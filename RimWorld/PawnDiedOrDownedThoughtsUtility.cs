using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class PawnDiedOrDownedThoughtsUtility
{
	private static List<IndividualThoughtToAdd> tmpIndividualThoughtsToAdd = new List<IndividualThoughtToAdd>();

	private static List<ThoughtToAddToAll> tmpAllColonistsThoughts = new List<ThoughtToAddToAll>();

	public static void TryGiveThoughts(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind)
	{
		try
		{
			if (PawnGenerator.IsBeingGenerated(victim) || Current.ProgramState != ProgramState.Playing || victim.wasLeftBehindStartingPawn)
			{
				return;
			}
			GetThoughts(victim, dinfo, thoughtsKind, tmpIndividualThoughtsToAdd, tmpAllColonistsThoughts);
			for (int i = 0; i < tmpIndividualThoughtsToAdd.Count; i++)
			{
				tmpIndividualThoughtsToAdd[i].Add();
			}
			if (tmpAllColonistsThoughts.Any())
			{
				foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists)
				{
					if (allMapsCaravansAndTravellingTransporters_Alive_Colonist != victim)
					{
						for (int j = 0; j < tmpAllColonistsThoughts.Count; j++)
						{
							tmpAllColonistsThoughts[j].Add(allMapsCaravansAndTravellingTransporters_Alive_Colonist);
						}
					}
				}
			}
			tmpIndividualThoughtsToAdd.Clear();
			tmpAllColonistsThoughts.Clear();
			if ((!dinfo.HasValue || !dinfo.Value.Def.execution) && thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && victim.IsPrisonerOfColony)
			{
				Pawn arg = FindResponsibleColonist(victim, dinfo);
				if (!victim.guilt.IsGuilty && !victim.InAggroMentalState)
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.InnocentPrisonerDied, arg.Named(HistoryEventArgsNames.Doer)));
				}
				else
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.GuiltyPrisonerDied, arg.Named(HistoryEventArgsNames.Doer)));
				}
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.PrisonerDied, arg.Named(HistoryEventArgsNames.Doer)));
			}
		}
		catch (Exception ex)
		{
			Log.Error("Could not give thoughts: " + ex);
		}
	}

	public static void TryGiveThoughts(IEnumerable<Pawn> victims, PawnDiedOrDownedThoughtsKind thoughtsKind)
	{
		foreach (Pawn victim in victims)
		{
			TryGiveThoughts(victim, null, thoughtsKind);
		}
	}

	public static void GetThoughts(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtToAddToAll> outAllColonistsThoughts)
	{
		outIndividualThoughts.Clear();
		outAllColonistsThoughts.Clear();
		if (victim.RaceProps.Humanlike)
		{
			AppendThoughts_ForHumanlike(victim, dinfo, thoughtsKind, outIndividualThoughts, outAllColonistsThoughts);
		}
		if (victim.relations != null && victim.relations.everSeenByPlayer)
		{
			AppendThoughts_Relations(victim, dinfo, thoughtsKind, outIndividualThoughts, outAllColonistsThoughts);
		}
	}

	public static void BuildMoodThoughtsListString(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, StringBuilder sb, string individualThoughtsHeader, string allColonistsThoughtsHeader)
	{
		GetThoughts(victim, dinfo, thoughtsKind, tmpIndividualThoughtsToAdd, tmpAllColonistsThoughts);
		if (tmpAllColonistsThoughts.Any())
		{
			if (!allColonistsThoughtsHeader.NullOrEmpty())
			{
				sb.Append(allColonistsThoughtsHeader);
				sb.AppendLine();
			}
			for (int i = 0; i < tmpAllColonistsThoughts.Count; i++)
			{
				ThoughtToAddToAll thoughtToAddToAll = tmpAllColonistsThoughts[i];
				if (sb.Length > 0)
				{
					sb.AppendLine();
				}
				sb.Append("  - " + thoughtToAddToAll.thoughtDef.stages[0].LabelCap + " " + Mathf.RoundToInt(thoughtToAddToAll.thoughtDef.stages[0].baseMoodEffect).ToStringWithSign());
			}
		}
		if (!tmpIndividualThoughtsToAdd.Any((IndividualThoughtToAdd x) => x.thought.MoodOffset() != 0f))
		{
			return;
		}
		if (!individualThoughtsHeader.NullOrEmpty())
		{
			sb.Append(individualThoughtsHeader);
		}
		foreach (IGrouping<Pawn, IndividualThoughtToAdd> item in from x in tmpIndividualThoughtsToAdd
			where x.thought.MoodOffset() != 0f
			group x by x.addTo)
		{
			if (sb.Length > 0)
			{
				sb.AppendLine();
				sb.AppendLine();
			}
			string value = item.Key.KindLabel.CapitalizeFirst() + " " + item.Key.LabelShort;
			sb.Append(value);
			sb.Append(":");
			foreach (IndividualThoughtToAdd item2 in item)
			{
				sb.AppendLine();
				sb.Append("    " + item2.LabelCap);
			}
		}
	}

	public static void BuildMoodThoughtsListString(IEnumerable<Pawn> victims, PawnDiedOrDownedThoughtsKind thoughtsKind, StringBuilder sb, string individualThoughtsHeader, string allColonistsThoughtsHeader, string victimLabelKey)
	{
		foreach (Pawn victim in victims)
		{
			GetThoughts(victim, null, thoughtsKind, tmpIndividualThoughtsToAdd, tmpAllColonistsThoughts);
			if (tmpIndividualThoughtsToAdd.Any() || tmpAllColonistsThoughts.Any())
			{
				if (sb.Length > 0)
				{
					sb.AppendLine();
					sb.AppendLine();
				}
				string text = victim.KindLabel.CapitalizeFirst() + " " + victim.LabelShort;
				if (victimLabelKey.NullOrEmpty())
				{
					sb.Append(text + ":");
				}
				else
				{
					sb.Append(victimLabelKey.Translate(text));
				}
				BuildMoodThoughtsListString(victim, null, thoughtsKind, sb, individualThoughtsHeader, allColonistsThoughtsHeader);
			}
		}
	}

	private static void AppendThoughts_ForHumanlike(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtToAddToAll> outAllColonistsThoughts)
	{
		bool flag = dinfo.HasValue && dinfo.Value.Def.execution;
		if (dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(victim) && dinfo.Value.Instigator != null && dinfo.Value.Instigator is Pawn)
		{
			Pawn pawn = (Pawn)dinfo.Value.Instigator;
			if (!pawn.Dead && pawn.needs.mood != null && pawn.story != null && pawn != victim && PawnUtility.ShouldGetThoughtAbout(pawn, victim))
			{
				if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
				{
					outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledHumanlikeBloodlust, pawn));
				}
				if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && victim.HostileTo(pawn) && victim.Faction != null && PawnUtility.IsFactionLeader(victim) && victim.Faction.HostileTo(pawn.Faction))
				{
					outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.DefeatedHostileFactionLeader, pawn, victim));
				}
				if (ModsConfig.BiotechActive && thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && !victim.DevelopmentalStage.Adult() && pawn.DevelopmentalStage.Adult())
				{
					outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledChild, pawn, victim));
				}
			}
		}
		if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && !flag)
		{
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
			{
				if (item == victim || item.needs == null || item.needs.mood == null || !PawnUtility.ShouldGetThoughtAbout(item, victim) || (item.MentalStateDef == MentalStateDefOf.SocialFighting && ((MentalState_SocialFighting)item.MentalState).otherPawn == victim))
				{
					continue;
				}
				if (ThoughtUtility.Witnessed(item, victim))
				{
					bool flag2 = item.Faction == Faction.OfPlayer && victim.IsQuestLodger();
					if (item.Faction == victim.Faction && !flag2 && !victim.IsSlave)
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathAlly, item));
					}
					else if (victim.Faction == null || !victim.Faction.HostileTo(item.Faction) || flag2 || victim.IsSlave)
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathNonAlly, item));
					}
					if (item.relations.FamilyByBlood.Contains(victim))
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathFamily, item));
					}
					outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathBloodlust, item));
				}
				else if (victim.Faction == Faction.OfPlayer && victim.Faction == item.Faction && victim.HostFaction != item.Faction && !victim.IsQuestLodger() && !victim.IsSubhuman && !victim.IsSlave)
				{
					outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KnowColonistDied, item, victim));
				}
			}
		}
		if (victim.guilt != null && victim.guilt.IsGuilty)
		{
			return;
		}
		if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Banished && victim.IsColonist && !victim.IsSlave)
		{
			outAllColonistsThoughts.Add(new ThoughtToAddToAll(ThoughtDefOf.ColonistBanished, victim));
		}
		if (thoughtsKind == PawnDiedOrDownedThoughtsKind.DeniedJoining)
		{
			outAllColonistsThoughts.Add(new ThoughtToAddToAll(ThoughtDefOf.DeniedJoining, victim));
		}
		if (thoughtsKind == PawnDiedOrDownedThoughtsKind.BanishedToDie)
		{
			if (victim.IsColonist && !victim.IsSlave)
			{
				outAllColonistsThoughts.Add(new ThoughtToAddToAll(ThoughtDefOf.ColonistBanishedToDie, victim));
			}
			else if (victim.IsPrisonerOfColony || victim.IsSlaveOfColony)
			{
				outAllColonistsThoughts.Add(new ThoughtToAddToAll(ThoughtDefOf.PrisonerBanishedToDie, victim));
			}
		}
		if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Lost && victim.IsColonist && !victim.IsQuestLodger() && !victim.IsSlave)
		{
			outAllColonistsThoughts.Add(new ThoughtToAddToAll(ThoughtDefOf.ColonistLost, victim));
		}
	}

	private static void AppendThoughts_Relations(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtToAddToAll> outAllColonistsThoughts)
	{
		if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Banished && victim.RaceProps.Animal)
		{
			GiveThoughtsForAnimalBond(ThoughtDefOf.BondedAnimalBanished);
		}
		if (thoughtsKind == PawnDiedOrDownedThoughtsKind.ReleasedToWild && victim.RaceProps.Animal)
		{
			GiveThoughtsForAnimalBond(ThoughtDefOf.BondedAnimalReleased);
		}
		if (thoughtsKind != PawnDiedOrDownedThoughtsKind.Died && thoughtsKind != PawnDiedOrDownedThoughtsKind.BanishedToDie && thoughtsKind != PawnDiedOrDownedThoughtsKind.Lost)
		{
			return;
		}
		List<Pawn> list = victim.relations.PotentiallyRelatedPawns.Where((Pawn x) => x.needs?.mood != null).ToList();
		foreach (Pawn item in list)
		{
			if (!PawnUtility.ShouldGetThoughtAbout(item, victim))
			{
				continue;
			}
			PawnRelationDef mostImportantRelation = item.GetMostImportantRelation(victim);
			if (mostImportantRelation != null)
			{
				ThoughtDef genderSpecificThought = mostImportantRelation.GetGenderSpecificThought(victim, thoughtsKind);
				if (genderSpecificThought != null)
				{
					outIndividualThoughts.Add(new IndividualThoughtToAdd(genderSpecificThought, item, victim));
				}
			}
		}
		if (dinfo.HasValue && thoughtsKind != PawnDiedOrDownedThoughtsKind.Lost && dinfo.Value.Instigator is Pawn pawn && pawn != victim)
		{
			foreach (Pawn item2 in list)
			{
				if (pawn == item2 || !PawnUtility.ShouldGetThoughtAbout(item2, victim))
				{
					continue;
				}
				ThoughtDef thoughtDef = item2.GetMostImportantRelation(victim)?.GetGenderSpecificKilledThought(victim);
				if (thoughtDef != null)
				{
					outIndividualThoughts.Add(new IndividualThoughtToAdd(thoughtDef, item2, pawn));
				}
				if (item2.RaceProps.IsFlesh)
				{
					int num = item2.relations.OpinionOf(victim);
					if (num >= 20)
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledMyFriend, item2, pawn, 1f, victim.relations.GetFriendDiedThoughtPowerFactor(num)));
					}
					else if (num <= -20)
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledMyRival, item2, pawn, 1f, victim.relations.GetRivalDiedThoughtPowerFactor(num)));
					}
				}
			}
		}
		if (!victim.RaceProps.Humanlike)
		{
			return;
		}
		ThoughtDef thoughtDef2;
		ThoughtDef thoughtDef3;
		if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Lost)
		{
			thoughtDef2 = ThoughtDefOf.PawnWithGoodOpinionLost;
			thoughtDef3 = ThoughtDefOf.PawnWithBadOpinionLost;
		}
		else
		{
			thoughtDef2 = ThoughtDefOf.PawnWithGoodOpinionDied;
			thoughtDef3 = ThoughtDefOf.PawnWithBadOpinionDied;
		}
		List<Pawn> allMapsCaravansAndTravellingTransporters_Alive = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive;
		for (int num2 = 0; num2 < allMapsCaravansAndTravellingTransporters_Alive.Count; num2++)
		{
			Pawn pawn2 = allMapsCaravansAndTravellingTransporters_Alive[num2];
			if (pawn2.needs != null && pawn2.RaceProps.IsFlesh && pawn2.needs.mood != null && PawnUtility.ShouldGetThoughtAbout(pawn2, victim))
			{
				int num3 = pawn2.relations.OpinionOf(victim);
				if (num3 >= 20)
				{
					outIndividualThoughts.Add(new IndividualThoughtToAdd(thoughtDef2, pawn2, victim, victim.relations.GetFriendDiedThoughtPowerFactor(num3)));
				}
				else if (num3 <= -20)
				{
					outIndividualThoughts.Add(new IndividualThoughtToAdd(thoughtDef3, pawn2, victim, victim.relations.GetRivalDiedThoughtPowerFactor(num3)));
				}
			}
		}
		void GiveThoughtsForAnimalBond(ThoughtDef thoughtDef4)
		{
			List<DirectPawnRelation> directRelations = victim.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				if (directRelations[i].otherPawn.needs != null && directRelations[i].otherPawn.needs.mood != null && PawnUtility.ShouldGetThoughtAbout(directRelations[i].otherPawn, victim) && directRelations[i].def == PawnRelationDefOf.Bond)
				{
					outIndividualThoughts.Add(new IndividualThoughtToAdd(thoughtDef4, directRelations[i].otherPawn, victim));
				}
			}
		}
	}

	private static Pawn FindResponsibleColonist(Pawn victim, DamageInfo? dinfo)
	{
		if (dinfo.HasValue && dinfo.Value.Instigator is Pawn { IsColonist: not false } pawn)
		{
			return pawn;
		}
		if (victim.Spawned)
		{
			if (victim.Map.mapPawns.FreeColonistsSpawned.Any((Pawn x) => !x.Downed))
			{
				return victim.Map.mapPawns.FreeColonistsSpawned.Where((Pawn x) => !x.Downed).MinBy((Pawn x) => x.Position.DistanceToSquared(victim.Position));
			}
			if (victim.Map.mapPawns.FreeColonistsSpawned.Any())
			{
				return victim.Map.mapPawns.FreeColonistsSpawned.MinBy((Pawn x) => x.Position.DistanceToSquared(victim.Position));
			}
		}
		List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
		if (allMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Any())
		{
			return allMapsCaravansAndTravellingTransporters_Alive_FreeColonists.First();
		}
		return null;
	}

	public static void RemoveDiedThoughts(Pawn pawn)
	{
		foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive)
		{
			if (item.needs == null || item.needs.mood == null || item == pawn)
			{
				continue;
			}
			MemoryThoughtHandler memories = item.needs.mood.thoughts.memories;
			memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.KnowColonistDied, pawn);
			memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.KnowPrisonerDiedInnocent, pawn);
			memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.PawnWithGoodOpinionDied, pawn);
			memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.PawnWithBadOpinionDied, pawn);
			if (ModsConfig.BiotechActive)
			{
				memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.Stillbirth, pawn);
			}
			List<PawnRelationDef> allDefsListForReading = DefDatabase<PawnRelationDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				ThoughtDef genderSpecificDiedThought = allDefsListForReading[i].GetGenderSpecificDiedThought(pawn);
				if (genderSpecificDiedThought != null)
				{
					memories.RemoveMemoriesOfDefWhereOtherPawnIs(genderSpecificDiedThought, pawn);
				}
			}
		}
	}

	public static void RemoveLostThoughts(Pawn pawn)
	{
		foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive)
		{
			if (item.needs == null || item.needs.mood == null || item == pawn)
			{
				continue;
			}
			MemoryThoughtHandler memories = item.needs.mood.thoughts.memories;
			memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.ColonistLost, pawn);
			memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.PawnWithGoodOpinionLost, pawn);
			memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.PawnWithBadOpinionLost, pawn);
			List<PawnRelationDef> allDefsListForReading = DefDatabase<PawnRelationDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				ThoughtDef genderSpecificLostThought = allDefsListForReading[i].GetGenderSpecificLostThought(pawn);
				if (genderSpecificLostThought != null)
				{
					memories.RemoveMemoriesOfDefWhereOtherPawnIs(genderSpecificLostThought, pawn);
				}
			}
		}
	}

	public static void RemoveResuedRelativeThought(Pawn pawn)
	{
		foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive)
		{
			if (item.needs != null && item.needs.mood != null && item != pawn)
			{
				item.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.RescuedRelative, pawn);
			}
		}
	}

	public static void GiveVeneratedAnimalDiedThoughts(Pawn victim, Map map)
	{
		if (!ModsConfig.IdeologyActive || victim.Faction == null || map == null)
		{
			return;
		}
		foreach (Pawn item in map.mapPawns.PawnsInFaction(victim.Faction))
		{
			if (item == victim || item.Ideo == null || item.needs?.mood?.thoughts == null || !item.Ideo.IsVeneratedAnimal(victim))
			{
				continue;
			}
			Thought_TameVeneratedAnimalDied thought_TameVeneratedAnimalDied = (Thought_TameVeneratedAnimalDied)ThoughtMaker.MakeThought(ThoughtDefOf.TameVeneratedAnimalDied);
			thought_TameVeneratedAnimalDied.animalKindLabel = victim.KindLabel;
			foreach (Precept item2 in item.Ideo.PreceptsListForReading)
			{
				if (item2 is Precept_Animal precept_Animal && precept_Animal.ThingDef == victim.def)
				{
					thought_TameVeneratedAnimalDied.sourcePrecept = item2;
					break;
				}
			}
			item.needs.mood.thoughts.memories.TryGainMemory(thought_TameVeneratedAnimalDied);
		}
	}
}
