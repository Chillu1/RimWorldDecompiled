using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Pawn_RoyaltyTracker : IExposable
	{
		public Pawn pawn;

		private List<RoyalTitle> titles = new List<RoyalTitle>();

		private Dictionary<Faction, int> favor = new Dictionary<Faction, int>();

		private Dictionary<Faction, RoyalTitleDef> highestTitles = new Dictionary<Faction, RoyalTitleDef>();

		private Dictionary<Faction, Pawn> heirs = new Dictionary<Faction, Pawn>();

		private Dictionary<RoyalTitlePermitDef, int> permitLastUsedTick = new Dictionary<RoyalTitlePermitDef, int>();

		public int lastDecreeTicks = -999999;

		public bool allowRoomRequirements = true;

		public bool allowApparelRequirements = true;

		private static List<RoyalTitle> EmptyTitles = new List<RoyalTitle>();

		private List<string> tmpDecreeTags = new List<string>();

		private List<Faction> factionHeirsToClearTmp = new List<Faction>();

		private static List<Action> tmpInheritedTitles = new List<Action>();

		public static readonly Texture2D CommandTex = ContentFinder<Texture2D>.Get("UI/Commands/AttackSettlement");

		private List<Faction> tmpFavorFactions;

		private List<Faction> tmpHighestTitleFactions;

		private List<Faction> tmpHeirFactions;

		private List<int> tmpAmounts;

		private List<int> tmlPermitLastUsed;

		private List<Pawn> tmpPawns;

		private List<RoyalTitleDef> tmpTitleDefs;

		private List<RoyalTitlePermitDef> tmpPermitDefs;

		public List<RoyalTitle> AllTitlesForReading => titles;

		public List<RoyalTitle> AllTitlesInEffectForReading
		{
			get
			{
				if (!pawn.IsWildMan())
				{
					return titles;
				}
				return EmptyTitles;
			}
		}

		public RoyalTitle MostSeniorTitle
		{
			get
			{
				List<RoyalTitle> allTitlesInEffectForReading = AllTitlesInEffectForReading;
				int num = -1;
				RoyalTitle royalTitle = null;
				for (int i = 0; i < allTitlesInEffectForReading.Count; i++)
				{
					if (allTitlesInEffectForReading[i].def.seniority > num)
					{
						num = allTitlesInEffectForReading[i].def.seniority;
						royalTitle = allTitlesInEffectForReading[i];
					}
				}
				return royalTitle ?? null;
			}
		}

		public IEnumerable<QuestScriptDef> PossibleDecreeQuests
		{
			get
			{
				tmpDecreeTags.Clear();
				List<RoyalTitle> allTitlesInEffectForReading = AllTitlesInEffectForReading;
				for (int i = 0; i < allTitlesInEffectForReading.Count; i++)
				{
					if (allTitlesInEffectForReading[i].def.decreeTags != null)
					{
						tmpDecreeTags.AddRange(allTitlesInEffectForReading[i].def.decreeTags);
					}
				}
				foreach (QuestScriptDef allDef in DefDatabase<QuestScriptDef>.AllDefs)
				{
					if (allDef.decreeTags != null && allDef.decreeTags.Any((string x) => tmpDecreeTags.Contains(x)))
					{
						Slate slate = new Slate();
						IIncidentTarget mapHeld = pawn.MapHeld;
						slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(mapHeld ?? Find.World));
						slate.Set("asker", pawn);
						if (allDef.CanRun(slate))
						{
							yield return allDef;
						}
					}
				}
			}
		}

		public bool HasAidPermit
		{
			get
			{
				foreach (RoyalTitle item in AllTitlesInEffectForReading)
				{
					if (item.def.permits.NullOrEmpty())
					{
						continue;
					}
					foreach (RoyalTitlePermitDef permit in item.def.permits)
					{
						if (permit.workerClass == typeof(RoyalTitlePermitWorker_CallAid))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public Pawn_RoyaltyTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		private int FindFactionTitleIndex(Faction faction, bool createIfNotExisting = false)
		{
			for (int i = 0; i < titles.Count; i++)
			{
				if (titles[i].faction == faction)
				{
					return i;
				}
			}
			if (createIfNotExisting)
			{
				titles.Add(new RoyalTitle
				{
					faction = faction,
					receivedTick = GenTicks.TicksGame,
					conceited = RoyalTitleUtility.ShouldBecomeConceitedOnNewTitle(pawn)
				});
				return titles.Count - 1;
			}
			return -1;
		}

		public bool HasAnyTitleIn(Faction faction)
		{
			foreach (RoyalTitle title in titles)
			{
				if (title.faction == faction)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasTitle(RoyalTitleDef title)
		{
			foreach (RoyalTitle title2 in titles)
			{
				if (title2.def == title)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasPermit(RoyalTitlePermitDef permit, Faction faction)
		{
			RoyalTitleDef currentTitle = GetCurrentTitle(faction);
			if (currentTitle != null)
			{
				if (currentTitle.permits != null)
				{
					return currentTitle.permits.Contains(permit);
				}
				return false;
			}
			return false;
		}

		public int GetPermitLastUsedTick(RoyalTitlePermitDef permitDef)
		{
			if (!permitLastUsedTick.ContainsKey(permitDef))
			{
				return -1;
			}
			return permitLastUsedTick[permitDef];
		}

		public bool PermitOnCooldown(RoyalTitlePermitDef permitDef)
		{
			int num = GetPermitLastUsedTick(permitDef);
			if (num != -1)
			{
				return GenTicks.TicksGame < num + permitDef.CooldownTicks;
			}
			return false;
		}

		public void Notify_PermitUsed(RoyalTitlePermitDef permitDef)
		{
			if (!permitLastUsedTick.ContainsKey(permitDef))
			{
				permitLastUsedTick.Add(permitDef, GenTicks.TicksGame);
			}
		}

		public RoyalTitleDef MainTitle()
		{
			if (titles.Count == 0)
			{
				return null;
			}
			RoyalTitleDef royalTitleDef = null;
			foreach (RoyalTitle title in titles)
			{
				if (royalTitleDef == null || title.def.seniority > royalTitleDef.seniority)
				{
					royalTitleDef = title.def;
				}
			}
			return royalTitleDef;
		}

		public int GetFavor(Faction faction)
		{
			if (!favor.TryGetValue(faction, out int value))
			{
				return 0;
			}
			return value;
		}

		public void GainFavor(Faction faction, int amount)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Royal favor is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 63699999);
				return;
			}
			if (!favor.TryGetValue(faction, out int value))
			{
				value = 0;
				favor.Add(faction, 0);
			}
			value += amount;
			favor[faction] = value;
			RoyalTitleDef currentTitle = GetCurrentTitle(faction);
			UpdateRoyalTitle(faction);
			RoyalTitleDef currentTitle2 = GetCurrentTitle(faction);
			if (currentTitle2 != currentTitle)
			{
				ApplyRewardsForTitle(faction, currentTitle, currentTitle2);
				OnPostTitleChanged(faction, currentTitle2);
			}
		}

		public bool TryRemoveFavor(Faction faction, int amount)
		{
			int num = GetFavor(faction);
			if (num < amount)
			{
				return false;
			}
			SetFavor(faction, num - amount);
			return true;
		}

		public void SetFavor(Faction faction, int amount)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Royal favor is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 7641236);
			}
			else if (amount == 0 && favor.ContainsKey(faction) && FindFactionTitleIndex(faction) == -1)
			{
				favor.Remove(faction);
			}
			else
			{
				favor.SetOrAdd(faction, amount);
			}
		}

		public RoyalTitleDef GetCurrentTitle(Faction faction)
		{
			return GetCurrentTitleInFaction(faction)?.def;
		}

		public RoyalTitle GetCurrentTitleInFaction(Faction faction)
		{
			if (faction == null)
			{
				Log.Error("Cannot get current title for null faction.");
			}
			int num = FindFactionTitleIndex(faction);
			if (num == -1)
			{
				return null;
			}
			return titles[num];
		}

		public void SetTitle(Faction faction, RoyalTitleDef title, bool grantRewards, bool rewardsOnlyForNewestTitle = false, bool sendLetter = true)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Royal favor is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 7445532);
				return;
			}
			RoyalTitleDef currentTitle = GetCurrentTitle(faction);
			OnPreTitleChanged(faction, currentTitle, title, sendLetter);
			if (grantRewards)
			{
				ApplyRewardsForTitle(faction, currentTitle, title, rewardsOnlyForNewestTitle);
			}
			int index = FindFactionTitleIndex(faction, createIfNotExisting: true);
			if (title != null)
			{
				titles[index].def = title;
				titles[index].receivedTick = GenTicks.TicksGame;
			}
			else
			{
				titles.RemoveAt(index);
			}
			SetFavor(faction, 0);
			OnPostTitleChanged(faction, title);
		}

		public void ReduceTitle(Faction faction)
		{
			RoyalTitleDef currentTitle = GetCurrentTitle(faction);
			if (currentTitle != null && currentTitle.Awardable)
			{
				RoyalTitleDef previousTitle = currentTitle.GetPreviousTitle(faction);
				OnPreTitleChanged(faction, currentTitle, previousTitle);
				CleanupThoughts(currentTitle);
				CleanupThoughts(previousTitle);
				if (currentTitle.awardThought != null && pawn.needs.mood != null)
				{
					Thought_MemoryRoyalTitle thought_MemoryRoyalTitle = (Thought_MemoryRoyalTitle)ThoughtMaker.MakeThought(currentTitle.lostThought);
					thought_MemoryRoyalTitle.titleDef = currentTitle;
					pawn.needs.mood.thoughts.memories.TryGainMemory(thought_MemoryRoyalTitle);
				}
				int index = FindFactionTitleIndex(faction);
				if (previousTitle == null)
				{
					titles.RemoveAt(index);
				}
				else
				{
					titles[index].def = previousTitle;
				}
				SetFavor(faction, 0);
				OnPostTitleChanged(faction, previousTitle);
			}
		}

		public Pawn GetHeir(Faction faction)
		{
			if (heirs != null && heirs.ContainsKey(faction))
			{
				return heirs[faction];
			}
			return null;
		}

		public void SetHeir(Pawn heir, Faction faction)
		{
			if (heirs != null)
			{
				heirs[faction] = heir;
			}
		}

		public void AssignHeirIfNone(RoyalTitleDef t, Faction faction)
		{
			if (!heirs.ContainsKey(faction) && t.Awardable && pawn.FactionOrExtraHomeFaction != Faction.Empire)
			{
				SetHeir(t.GetInheritanceWorker(faction).FindHeir(faction, pawn, t), faction);
			}
		}

		public void RoyaltyTrackerTick()
		{
			List<RoyalTitle> allTitlesInEffectForReading = AllTitlesInEffectForReading;
			for (int i = 0; i < allTitlesInEffectForReading.Count; i++)
			{
				allTitlesInEffectForReading[i].RoyalTitleTick(pawn);
			}
			if (!pawn.Spawned || pawn.RaceProps.Animal)
			{
				return;
			}
			factionHeirsToClearTmp.Clear();
			foreach (KeyValuePair<Faction, Pawn> heir in heirs)
			{
				RoyalTitleDef currentTitle = GetCurrentTitle(heir.Key);
				if (currentTitle != null && currentTitle.canBeInherited)
				{
					Pawn value = heir.Value;
					if (value != null && value.Dead)
					{
						Find.LetterStack.ReceiveLetter("LetterTitleHeirLostLabel".Translate(), "LetterTitleHeirLost".Translate(pawn.Named("HOLDER"), value.Named("HEIR"), heir.Key.Named("FACTION")), LetterDefOf.NegativeEvent, pawn);
						factionHeirsToClearTmp.Add(heir.Key);
					}
				}
			}
			foreach (Faction item in factionHeirsToClearTmp)
			{
				heirs[item] = null;
			}
			for (int num = permitLastUsedTick.Count - 1; num >= 0; num--)
			{
				KeyValuePair<RoyalTitlePermitDef, int> keyValuePair = permitLastUsedTick.ElementAt(num);
				if (!PermitOnCooldown(keyValuePair.Key))
				{
					Messages.Message("MessagePermitCooldownFinished".Translate(pawn, keyValuePair.Key.LabelCap), pawn, MessageTypeDefOf.PositiveEvent);
					permitLastUsedTick.Remove(keyValuePair.Key);
				}
			}
		}

		public void IssueDecree(bool causedByMentalBreak, string mentalBreakReason = null)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Decrees are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 281653);
				return;
			}
			IIncidentTarget mapHeld = pawn.MapHeld;
			IIncidentTarget target = mapHeld ?? Find.World;
			if (PossibleDecreeQuests.TryRandomElementByWeight((QuestScriptDef x) => NaturalRandomQuestChooser.GetNaturalDecreeSelectionWeight(x, target.StoryState), out QuestScriptDef result))
			{
				lastDecreeTicks = Find.TickManager.TicksGame;
				Slate slate = new Slate();
				slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(target));
				slate.Set("asker", pawn);
				Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(result, slate);
				target.StoryState.RecordDecreeFired(result);
				string str = (!causedByMentalBreak) ? ((string)"LetterLabelRandomDecree".Translate(pawn)) : ((string)("WildDecree".Translate() + ": " + pawn.LabelShortCap));
				string text = (!causedByMentalBreak) ? ((string)"LetterRandomDecree".Translate(pawn)) : ((string)"LetterDecreeMentalBreak".Translate(pawn));
				if (mentalBreakReason != null)
				{
					text = text + "\n\n" + mentalBreakReason;
				}
				text += "\n\n" + "LetterDecree_Quest".Translate(quest.name);
				ChoiceLetter let = LetterMaker.MakeLetter(str, text, IncidentDefOf.GiveQuest_Random.letterDef, LookTargets.Invalid, null, quest);
				Find.LetterStack.ReceiveLetter(let);
			}
		}

		private void CleanupThoughts(RoyalTitleDef title)
		{
			if (title != null)
			{
				if (title.awardThought != null && pawn.needs != null && pawn.needs.mood != null)
				{
					pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(title.awardThought);
				}
				if (title.lostThought != null && pawn.needs != null && pawn.needs.mood != null)
				{
					pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(title.lostThought);
				}
			}
		}

		private void OnPreTitleChanged(Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle, bool sendLetter = true)
		{
			AssignHeirIfNone(newTitle, faction);
			if (pawn.IsColonist && sendLetter)
			{
				TaggedString taggedString = null;
				TaggedString taggedString2 = null;
				if (currentTitle == null || faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(currentTitle) < faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(newTitle))
				{
					taggedString = "LetterGainedRoyalTitle".Translate(pawn.Named("PAWN"), faction.Named("FACTION"), newTitle.GetLabelCapFor(pawn).Named("TITLE"));
					taggedString2 = "LetterLabelGainedRoyalTitle".Translate(pawn.Named("PAWN"), newTitle.GetLabelCapFor(pawn).Named("TITLE"));
				}
				else
				{
					taggedString = "LetterLostRoyalTitle".Translate(pawn.Named("PAWN"), faction.Named("FACTION"), currentTitle.GetLabelCapFor(pawn).Named("TITLE"));
					taggedString2 = "LetterLabelLostRoyalTitle".Translate(pawn.Named("PAWN"), currentTitle.GetLabelCapFor(pawn).Named("TITLE"));
				}
				string text = RoyalTitleUtility.BuildDifferenceExplanationText(currentTitle, newTitle, faction, pawn);
				if (text.Length > 0)
				{
					taggedString += "\n\n" + text;
				}
				taggedString = taggedString.Resolve().TrimEndNewlines();
				Find.LetterStack.ReceiveLetter(taggedString2, taggedString, LetterDefOf.PositiveEvent, pawn);
			}
			if (currentTitle != null)
			{
				for (int i = 0; i < currentTitle.grantedAbilities.Count; i++)
				{
					pawn.abilities.RemoveAbility(currentTitle.grantedAbilities[i]);
				}
			}
		}

		private void OnPostTitleChanged(Faction faction, RoyalTitleDef newTitle)
		{
			pawn.Notify_DisabledWorkTypesChanged();
			pawn.needs?.AddOrRemoveNeedsAsAppropriate();
			if (newTitle != null)
			{
				if (newTitle.disabledJoyKinds != null && pawn.jobs != null && RoyalTitleUtility.ShouldBecomeConceitedOnNewTitle(pawn))
				{
					foreach (JoyKindDef disabledJoyKind in newTitle.disabledJoyKinds)
					{
						pawn.jobs.Notify_JoyKindDisabled(disabledJoyKind);
					}
				}
				for (int i = 0; i < newTitle.grantedAbilities.Count; i++)
				{
					pawn.abilities.GainAbility(newTitle.grantedAbilities[i]);
				}
				UpdateHighestTitleAchieved(faction, newTitle);
			}
			QuestUtility.SendQuestTargetSignals(pawn.questTags, "TitleChanged", pawn.Named("SUBJECT"));
			MeditationFocusTypeAvailabilityCache.ClearFor(pawn);
		}

		private void UpdateRoyalTitle(Faction faction)
		{
			RoyalTitleDef currentTitle = GetCurrentTitle(faction);
			if (currentTitle != null && !currentTitle.Awardable)
			{
				return;
			}
			RoyalTitleDef nextTitle = currentTitle.GetNextTitle(faction);
			if (nextTitle == null)
			{
				return;
			}
			int num = GetFavor(faction);
			if (num >= nextTitle.favorCost)
			{
				OnPreTitleChanged(faction, currentTitle, nextTitle);
				SetFavor(faction, num - nextTitle.favorCost);
				int index = FindFactionTitleIndex(faction, createIfNotExisting: true);
				titles[index].def = nextTitle;
				CleanupThoughts(currentTitle);
				CleanupThoughts(nextTitle);
				if (nextTitle.awardThought != null && pawn.needs != null && pawn.needs.mood != null)
				{
					Thought_MemoryRoyalTitle thought_MemoryRoyalTitle = (Thought_MemoryRoyalTitle)ThoughtMaker.MakeThought(nextTitle.awardThought);
					thought_MemoryRoyalTitle.titleDef = nextTitle;
					pawn.needs.mood.thoughts.memories.TryGainMemory(thought_MemoryRoyalTitle);
				}
				UpdateRoyalTitle(faction);
			}
		}

		public List<Thing> ApplyRewardsForTitle(Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle, bool onlyForNewestTitle = false)
		{
			List<Thing> list = new List<Thing>();
			List<ThingCount> list2 = new List<ThingCount>();
			if (newTitle != null && newTitle.Awardable && pawn.IsColonist && NewHighestTitle(faction, newTitle))
			{
				int num = ((currentTitle != null) ? faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(currentTitle) : 0) + 1;
				int num2 = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(newTitle);
				if (onlyForNewestTitle)
				{
					num = num2;
				}
				IntVec3 result = IntVec3.Invalid;
				Map mapHeld = pawn.MapHeld;
				if (mapHeld != null)
				{
					if (mapHeld.IsPlayerHome)
					{
						result = DropCellFinder.TradeDropSpot(mapHeld);
					}
					else if (!DropCellFinder.TryFindDropSpotNear(pawn.Position, mapHeld, out result, allowFogged: false, canRoofPunch: false))
					{
						result = DropCellFinder.RandomDropSpot(mapHeld);
					}
				}
				for (int i = num; i <= num2; i++)
				{
					RoyalTitleDef royalTitleDef = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading[i];
					if (royalTitleDef.rewards == null)
					{
						continue;
					}
					List<Thing> list3 = royalTitleDef.rewards.Select(delegate(ThingDefCountClass r)
					{
						Thing thing = ThingMaker.MakeThing(r.thingDef);
						thing.stackCount = r.count;
						return thing;
					}).ToList();
					for (int j = 0; j < list3.Count; j++)
					{
						if (list3[j].def == ThingDefOf.PsychicAmplifier)
						{
							Find.History.Notify_PsylinkAvailable();
							break;
						}
					}
					if (pawn.Spawned)
					{
						DropPodUtility.DropThingsNear(result, mapHeld, list3, 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: false, forbid: false);
					}
					else
					{
						foreach (Thing item in list3)
						{
							pawn.inventory.TryAddItemNotForSale(item);
						}
					}
					for (int k = 0; k < list3.Count; k++)
					{
						list2.Add(new ThingCount(list3[k], list3[k].stackCount));
					}
					list.AddRange(list3);
				}
				if (list.Count > 0)
				{
					TaggedString text = "LetterRewardsForNewTitle".Translate(pawn.Named("PAWN"), faction.Named("FACTION"), newTitle.GetLabelCapFor(pawn).Named("TITLE")) + "\n\n" + GenLabel.ThingsLabel(list2, "  - ", ignoreStackLimit: true) + "\n\n" + (pawn.Spawned ? "LetterRewardsForNewTitleDeliveryBase" : "LetterRewardsForNewTitleDeliveryDirect").Translate(pawn.Named("PAWN"));
					Find.LetterStack.ReceiveLetter("LetterLabelRewardsForNewTitle".Translate(), text, LetterDefOf.PositiveEvent, list);
				}
			}
			return list;
		}

		private void UpdateHighestTitleAchieved(Faction faction, RoyalTitleDef title)
		{
			if (!highestTitles.ContainsKey(faction))
			{
				highestTitles.Add(faction, title);
			}
			else if (NewHighestTitle(faction, title))
			{
				highestTitles[faction] = title;
			}
		}

		public bool NewHighestTitle(Faction faction, RoyalTitleDef newTitle)
		{
			if (highestTitles == null)
			{
				highestTitles = new Dictionary<Faction, RoyalTitleDef>();
			}
			if (!highestTitles.ContainsKey(faction))
			{
				return true;
			}
			int num = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(highestTitles[faction]);
			return faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(newTitle) > num;
		}

		public void Notify_PawnKilled()
		{
			if (PawnGenerator.IsBeingGenerated(pawn) || AllTitlesForReading.Count == 0)
			{
				return;
			}
			bool flag = false;
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				stringBuilder.AppendLine("LetterTitleInheritance_Base".Translate(pawn.Named("PAWN")));
				stringBuilder.AppendLine();
				foreach (RoyalTitle item in AllTitlesForReading)
				{
					if (!item.def.canBeInherited)
					{
						continue;
					}
					if (pawn.IsFreeColonist && !pawn.IsQuestLodger())
					{
						flag = true;
					}
					if (item.def.TryInherit(pawn, item.faction, out RoyalTitleInheritanceOutcome outcome))
					{
						if (outcome.HeirHasTitle && !outcome.heirTitleHigher)
						{
							stringBuilder.AppendLine("LetterTitleInheritance_AsReplacement".Translate(pawn.Named("PAWN"), item.faction.Named("FACTION"), outcome.heir.Named("HEIR"), item.def.GetLabelFor(pawn).Named("TITLE"), outcome.heirCurrentTitle.GetLabelFor(outcome.heir).Named("REPLACEDTITLE")).CapitalizeFirst().Resolve());
							stringBuilder.AppendLine();
						}
						else if (outcome.heirTitleHigher)
						{
							stringBuilder.AppendLine("LetterTitleInheritance_NoEffectHigherTitle".Translate(pawn.Named("PAWN"), item.faction.Named("FACTION"), outcome.heir.Named("HEIR"), item.def.GetLabelFor(pawn).Named("TITLE"), outcome.heirCurrentTitle.GetLabelFor(outcome.heir).Named("HIGHERTITLE")).CapitalizeFirst().Resolve());
							stringBuilder.AppendLine();
						}
						else
						{
							stringBuilder.AppendLine("LetterTitleInheritance_WasInherited".Translate(pawn.Named("PAWN"), item.faction.Named("FACTION"), outcome.heir.Named("HEIR"), item.def.GetLabelFor(pawn).Named("TITLE")).CapitalizeFirst().Resolve());
							stringBuilder.AppendLine();
						}
						if (!outcome.heirTitleHigher)
						{
							RoyalTitle titleLocal = item;
							tmpInheritedTitles.Add(delegate
							{
								outcome.heir.royalty.SetTitle(titleLocal.faction, titleLocal.def, grantRewards: true, rewardsOnlyForNewestTitle: true);
								titleLocal.wasInherited = true;
							});
							if (outcome.heir.IsFreeColonist && !outcome.heir.IsQuestLodger())
							{
								flag = true;
							}
						}
					}
					else
					{
						stringBuilder.AppendLine("LetterTitleInheritance_NoHeirFound".Translate(pawn.Named("PAWN"), item.def.GetLabelFor(pawn).Named("TITLE"), item.faction.Named("FACTION")).CapitalizeFirst().Resolve());
					}
				}
				if (stringBuilder.Length > 0 && flag)
				{
					Find.LetterStack.ReceiveLetter("LetterTitleInheritance".Translate(), stringBuilder.ToString().TrimEndNewlines(), LetterDefOf.PositiveEvent);
				}
				foreach (Action tmpInheritedTitle in tmpInheritedTitles)
				{
					tmpInheritedTitle();
				}
			}
			finally
			{
				tmpInheritedTitles.Clear();
			}
		}

		public void Notify_Resurrected()
		{
			foreach (Faction item in titles.Select((RoyalTitle t) => t.faction).Distinct().ToList())
			{
				int index = FindFactionTitleIndex(item);
				if (titles[index].wasInherited)
				{
					SetTitle(item, null, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
				}
			}
		}

		public Gizmo RoyalAidGizmo()
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandCallRoyalAid".Translate();
			command_Action.defaultDesc = "CommandCallRoyalAidDesc".Translate();
			command_Action.icon = CommandTex;
			if (pawn.Downed)
			{
				command_Action.Disable("CommandDisabledUnconscious".TranslateWithBackup("CommandCallRoyalAidUnconscious").Formatted(pawn));
			}
			if (pawn.IsQuestLodger())
			{
				command_Action.Disable("CommandCallRoyalAidLodger".Translate());
			}
			command_Action.action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (RoyalTitle item in AllTitlesInEffectForReading.Where((RoyalTitle t) => !t.def.permits.NullOrEmpty()))
				{
					foreach (RoyalTitlePermitDef permit in item.def.permits)
					{
						IEnumerable<FloatMenuOption> royalAidOptions = permit.Worker.GetRoyalAidOptions(pawn.MapHeld, pawn, item.faction);
						if (royalAidOptions != null)
						{
							list.AddRange(royalAidOptions);
						}
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			};
			return command_Action;
		}

		public bool CanRequireThroneroom()
		{
			if (pawn.IsFreeColonist && allowRoomRequirements)
			{
				return !pawn.IsQuestLodger();
			}
			return false;
		}

		public RoyalTitle HighestTitleWithThroneRoomRequirements()
		{
			if (!CanRequireThroneroom())
			{
				return null;
			}
			RoyalTitle royalTitle = null;
			List<RoyalTitle> allTitlesInEffectForReading = AllTitlesInEffectForReading;
			for (int i = 0; i < allTitlesInEffectForReading.Count; i++)
			{
				if (!allTitlesInEffectForReading[i].def.throneRoomRequirements.EnumerableNullOrEmpty() && (royalTitle == null || allTitlesInEffectForReading[i].def.seniority > royalTitle.def.seniority))
				{
					royalTitle = allTitlesInEffectForReading[i];
				}
			}
			return royalTitle;
		}

		public IEnumerable<string> GetUnmetThroneroomRequirements(bool includeOnGracePeriod = true, bool onlyOnGracePeriod = false)
		{
			if (pawn.ownership.AssignedThrone == null)
			{
				yield break;
			}
			RoyalTitle highestTitle = HighestTitleWithThroneRoomRequirements();
			if (highestTitle == null)
			{
				yield break;
			}
			Room throneRoom = pawn.ownership.AssignedThrone.GetRoom();
			if (throneRoom == null)
			{
				yield break;
			}
			RoyalTitleDef prevTitle = highestTitle.def.GetPreviousTitle(highestTitle.faction);
			foreach (RoomRequirement throneRoomRequirement in highestTitle.def.throneRoomRequirements)
			{
				if (!throneRoomRequirement.Met(throneRoom, pawn))
				{
					bool flag = highestTitle.RoomRequirementGracePeriodActive(pawn);
					bool flag2 = prevTitle != null && !prevTitle.HasSameThroneroomRequirement(throneRoomRequirement);
					if ((!onlyOnGracePeriod || (flag2 && flag)) && (!(flag && flag2) || includeOnGracePeriod))
					{
						yield return throneRoomRequirement.LabelCap(throneRoom);
					}
				}
			}
		}

		public bool CanRequireBedroom()
		{
			if (allowRoomRequirements)
			{
				return !pawn.IsPrisoner;
			}
			return false;
		}

		public RoyalTitle HighestTitleWithBedroomRequirements()
		{
			if (!CanRequireBedroom())
			{
				return null;
			}
			RoyalTitle royalTitle = null;
			List<RoyalTitle> allTitlesInEffectForReading = AllTitlesInEffectForReading;
			for (int i = 0; i < allTitlesInEffectForReading.Count; i++)
			{
				if (!allTitlesInEffectForReading[i].def.GetBedroomRequirements(pawn).EnumerableNullOrEmpty() && (royalTitle == null || allTitlesInEffectForReading[i].def.seniority > royalTitle.def.seniority))
				{
					royalTitle = allTitlesInEffectForReading[i];
				}
			}
			return royalTitle;
		}

		public IEnumerable<string> GetUnmetBedroomRequirements(bool includeOnGracePeriod = true, bool onlyOnGracePeriod = false)
		{
			RoyalTitle royalTitle = HighestTitleWithBedroomRequirements();
			if (royalTitle == null)
			{
				yield break;
			}
			bool gracePeriodActive = royalTitle.RoomRequirementGracePeriodActive(pawn);
			RoyalTitleDef prevTitle = royalTitle.def.GetPreviousTitle(royalTitle.faction);
			if (!HasPersonalBedroom())
			{
				yield break;
			}
			Room bedroom = pawn.ownership.OwnedRoom;
			foreach (RoomRequirement bedroomRequirement in royalTitle.def.GetBedroomRequirements(pawn))
			{
				if (!bedroomRequirement.Met(bedroom))
				{
					bool flag = prevTitle != null && !prevTitle.HasSameBedroomRequirement(bedroomRequirement);
					if ((!onlyOnGracePeriod || (flag && gracePeriodActive)) && (!(gracePeriodActive && flag) || includeOnGracePeriod))
					{
						yield return bedroomRequirement.LabelCap(bedroom);
					}
				}
			}
		}

		public bool HasPersonalBedroom()
		{
			Building_Bed ownedBed = pawn.ownership.OwnedBed;
			if (ownedBed == null)
			{
				return false;
			}
			Room ownedRoom = pawn.ownership.OwnedRoom;
			if (ownedRoom == null)
			{
				return false;
			}
			foreach (Building_Bed containedBed in ownedRoom.ContainedBeds)
			{
				if (containedBed != ownedBed && containedBed.OwnersForReading.Any((Pawn p) => p != pawn && !p.RaceProps.Animal && !LovePartnerRelationUtility.LovePartnerRelationExists(p, pawn)))
				{
					return false;
				}
			}
			return true;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref titles, "titles", LookMode.Deep);
			Scribe_Collections.Look(ref favor, "favor", LookMode.Reference, LookMode.Value, ref tmpFavorFactions, ref tmpAmounts);
			Scribe_Values.Look(ref lastDecreeTicks, "lastDecreeTicks", -999999);
			Scribe_Collections.Look(ref highestTitles, "highestTitles", LookMode.Reference, LookMode.Def, ref tmpHighestTitleFactions, ref tmpTitleDefs);
			Scribe_Collections.Look(ref heirs, "heirs", LookMode.Reference, LookMode.Reference, ref tmpHeirFactions, ref tmpPawns);
			Scribe_Collections.Look(ref permitLastUsedTick, "permitLastUsed", LookMode.Def, LookMode.Value, ref tmpPermitDefs, ref tmlPermitLastUsed);
			Scribe_Values.Look(ref allowRoomRequirements, "allowRoomRequirements", defaultValue: true);
			Scribe_Values.Look(ref allowApparelRequirements, "allowApparelRequirements", defaultValue: true);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (titles.RemoveAll((RoyalTitle x) => x.def == null) != 0)
				{
					Log.Error("Some RoyalTitles had null defs after loading.");
				}
				if (heirs == null)
				{
					heirs = new Dictionary<Faction, Pawn>();
				}
				if (permitLastUsedTick == null)
				{
					permitLastUsedTick = new Dictionary<RoyalTitlePermitDef, int>();
				}
				foreach (RoyalTitle title in titles)
				{
					AssignHeirIfNone(title.def, title.faction);
				}
				for (int i = 0; i < AllTitlesInEffectForReading.Count; i++)
				{
					RoyalTitle royalTitle = AllTitlesInEffectForReading[i];
					for (int j = 0; j < royalTitle.def.grantedAbilities.Count; j++)
					{
						pawn.abilities.GainAbility(royalTitle.def.grantedAbilities[j]);
					}
				}
			}
			BackCompatibility.PostExposeData(this);
		}
	}
}
