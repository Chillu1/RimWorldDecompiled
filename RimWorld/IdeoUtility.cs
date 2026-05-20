using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class IdeoUtility
{
	public const int MinFluidIdeoNormalMemes = 1;

	private static HashSet<Room> tmpCheckRooms = new HashSet<Room>();

	private static List<TreeSighting> tmpTreeSightings = new List<TreeSighting>();

	public static void Notify_HistoryEvent(HistoryEvent ev, bool canApplySelfTookThoughts = true)
	{
		if (ev.args.TryGetArg(HistoryEventArgsNames.Doer, out Pawn pawn))
		{
			if (pawn.Ideo != null)
			{
				pawn.Ideo.Notify_MemberTookAction(ev, canApplySelfTookThoughts);
			}
			if (pawn.IsCaravanMember())
			{
				Caravan caravan = pawn.GetCaravan();
				for (int i = 0; i < caravan.pawns.Count; i++)
				{
					CheckKnows(caravan.pawns[i]);
				}
			}
			else if (pawn.Spawned)
			{
				IReadOnlyList<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
				for (int j = 0; j < allPawnsSpawned.Count; j++)
				{
					CheckKnows(allPawnsSpawned[j]);
				}
			}
		}
		else
		{
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
			for (int k = 0; k < allMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count; k++)
			{
				Pawn pawn2 = allMapsCaravansAndTravellingTransporters_Alive_FreeColonists[k];
				if (pawn2.Ideo != null)
				{
					pawn2.Ideo.Notify_MemberKnows(ev, pawn2);
				}
			}
		}
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			List<Precept> preceptsListForReading = allIdeo.PreceptsListForReading;
			for (int l = 0; l < preceptsListForReading.Count; l++)
			{
				preceptsListForReading[l].Notify_HistoryEvent(ev);
			}
		}
		void CheckKnows(Pawn p)
		{
			if (p != pawn && p.Ideo != null)
			{
				p.Ideo.Notify_MemberKnows(ev, p);
			}
		}
	}

	public static bool DoerWillingToDo(this HistoryEvent ev)
	{
		Pawn arg = ev.args.GetArg<Pawn>(HistoryEventArgsNames.Doer);
		if (arg?.Ideo != null)
		{
			return arg.Ideo.MemberWillingToDo(ev);
		}
		return true;
	}

	public static bool DoerWillingToDo(HistoryEventDef def, Pawn pawn)
	{
		return new HistoryEvent(def, pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo();
	}

	public static bool Notify_PawnAboutToDo(this HistoryEvent ev, string messageKey = "MessagePawnUnwillingToDoDueToIdeo")
	{
		if (!ev.DoerWillingToDo())
		{
			Pawn arg = ev.args.GetArg<Pawn>(HistoryEventArgsNames.Doer);
			Messages.Message(messageKey.Translate(arg), arg, MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		return true;
	}

	public static bool Notify_PawnAboutToDo(this HistoryEvent ev, out FloatMenuOption opt, string baseText)
	{
		if (!ev.DoerWillingToDo())
		{
			opt = new FloatMenuOption(baseText + ": " + "IdeoligionForbids".Translate(), null);
			return false;
		}
		opt = null;
		return true;
	}

	public static bool Notify_PawnAboutToDo_Job(this HistoryEvent ev)
	{
		if (!ev.DoerWillingToDo())
		{
			JobFailReason.Is("IdeoligionForbids".Translate());
			return false;
		}
		return true;
	}

	public static List<MemeDef> GenerateRandomMemes(IdeoGenerationParms parms)
	{
		return GenerateRandomMemes(parms.forNewFluidIdeo ? IdeoFoundation.MemeCountRangeFluidAbsolute.RandomInRange : IdeoFoundation.MemeCountRangeNPCInitial.RandomInRange, parms);
	}

	private static bool CanAdd(MemeDef meme, List<MemeDef> memes, FactionDef forFaction = null, bool forNewFluidIdeo = false)
	{
		if (memes.Contains(meme))
		{
			return false;
		}
		if (forFaction != null && !IsMemeAllowedFor(meme, forFaction))
		{
			return false;
		}
		if (forNewFluidIdeo && !IsMemeAllowedForInitialFluidIdeo(meme))
		{
			return false;
		}
		for (int i = 0; i < memes.Count; i++)
		{
			for (int j = 0; j < meme.exclusionTags.Count; j++)
			{
				if (memes[i].exclusionTags.Contains(meme.exclusionTags[j]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static List<MemeDef> GenerateRandomMemes(int count, IdeoGenerationParms parms)
	{
		FactionDef forFaction = parms.forFaction;
		bool forPlayerFaction = forFaction != null && forFaction.isPlayer;
		List<MemeDef> memes = new List<MemeDef>();
		bool flag = false;
		if (forFaction != null && forFaction.requiredMemes != null)
		{
			for (int i = 0; i < forFaction.requiredMemes.Count; i++)
			{
				memes.Add(forFaction.requiredMemes[i]);
				if (forFaction.requiredMemes[i].category == MemeCategory.Normal)
				{
					count--;
				}
				else if (forFaction.requiredMemes[i].category == MemeCategory.Structure)
				{
					flag = true;
				}
			}
		}
		if (parms.forcedMemes != null)
		{
			foreach (MemeDef forcedMeme in parms.forcedMemes)
			{
				if (forcedMeme.category == MemeCategory.Structure)
				{
					flag = true;
					break;
				}
			}
		}
		if (forFaction != null && forFaction.structureMemeWeights != null && !flag)
		{
			MemeWeight result2;
			if (forFaction.structureMemeWeights.Where((MemeWeight x) => CanAdd(x.meme, memes, forFaction, parms.forNewFluidIdeo) && (forPlayerFaction || !AnyIdeoHas(x.meme))).TryRandomElementByWeight((MemeWeight x) => x.selectionWeight * x.meme.randomizationSelectionWeightFactor, out var result))
			{
				memes.Add(result.meme);
				flag = true;
			}
			else if (forFaction.structureMemeWeights.Where((MemeWeight x) => CanAdd(x.meme, memes, forFaction, parms.forNewFluidIdeo)).TryRandomElementByWeight((MemeWeight x) => x.selectionWeight * x.meme.randomizationSelectionWeightFactor, out result2))
			{
				memes.Add(result2.meme);
				flag = true;
			}
		}
		if (!flag)
		{
			MemeDef result4;
			if (DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => x.category == MemeCategory.Structure && CanAdd(x, memes, forFaction, parms.forNewFluidIdeo) && (forPlayerFaction || !AnyIdeoHas(x))).TryRandomElement(out var result3))
			{
				memes.Add(result3);
			}
			else if (DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => x.category == MemeCategory.Structure && CanAdd(x, memes, forFaction, parms.forNewFluidIdeo)).TryRandomElementByWeight((MemeDef x) => x.randomizationSelectionWeightFactor, out result4))
			{
				memes.Add(result4);
			}
		}
		if (parms.forcedMemes != null)
		{
			memes.AddRange(parms.forcedMemes);
			return memes;
		}
		for (int num = 0; num < count; num++)
		{
			MemeDef result6;
			if (DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => x.category == MemeCategory.Normal && CanAdd(x, memes, forFaction, parms.forNewFluidIdeo) && (forPlayerFaction || !AnyIdeoHas(x)) && (parms.disallowedMemes == null || !parms.disallowedMemes.Contains(x))).TryRandomElementByWeight((MemeDef x) => x.randomizationSelectionWeightFactor, out var result5))
			{
				memes.Add(result5);
			}
			else if (DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => x.category == MemeCategory.Normal && CanAdd(x, memes, forFaction, parms.forNewFluidIdeo) && (parms.disallowedMemes == null || !parms.disallowedMemes.Contains(x))).TryRandomElementByWeight((MemeDef x) => x.randomizationSelectionWeightFactor, out result6))
			{
				memes.Add(result6);
			}
		}
		return memes;
	}

	public static List<MemeDef> RandomizeNormalMemes(int count, List<MemeDef> previousMemes, FactionDef forFaction = null, bool forNewFluidIdeo = false)
	{
		List<MemeDef> memes = new List<MemeDef>();
		foreach (MemeDef previousMeme in previousMemes)
		{
			if (previousMeme.category != MemeCategory.Normal)
			{
				memes.Add(previousMeme);
			}
		}
		if (forFaction != null && forFaction.requiredMemes != null)
		{
			for (int i = 0; i < forFaction.requiredMemes.Count; i++)
			{
				MemeDef memeDef = forFaction.requiredMemes[i];
				if (memeDef.category == MemeCategory.Normal)
				{
					memes.Add(memeDef);
					count--;
				}
			}
		}
		for (int j = 0; j < count; j++)
		{
			if (DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => x.category == MemeCategory.Normal && CanAdd(x, memes, forFaction, forNewFluidIdeo)).TryRandomElement(out var result))
			{
				memes.Add(result);
			}
		}
		return memes;
	}

	public static List<MemeDef> RandomizeNormalMemesForReforming(int maxMemes, List<MemeDef> existingMemes, FactionDef forFaction = null)
	{
		List<MemeDef> memes = new List<MemeDef>();
		memes.AddRange(existingMemes);
		int num = memes.Count((MemeDef m) => m.category == MemeCategory.Normal);
		MemeDef result;
		if (num > 1 && (num >= maxMemes || Rand.Bool))
		{
			memes.Remove(memes.Where((MemeDef m) => m.category == MemeCategory.Normal).RandomElement());
		}
		else if (num < maxMemes && DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => x.category == MemeCategory.Normal && CanAdd(x, memes, forFaction)).TryRandomElement(out result))
		{
			memes.Add(result);
		}
		return memes;
	}

	public static List<MemeDef> RandomizeStructureMeme(List<MemeDef> previousMemes, FactionDef forFaction = null)
	{
		List<MemeDef> memes = new List<MemeDef>();
		foreach (MemeDef previousMeme in previousMemes)
		{
			if (previousMeme.category != MemeCategory.Structure)
			{
				memes.Add(previousMeme);
			}
		}
		bool flag = false;
		if (forFaction != null && forFaction.requiredMemes != null)
		{
			for (int i = 0; i < forFaction.requiredMemes.Count; i++)
			{
				MemeDef memeDef = forFaction.requiredMemes[i];
				if (memeDef.category == MemeCategory.Structure)
				{
					memes.Add(memeDef);
					flag = true;
				}
			}
		}
		if (!flag && DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => x.category == MemeCategory.Structure && CanAdd(x, memes, forFaction)).TryRandomElement(out var result))
		{
			memes.Add(result);
		}
		return memes;
	}

	private static bool AnyIdeoHas(MemeDef meme)
	{
		if (Find.World == null)
		{
			return false;
		}
		List<Ideo> ideosListForReading = Find.IdeoManager.IdeosListForReading;
		for (int i = 0; i < ideosListForReading.Count; i++)
		{
			if (ideosListForReading[i].memes.Contains(meme))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanUseIdeo(FactionDef factionDef, Ideo ideo, IdeoGenerationParms parms)
	{
		if (factionDef.allowedCultures != null && !factionDef.allowedCultures.Contains(ideo.culture))
		{
			return false;
		}
		if (factionDef.requiredMemes != null)
		{
			for (int i = 0; i < factionDef.requiredMemes.Count; i++)
			{
				if (!ideo.memes.Contains(factionDef.requiredMemes[i]))
				{
					return false;
				}
			}
		}
		for (int j = 0; j < ideo.memes.Count; j++)
		{
			if (!IsMemeAllowedFor(ideo.memes[j], factionDef))
			{
				return false;
			}
		}
		if (parms.disallowedPrecepts != null && ideo.PreceptsListForReading.Any((Precept p) => parms.disallowedPrecepts.Contains(p.def)))
		{
			return false;
		}
		if (parms.forcedMemes != null && !ideo.memes.Any(parms.forcedMemes.Contains))
		{
			return false;
		}
		if (parms.disallowedMemes != null && ideo.memes.Any(parms.disallowedMemes.Contains))
		{
			return false;
		}
		if (parms.styles != null && !parms.styles.All((StyleCategoryDef style) => ideo.thingStyleCategories.Any((ThingStyleCategoryWithPriority inner) => inner.category == style)))
		{
			return false;
		}
		if (ideo.PreceptsListForReading.OfType<Precept_Ritual>().Any((Precept_Ritual p) => !RitualPatternDef.CanUseWithTechLevel(factionDef.techLevel, p.minTechLevel, p.maxTechLevel)))
		{
			return false;
		}
		if (ideo.PreceptsListForReading.OfType<Precept_Role>().Any((Precept_Role p) => !p.apparelRequirements.NullOrEmpty() && p.apparelRequirements.Any((PreceptApparelRequirement req) => !req.Compatible(ideo, factionDef))))
		{
			return false;
		}
		return true;
	}

	public static bool IsMemeAllowedFor(MemeDef meme, FactionDef faction)
	{
		if (faction.structureMemeWeights != null && meme.category == MemeCategory.Structure && faction.structureMemeWeights.Any((MemeWeight x) => x.meme == meme && x.selectionWeight != 0f))
		{
			return true;
		}
		if (faction.forcedMemes != null)
		{
			return faction.forcedMemes.Contains(meme);
		}
		if (faction.requiredMemes != null && faction.requiredMemes.Contains(meme))
		{
			return true;
		}
		if (meme.category == MemeCategory.Normal && !meme.allowDuringTutorial && faction.classicIdeo)
		{
			return false;
		}
		if (faction.disallowedMemes != null && faction.disallowedMemes.Contains(meme))
		{
			return false;
		}
		if (faction.allowedMemes != null && !faction.allowedMemes.Contains(meme))
		{
			return false;
		}
		if (meme.factionWhitelist != null && !meme.factionWhitelist.Contains(faction))
		{
			return false;
		}
		return true;
	}

	public static bool PlayerHasPreceptForBuilding(ThingDef buildingDef)
	{
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			if (allIdeo.HasPreceptForBuilding(buildingDef))
			{
				return true;
			}
		}
		return false;
	}

	public static IEnumerable<Pawn> AllColonistsWithCharityPrecept()
	{
		if (!ModsConfig.IdeologyActive)
		{
			yield break;
		}
		List<Pawn> colonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
		foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
		{
			List<Precept> precepts = ideo.PreceptsListForReading;
			for (int j = 0; j < precepts.Count; j++)
			{
				if (precepts[j].def.issue != IssueDefOf.Charity)
				{
					continue;
				}
				for (int i = 0; i < colonists.Count; i++)
				{
					if (colonists[i].Ideo == ideo)
					{
						yield return colonists[i];
					}
				}
			}
		}
	}

	public static bool AnyColonistWithRanchingIssue()
	{
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			List<Precept> preceptsListForReading = allIdeo.PreceptsListForReading;
			for (int i = 0; i < preceptsListForReading.Count; i++)
			{
				if (preceptsListForReading[i].def.issue == IssueDefOf.Ranching)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void Notify_PlayerRaidedSomeone(IEnumerable<Pawn> allRaiders)
	{
		if (allRaiders.EnumerableNullOrEmpty())
		{
			return;
		}
		allRaiders = allRaiders.Where((Pawn p) => !p.Dead);
		Find.History.Notify_PlayerRaidedSomeone();
		if (!ModsConfig.IdeologyActive)
		{
			return;
		}
		foreach (Pawn allRaider in allRaiders)
		{
			HistoryEvent historyEvent = new HistoryEvent(HistoryEventDefOf.Raided, allRaider.Named(HistoryEventArgsNames.Doer));
			Find.HistoryEventsManager.RecordEvent(historyEvent);
		}
		Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.PlayerRaidedSomeone));
	}

	public static void Notify_QuestCleanedUp(Quest quest, QuestState state)
	{
		switch (state)
		{
		case QuestState.EndedOfferExpired:
		case QuestState.EndedFailed:
			if (quest.root.failedOrExpiredHistoryEvent != null)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(quest.root.failedOrExpiredHistoryEvent));
			}
			if (quest.charity && ModsConfig.IdeologyActive && AllColonistsWithCharityPrecept().Any())
			{
				TaggedString taggedString2 = "MessageCharityEventRefused".Translate();
				if (!quest.hidden)
				{
					taggedString2 += ": " + "MessageCharityQuestEndedFailed".Translate(quest.name);
				}
				Messages.Message(taggedString2, null, MessageTypeDefOf.NegativeEvent, (!quest.hidden) ? quest : null);
			}
			break;
		case QuestState.EndedSuccess:
			if (quest.root.successHistoryEvent != null)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(quest.root.successHistoryEvent, quest.Named(HistoryEventArgsNames.Quest)));
			}
			if (quest.charity && ModsConfig.IdeologyActive && AllColonistsWithCharityPrecept().Any())
			{
				TaggedString taggedString = "MessageCharityEventFulfilled".Translate();
				if (!quest.hidden)
				{
					taggedString += ": " + "MessageCharityQuestEndedSuccess".Translate(quest.name);
				}
				Messages.Message(taggedString, null, MessageTypeDefOf.PositiveEvent, (!quest.hidden) ? quest : null);
			}
			break;
		}
	}

	public static float GetStyleDominanceFromCellsCenteredOn(IntVec3 center, IntVec3 rootCell, Map map, Ideo ideo)
	{
		bool flag = false;
		float num = 0f;
		GetJoinedRooms(rootCell, map, tmpCheckRooms);
		if (!tmpCheckRooms.Any())
		{
			return num;
		}
		int num2 = GenRadial.NumCellsInRadius(24.9f);
		for (int i = 0; i < num2; i++)
		{
			IntVec3 c = center + GenRadial.RadialPattern[i];
			if (!CellIsValid(c))
			{
				continue;
			}
			if (flag)
			{
				map.debugDrawer.FlashCell(c, 0.1f, "d");
			}
			TerrainDef terrain = c.GetTerrain(map);
			if (ideo.cachedPossibleBuildables.Contains(terrain))
			{
				num += terrain.GetStatValueAbstract(StatDefOf.StyleDominance);
			}
			else if (!terrain.canGenerateDefaultDesignator)
			{
				num -= terrain.GetStatValueAbstract(StatDefOf.StyleDominance);
			}
			foreach (Thing thing in c.GetThingList(map))
			{
				num += GetStyleDominance(thing, ideo);
			}
		}
		tmpCheckRooms.Clear();
		return num;
		bool CellIsValid(IntVec3 intVec)
		{
			if (!intVec.InBounds(map) || intVec.Fogged(map))
			{
				return false;
			}
			Room room = intVec.GetRoom(map);
			if (room == null)
			{
				return false;
			}
			return tmpCheckRooms.Contains(room);
		}
	}

	private static void GetJoinedRooms(IntVec3 rootCell, Map map, HashSet<Room> rooms)
	{
		Room room = rootCell.GetRoom(map);
		if (room == null)
		{
			return;
		}
		rooms.Clear();
		rooms.Add(room);
		if (!room.IsDoorway)
		{
			return;
		}
		foreach (Region neighbor in room.FirstRegion.Neighbors)
		{
			if (!rooms.Contains(neighbor.Room))
			{
				rooms.Add(neighbor.Room);
			}
		}
	}

	public static bool ThingSatisfiesIdeo(Thing thing, Ideo ideo)
	{
		return GetStyleDominance(thing, ideo) > 0f;
	}

	private static float GetStyleDominance(Thing t, Ideo ideo)
	{
		float statValue = t.GetStatValue(StatDefOf.StyleDominance);
		CompRelicContainer compRelicContainer = t.TryGetComp<CompRelicContainer>();
		if (compRelicContainer != null && compRelicContainer.Full)
		{
			CompStyleable compStyleable = (compRelicContainer.ContainedThing as ThingWithComps)?.compStyleable;
			if (compStyleable?.SourcePrecept?.ideo != null)
			{
				if (compStyleable.SourcePrecept.ideo == ideo)
				{
					return statValue;
				}
				return 0f - statValue;
			}
		}
		ThingStyleDef styleDef = t.GetStyleDef();
		if (styleDef != null)
		{
			if (ideo.GetStyleFor(t.def) == styleDef)
			{
				return statValue;
			}
			return 0f - statValue;
		}
		if (!t.def.canGenerateDefaultDesignator)
		{
			if (ideo.cachedPossibleBuildables.Contains(t.def) || (t.StyleSourcePrecept != null && ideo.PreceptsListForReading.Contains(t.StyleSourcePrecept)))
			{
				return statValue;
			}
			return 0f - statValue;
		}
		return 0f;
	}

	public static List<TreeSighting> TreeSightingsNearPawn(IntVec3 rootCell, Map map, Ideo ideo)
	{
		tmpTreeSightings.Clear();
		int num = GenRadial.NumCellsInRadius(11.9f);
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = rootCell + GenRadial.RadialPattern[i];
			if (!intVec.InBounds(map) || intVec.Fogged(map) || !GenSight.LineOfSight(rootCell, intVec, map))
			{
				continue;
			}
			foreach (Thing thing in intVec.GetThingList(map))
			{
				if (thing is Plant plant && plant.def.plant.IsTree)
				{
					tmpTreeSightings.Add(new TreeSighting(thing, Find.TickManager.TicksGame));
				}
			}
		}
		return tmpTreeSightings;
	}

	public static bool IdeoCausesHumanMeatCravings(this Ideo ideo)
	{
		if (!ideo.cachedPossibleSituationalThoughts.Contains(ThoughtDefOf.NoRecentHumanMeat_Preferred) && !ideo.cachedPossibleSituationalThoughts.Contains(ThoughtDefOf.NoRecentHumanMeat_RequiredStrong))
		{
			return ideo.cachedPossibleSituationalThoughts.Contains(ThoughtDefOf.NoRecentHumanMeat_RequiredRavenous);
		}
		return true;
	}

	public static bool IdeoPrefersNudity(this Ideo ideo)
	{
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			if (item.def.prefersNudity)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IdeoPrefersNudityForGender(this Ideo ideo, Gender gender)
	{
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			if (item.def.prefersNudity && (item.def.genderPrefersNudity == Gender.None || item.def.genderPrefersNudity == gender))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IdeoApprovesOfSlavery(this Ideo ideo)
	{
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			if (item.def.approvesOfSlavery)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IdeoPrefersDarkness(this Ideo ideo)
	{
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			if (item.def.prefersDarkness)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IdeoApprovesOfBlindness(this Ideo ideo)
	{
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			if (item.def.approvesOfBlindness)
			{
				return true;
			}
		}
		return false;
	}

	public static void Notify_NewColonyStarted()
	{
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			allIdeo.relicsCollected = false;
		}
		foreach (Ideo allIdeo2 in Faction.OfPlayer.ideos.AllIdeos)
		{
			foreach (Precept item in allIdeo2.PreceptsListForReading)
			{
				if (item is Precept_Relic precept_Relic)
				{
					precept_Relic.Notify_NewColonyStarted();
				}
			}
		}
	}

	public static Pawn FindFirstPawnWithLeaderRole(Caravan caravan)
	{
		List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
		for (int i = 0; i < pawnsListForReading.Count; i++)
		{
			Pawn pawn = pawnsListForReading[i];
			if (pawn.Ideo != null)
			{
				Precept_Role role = pawn.Ideo.GetRole(pawn);
				if (role != null && role.def.leaderRole)
				{
					return pawn;
				}
			}
		}
		return null;
	}

	public static Ideo MakeEmptyIdeo()
	{
		Ideo ideo = IdeoGenerator.MakeIdeo(DefDatabase<IdeoFoundationDef>.AllDefs.RandomElement());
		ideo.foundation.RandomizeIcon();
		return ideo;
	}

	public static Color? GetIdeoColorForBuilding(ThingDef def, Faction faction)
	{
		if (def.building.useIdeoColor && faction != null && faction.ideos != null)
		{
			Ideo ideo = null;
			foreach (Ideo allIdeo in faction.ideos.AllIdeos)
			{
				if (def.dominantStyleCategory != null && allIdeo.thingStyleCategories.Any((ThingStyleCategoryWithPriority sc) => sc.category == def.dominantStyleCategory))
				{
					ideo = allIdeo;
					break;
				}
				foreach (MemeDef meme in allIdeo.memes)
				{
					if (meme.AllDesignatorBuildables.Contains(def))
					{
						ideo = allIdeo;
						break;
					}
				}
				if (ideo != null)
				{
					break;
				}
			}
			ideo = ideo ?? faction.ideos.PrimaryIdeo;
			if (ideo != null)
			{
				return ideo.Color;
			}
		}
		return null;
	}

	public static bool IsMemeAllowedForInitialFluidIdeo(MemeDef memeDef)
	{
		return memeDef.impact <= 2;
	}

	public static bool CountsAsNonNPCForPrecepts(this Pawn pawn)
	{
		if (!pawn.IsColonist && !pawn.IsPrisonerOfColony)
		{
			return pawn.IsSlaveOfColony;
		}
		return true;
	}

	public static float IdeoChangeToWeight(Pawn pawn, Ideo ideo)
	{
		if (ModsConfig.AnomalyActive && ideo == Find.IdeoManager.Horaxian)
		{
			return 0f;
		}
		if (pawn == null)
		{
			return 1f;
		}
		if (ideo == pawn.Ideo)
		{
			return 0f;
		}
		float num = 1f;
		if (pawn.Faction != null)
		{
			foreach (Pawn item in PawnsFinder.AllMaps_SpawnedPawnsInFaction(pawn.Faction))
			{
				if (item.ideo != null && item.Ideo == ideo)
				{
					num += 1f;
					break;
				}
			}
			foreach (Faction item2 in Find.FactionManager.AllFactionsVisible)
			{
				if (item2 != pawn.Faction && item2.RelationKindWith(pawn.Faction) == FactionRelationKind.Ally && item2.ideos.IsPrimary(ideo))
				{
					num += 1f;
					break;
				}
			}
		}
		if (pawn.Spawned)
		{
			foreach (Pawn item3 in pawn.Map.mapPawns.AllPawnsSpawned)
			{
				if (item3.Faction != null && item3.ideo != null && item3.Ideo == ideo && (item3.Faction == pawn.Faction || !item3.Faction.HostileTo(pawn.Faction)))
				{
					num += 1f;
					break;
				}
			}
		}
		else
		{
			Caravan caravan = pawn.GetCaravan();
			if (caravan != null)
			{
				foreach (Pawn item4 in caravan.PawnsListForReading)
				{
					if (item4.ideo != null && item4.Ideo == ideo)
					{
						num += 1f;
						break;
					}
				}
			}
		}
		return num;
	}
}
