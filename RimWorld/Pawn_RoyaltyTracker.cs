using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.QuestGen;
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

		private List<FactionPermit> factionPermits = new List<FactionPermit>();

		private List<Ability> abilities = new List<Ability>();

		public int lastDecreeTicks = -999999;

		public bool allowRoomRequirements = true;

		public bool allowApparelRequirements = true;

		private static List<RoyalTitle> EmptyTitles = new List<RoyalTitle>();

		private const int BestowingCeremonyCheckInterval = 37500;

		private List<string> tmpDecreeTags = new List<string>();

		private static List<FactionPermit> tmpPermits = new List<FactionPermit>();

		private List<Faction> factionHeirsToClearTmp = new List<Faction>();

		private static List<Action> tmpInheritedTitles = new List<Action>();

		public static readonly Texture2D CommandTex = ContentFinder<Texture2D>.Get("UI/Commands/CallAid");

		private List<Faction> tmpFavorFactions;

		private List<Faction> tmpHighestTitleFactions;

		private List<Faction> tmpHeirFactions;

		private List<int> tmpAmounts;

		private List<Pawn> tmpPawns;

		private List<RoyalTitleDef> tmpTitleDefs;

		public List<RoyalTitle> AllTitlesForReading => titles;

		public List<RoyalTitle> AllTitlesInEffectForReading
		{
			get
			{
				if (!pawn.IsWildMan() && (!pawn.IsMutant || !pawn.mutant.Def.disableTitles))
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

		public List<Ability> AllAbilitiesForReading => abilities;

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
						mapHeld = pawn.MapHeld;
						if (allDef.CanRun(slate, mapHeld ?? Find.World))
						{
							yield return allDef;
						}
					}
				}
			}
		}

		public bool PermitPointsAvailable
		{
			get
			{
				foreach (Faction item in Find.FactionManager.AllFactionsVisible)
				{
					if (GetPermitPoints(item) > 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public List<FactionPermit> AllFactionPermits => factionPermits;

		public bool HasAidPermit => factionPermits.Any();

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
					conceited = RoyalTitleUtility.ShouldBecomeConceitedOnNewTitle(pawn),
					pawn = pawn
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

		public List<FactionPermit> PermitsFromFaction(Faction faction)
		{
			tmpPermits.Clear();
			foreach (FactionPermit factionPermit in factionPermits)
			{
				if (factionPermit.Faction == faction)
				{
					tmpPermits.Add(factionPermit);
				}
			}
			return tmpPermits;
		}

		public void AddPermit(RoyalTitlePermitDef permit, Faction faction)
		{
			if (!ModLister.CheckRoyalty("Permit") || HasPermit(permit, faction))
			{
				return;
			}
			if (permit.prerequisite != null && HasPermit(permit.prerequisite, faction))
			{
				FactionPermit item = factionPermits.Find((FactionPermit x) => x.Permit == permit.prerequisite && x.Faction == faction);
				factionPermits.Remove(item);
			}
			factionPermits.Add(new FactionPermit(faction, GetCurrentTitle(faction), permit));
		}

		public void RefundPermits(int favorCost, Faction faction)
		{
			if (favor[faction] < favorCost)
			{
				Log.Error("Not enough favor to refund permits.");
				return;
			}
			bool flag = false;
			for (int num = factionPermits.Count - 1; num >= 0; num--)
			{
				if (factionPermits[num].Faction == faction)
				{
					factionPermits.RemoveAt(num);
					flag = true;
				}
			}
			if (flag)
			{
				int num2 = GetFavor(faction);
				favor[faction] -= favorCost;
				OnFavorChanged(faction, num2, num2 - favorCost);
			}
		}

		public bool HasPermit(RoyalTitlePermitDef permit, Faction faction)
		{
			foreach (FactionPermit factionPermit in factionPermits)
			{
				if (factionPermit.Permit == permit && factionPermit.Faction == faction)
				{
					return true;
				}
			}
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
			if (!favor.TryGetValue(faction, out var value))
			{
				return 0;
			}
			return value;
		}

		public FactionPermit GetPermit(RoyalTitlePermitDef permit, Faction faction)
		{
			foreach (FactionPermit factionPermit in factionPermits)
			{
				if (factionPermit.Permit == permit && factionPermit.Faction == faction)
				{
					return factionPermit;
				}
			}
			return null;
		}

		public int GetPermitPoints(Faction faction)
		{
			int num = 0;
			RoyalTitleDef royalTitleDef = GetCurrentTitle(faction);
			int num2 = 200;
			while (royalTitleDef != null)
			{
				num += royalTitleDef.permitPointsAwarded;
				royalTitleDef = royalTitleDef.GetPreviousTitle_IncludeNonRewardable(faction);
				num2--;
				if (num2 <= 0)
				{
					Log.ErrorOnce("GetPermitPoints exceeded iterations limit.", 1837503);
					break;
				}
			}
			for (int i = 0; i < factionPermits.Count; i++)
			{
				if (factionPermits[i].Faction != faction)
				{
					continue;
				}
				RoyalTitlePermitDef royalTitlePermitDef = factionPermits[i].Permit;
				num2 = 200;
				while (royalTitlePermitDef != null)
				{
					num -= royalTitlePermitDef.permitPointCost;
					royalTitlePermitDef = royalTitlePermitDef.prerequisite;
					num2--;
					if (num2 <= 0)
					{
						Log.ErrorOnce("GetPermitPoints exceeded iterations limit.", 1837503);
						break;
					}
				}
			}
			return num;
		}

		public void GainFavor(Faction faction, int amount)
		{
			if (ModLister.CheckRoyalty("Honor"))
			{
				int oldAmount = GetFavor(faction);
				if (!favor.TryGetValue(faction, out var value))
				{
					value = 0;
					favor.Add(faction, 0);
				}
				value += amount;
				favor[faction] = value;
				if (amount < 0)
				{
					TryUpdateTitle(faction);
				}
				OnFavorChanged(faction, oldAmount, value);
			}
		}

		public RoyalTitleDef GetTitleAwardedWhenUpdating(Faction faction, int favor)
		{
			RoyalTitleDef royalTitleDef = GetCurrentTitle(faction);
			RoyalTitleDef result = null;
			while (favor > 0 && royalTitleDef.GetNextTitle(faction) != null)
			{
				royalTitleDef = ((royalTitleDef != null) ? royalTitleDef.GetNextTitle(faction) : faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.First());
				favor -= royalTitleDef.favorCost;
				if (favor >= 0)
				{
					result = royalTitleDef;
				}
			}
			return result;
		}

		public bool CanUpdateTitleOfAnyFaction(out Faction faction)
		{
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				if (CanUpdateTitle(allFaction))
				{
					faction = allFaction;
					return true;
				}
			}
			faction = null;
			return false;
		}

		public bool CanUpdateTitle(Faction faction)
		{
			RoyalTitleDef currentTitle = GetCurrentTitle(faction);
			int num = GetFavor(faction);
			RoyalTitleDef nextTitle = currentTitle.GetNextTitle(faction);
			if (nextTitle == null)
			{
				return false;
			}
			return num >= nextTitle.favorCost;
		}

		public bool TryUpdateTitle(Faction faction, bool sendLetter = true, RoyalTitleDef updateTo = null)
		{
			RoyalTitleDef currentTitle = GetCurrentTitle(faction);
			UpdateRoyalTitle(faction, sendLetter, updateTo);
			RoyalTitleDef currentTitle2 = GetCurrentTitle(faction);
			if (currentTitle2 != currentTitle)
			{
				ApplyRewardsForTitle(faction, currentTitle, currentTitle2);
				OnPostTitleChanged(faction, currentTitle, currentTitle2);
			}
			return currentTitle2 != currentTitle;
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

		public void SetFavor(Faction faction, int amount, bool notifyOnFavorChanged = true)
		{
			if (ModLister.CheckRoyalty("Honor"))
			{
				int oldAmount = GetFavor(faction);
				if (amount == 0 && favor.ContainsKey(faction) && FindFactionTitleIndex(faction) == -1)
				{
					favor.Remove(faction);
				}
				else
				{
					favor.SetOrAdd(faction, amount);
				}
				if (notifyOnFavorChanged)
				{
					OnFavorChanged(faction, oldAmount, amount);
				}
			}
		}

		private void OnFavorChanged(Faction faction, int oldAmount, int newAmount)
		{
			RoyalTitleDef titleAwardedWhenUpdating = GetTitleAwardedWhenUpdating(faction, oldAmount);
			RoyalTitleDef titleAwardedWhenUpdating2 = GetTitleAwardedWhenUpdating(faction, newAmount);
			RoyalTitle currentTitleInFaction = GetCurrentTitleInFaction(faction);
			RoyalTitleDef royalTitleDef = null;
			if (currentTitleInFaction != null && titleAwardedWhenUpdating != null)
			{
				royalTitleDef = ((currentTitleInFaction.def.seniority >= titleAwardedWhenUpdating.seniority) ? currentTitleInFaction.def : titleAwardedWhenUpdating);
			}
			else if (currentTitleInFaction != null)
			{
				royalTitleDef = currentTitleInFaction.def;
			}
			else if (titleAwardedWhenUpdating != null)
			{
				royalTitleDef = titleAwardedWhenUpdating;
			}
			if (royalTitleDef == titleAwardedWhenUpdating2)
			{
				return;
			}
			List<RoyalTitleDef> list = new List<RoyalTitleDef>();
			int previousTitleSeniority = royalTitleDef?.seniority ?? (-1);
			int newAwardedSeniority = titleAwardedWhenUpdating2?.seniority ?? (-1);
			if (previousTitleSeniority < newAwardedSeniority)
			{
				list.AddRange(faction.def.RoyalTitlesAllInSeniorityOrderForReading.Where((RoyalTitleDef t) => t.seniority > previousTitleSeniority && t.seniority <= newAwardedSeniority));
			}
			else
			{
				list.Add(titleAwardedWhenUpdating2);
			}
			foreach (RoyalTitleDef item in list)
			{
				if (item != null && pawn.Faction != null && pawn.Faction.IsPlayer)
				{
					item.AwardWorker.OnPreAward(pawn, faction, royalTitleDef, item);
				}
				QuestUtility.SendQuestTargetSignals(pawn.questTags, "TitleAwardedWhenUpdatingChanged");
				if (item != null && pawn.Faction != null && pawn.Faction.IsPlayer)
				{
					item.AwardWorker.DoAward(pawn, faction, royalTitleDef, item);
				}
				royalTitleDef = item;
			}
			if (previousTitleSeniority < newAwardedSeniority)
			{
				RoyalTitleUtility.EndExistingBestowingCeremonyQuest(pawn, faction);
				RoyalTitleUtility.GenerateBestowingCeremonyQuest(pawn, faction);
			}
		}

		public RoyalTitleDef GetCurrentTitle(Faction faction)
		{
			RoyalTitle currentTitleInFaction = GetCurrentTitleInFaction(faction);
			if (AllTitlesInEffectForReading.Contains(currentTitleInFaction))
			{
				return currentTitleInFaction?.def;
			}
			return null;
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
			if (ModLister.CheckRoyalty("Honor"))
			{
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
				OnPostTitleChanged(faction, currentTitle, title);
			}
		}

		public void ReduceTitle(Faction faction)
		{
			RoyalTitleDef currentTitle = GetCurrentTitle(faction);
			if (currentTitle == null || !currentTitle.Awardable)
			{
				return;
			}
			for (int num = factionPermits.Count - 1; num >= 0; num--)
			{
				if (factionPermits[num].Title == currentTitle)
				{
					factionPermits.RemoveAt(num);
				}
			}
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
			OnPostTitleChanged(faction, currentTitle, previousTitle);
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
			if (!heirs.ContainsKey(faction) && t.Awardable && (Faction.OfEmpire == null || pawn.HomeFaction != Faction.OfEmpire))
			{
				SetHeir(t.GetInheritanceWorker(faction).FindHeir(faction, pawn, t), faction);
			}
		}

		public void RoyaltyTrackerTickInterval(int delta)
		{
			if (pawn.IsHashIntervalTick(37500, delta) && RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(pawn, out var faction))
			{
				RoyalTitleUtility.GenerateBestowingCeremonyQuest(pawn, faction);
			}
			List<RoyalTitle> allTitlesInEffectForReading = AllTitlesInEffectForReading;
			for (int i = 0; i < allTitlesInEffectForReading.Count; i++)
			{
				allTitlesInEffectForReading[i].RoyalTitleTick(delta);
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
			foreach (FactionPermit factionPermit in factionPermits)
			{
				if (factionPermit.LastUsedTick > 0 && Find.TickManager.TicksGame == factionPermit.LastUsedTick + factionPermit.Permit.CooldownTicks)
				{
					Messages.Message("MessagePermitCooldownFinished".Translate(pawn, factionPermit.Permit.LabelCap), pawn, MessageTypeDefOf.PositiveEvent);
				}
			}
		}

		public void IssueDecree(bool causedByMentalBreak, string mentalBreakReason = null)
		{
			if (!ModLister.CheckRoyalty("Decree"))
			{
				return;
			}
			IIncidentTarget mapHeld = pawn.MapHeld;
			IIncidentTarget target = mapHeld ?? Find.World;
			if (PossibleDecreeQuests.TryRandomElementByWeight((QuestScriptDef x) => NaturalRandomQuestChooser.GetNaturalDecreeSelectionWeight(x, target.StoryState), out var result))
			{
				lastDecreeTicks = Find.TickManager.TicksGame;
				Slate slate = new Slate();
				slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(target));
				slate.Set("asker", pawn);
				Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(result, slate);
				target.StoryState.RecordDecreeFired(result);
				string text = ((!causedByMentalBreak) ? ((string)"LetterLabelRandomDecree".Translate(pawn)) : ((string)("WildDecree".Translate() + ": " + pawn.LabelShortCap)));
				string text2 = ((!causedByMentalBreak) ? ((string)"LetterRandomDecree".Translate(pawn)) : ((string)"LetterDecreeMentalBreak".Translate(pawn)));
				if (mentalBreakReason != null)
				{
					text2 = text2 + "\n\n" + mentalBreakReason;
				}
				text2 += "\n\n" + "LetterDecree_Quest".Translate(quest.name);
				ChoiceLetter choiceLetter = LetterMaker.MakeLetter(text, text2, IncidentDefOf.GiveQuest_Random.letterDef, LookTargets.Invalid, null, quest);
				Find.LetterStack.ReceiveLetter(choiceLetter);
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

		public static void MakeLetterTextForTitleChange(Pawn pawn, Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle, out string headline, out string body)
		{
			if (currentTitle == null || faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(currentTitle) < faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(newTitle))
			{
				headline = "LetterGainedRoyalTitle".Translate(pawn.Named("PAWN"), faction.Named("FACTION"), newTitle.GetLabelCapFor(pawn).Named("TITLE")).Resolve();
			}
			else
			{
				headline = "LetterLostRoyalTitle".Translate(pawn.Named("PAWN"), faction.Named("FACTION"), currentTitle.GetLabelCapFor(pawn).Named("TITLE")).Resolve();
			}
			body = RoyalTitleUtility.BuildDifferenceExplanationText(currentTitle, newTitle, faction, pawn);
			body = ((TaggedString)body).Resolve().TrimEndNewlines();
		}

		public static string MakeLetterTextForTitleChange(Pawn pawn, Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle)
		{
			MakeLetterTextForTitleChange(pawn, faction, currentTitle, newTitle, out var headline, out var body);
			if (body.Length > 0)
			{
				body = "\n\n" + body;
			}
			return headline + body;
		}

		public void ResetPermitsAndPoints(Faction faction, RoyalTitleDef currentTitle)
		{
			if (currentTitle == null)
			{
				return;
			}
			for (int num = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(currentTitle) + 1 - 1; num >= 0; num--)
			{
				RoyalTitleDef royalTitleDef = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading[num];
				List<FactionPermit> list = PermitsFromFaction(faction);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].Title == royalTitleDef)
					{
						factionPermits.Remove(list[i]);
					}
				}
			}
		}

		private void OnPreTitleChanged(Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle, bool sendLetter = true)
		{
			if (newTitle != null)
			{
				AssignHeirIfNone(newTitle, faction);
			}
			if (Current.ProgramState == ProgramState.Playing && sendLetter && pawn.IsColonist)
			{
				TaggedString taggedString = null;
				taggedString = ((currentTitle != null && faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(currentTitle) >= faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(newTitle)) ? "LetterLabelLostRoyalTitle".Translate(pawn.Named("PAWN"), currentTitle.GetLabelCapFor(pawn).Named("TITLE")) : "LetterLabelGainedRoyalTitle".Translate(pawn.Named("PAWN"), newTitle.GetLabelCapFor(pawn).Named("TITLE")));
				Find.LetterStack.ReceiveLetter(taggedString, MakeLetterTextForTitleChange(pawn, faction, currentTitle, newTitle), LetterDefOf.PositiveEvent, pawn);
			}
			if (currentTitle != null)
			{
				for (int i = 0; i < currentTitle.grantedAbilities.Count; i++)
				{
					pawn.abilities.RemoveAbility(currentTitle.grantedAbilities[i]);
				}
			}
		}

		private void OnPostTitleChanged(Faction faction, RoyalTitleDef prevTitle, RoyalTitleDef newTitle)
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
				UpdateAvailableAbilities();
				pawn.abilities.Notify_TemporaryAbilitiesChanged();
				UpdateHighestTitleAchieved(faction, newTitle);
			}
			QuestUtility.SendQuestTargetSignals(pawn.questTags, "TitleChanged", pawn.Named("SUBJECT"));
			MeditationFocusTypeAvailabilityCache.ClearFor(pawn);
			pawn.apparel?.Notify_TitleChanged();
		}

		private void UpdateRoyalTitle(Faction faction, bool sendLetter = true, RoyalTitleDef updateTo = null)
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
				OnPreTitleChanged(faction, currentTitle, nextTitle, sendLetter);
				SetFavor(faction, num - nextTitle.favorCost, notifyOnFavorChanged: false);
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
				if (nextTitle != updateTo)
				{
					UpdateRoyalTitle(faction, sendLetter);
				}
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
					for (int num3 = 0; num3 < list3.Count; num3++)
					{
						if (list3[num3].def == ThingDefOf.PsychicAmplifier)
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
					for (int num4 = 0; num4 < list3.Count; num4++)
					{
						list2.Add(new ThingCount(list3[num4], list3[num4].stackCount));
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

		public IEnumerable<Gizmo> GetGizmos()
		{
			if (!PermitPointsAvailable || pawn.Faction != Faction.OfPlayer)
			{
				yield break;
			}
			Faction faction = null;
			foreach (Faction item in Find.FactionManager.AllFactionsVisibleInViewOrder)
			{
				if (GetPermitPoints(item) > 0)
				{
					faction = item;
					break;
				}
			}
			if (faction != null)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "ChooseRoyalPermit".Translate();
				command_Action.defaultDesc = "ChooseRoyalPermit_Desc".Translate();
				command_Action.icon = faction.def.FactionIcon;
				command_Action.defaultIconColor = faction.Color;
				command_Action.action = delegate
				{
					OpenPermitWindow();
				};
				command_Action.Order = -100f;
				yield return command_Action;
			}
		}

		public void OpenPermitWindow()
		{
			Dialog_InfoCard dialog_InfoCard = new Dialog_InfoCard(pawn);
			dialog_InfoCard.SetTab(Dialog_InfoCard.InfoCardTab.Permits);
			Find.WindowStack.Add(dialog_InfoCard);
		}

		public void Notify_PawnKilled()
		{
			if (PawnGenerator.IsBeingGenerated(pawn) || AllTitlesForReading.Count == 0 || (pawn.IsMutant && pawn.mutant.Def.disableTitles))
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
					if (item.def.TryInherit(pawn, item.faction, out var outcome))
					{
						if (outcome.HeirHasTitle && !outcome.heirTitleHigher)
						{
							stringBuilder.AppendLineTagged("LetterTitleInheritance_AsReplacement".Translate(pawn.Named("PAWN"), item.faction.Named("FACTION"), outcome.heir.Named("HEIR"), item.def.GetLabelFor(pawn).Named("TITLE"), outcome.heirCurrentTitle.GetLabelFor(outcome.heir).Named("REPLACEDTITLE")).CapitalizeFirst());
							stringBuilder.AppendLine();
						}
						else if (outcome.heirTitleHigher)
						{
							stringBuilder.AppendLineTagged("LetterTitleInheritance_NoEffectHigherTitle".Translate(pawn.Named("PAWN"), item.faction.Named("FACTION"), outcome.heir.Named("HEIR"), item.def.GetLabelFor(pawn).Named("TITLE"), outcome.heirCurrentTitle.GetLabelFor(outcome.heir).Named("HIGHERTITLE")).CapitalizeFirst());
							stringBuilder.AppendLine();
						}
						else
						{
							stringBuilder.AppendLineTagged("LetterTitleInheritance_WasInherited".Translate(pawn.Named("PAWN"), item.faction.Named("FACTION"), outcome.heir.Named("HEIR"), item.def.GetLabelFor(pawn).Named("TITLE")).CapitalizeFirst());
							stringBuilder.AppendLine();
						}
						if (outcome.heirTitleHigher)
						{
							continue;
						}
						RoyalTitle titleLocal = item;
						tmpInheritedTitles.Add(delegate
						{
							int num = titleLocal.def.favorCost;
							RoyalTitleDef previousTitle_IncludeNonRewardable = titleLocal.def.GetPreviousTitle_IncludeNonRewardable(titleLocal.faction);
							int num2 = 1000;
							while (previousTitle_IncludeNonRewardable != null)
							{
								num += previousTitle_IncludeNonRewardable.favorCost;
								previousTitle_IncludeNonRewardable = previousTitle_IncludeNonRewardable.GetPreviousTitle_IncludeNonRewardable(titleLocal.faction);
								num2--;
								if (num2 <= 0)
								{
									Log.ErrorOnce("Iterations limit exceeded while getting favor for inheritance.", 91727191);
									break;
								}
							}
							outcome.heir.royalty.GainFavor(titleLocal.faction, num);
							titleLocal.wasInherited = true;
						});
						if (outcome.heir.IsFreeColonist && !outcome.heir.IsQuestLodger())
						{
							flag = true;
						}
					}
					else
					{
						stringBuilder.AppendLineTagged("LetterTitleInheritance_NoHeirFound".Translate(pawn.Named("PAWN"), item.def.GetLabelFor(pawn).Named("TITLE"), item.faction.Named("FACTION")).CapitalizeFirst());
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
				if (!titles[index].wasInherited)
				{
					continue;
				}
				List<FactionPermit> list = PermitsFromFaction(item);
				for (int num = 0; num < list.Count; num++)
				{
					if (list[num].Title == titles[index].def)
					{
						factionPermits.Remove(list[num]);
					}
				}
				SetTitle(item, null, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
			}
			UpdateAvailableAbilities();
		}

		public Gizmo RoyalAidGizmo()
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandCallRoyalAid".Translate();
			command_Action.defaultDesc = "CommandCallRoyalAidDesc".Translate();
			command_Action.icon = CommandTex;
			if (Find.Selector.NumSelected > 1)
			{
				command_Action.defaultLabel = command_Action.defaultLabel + " (" + pawn.LabelShort + ")";
			}
			if (pawn.Spawned && pawn.Map.weatherManager.CurWeatherMaxRangeCap >= 0f)
			{
				command_Action.defaultDescPostfix = "\n\n" + ("WeatherMaxRangeCap".Translate() + ": " + pawn.Map.weatherManager.curWeather.LabelCap).Colorize(ColoredText.WarningColor);
			}
			if (pawn.Downed)
			{
				command_Action.Disable("CommandDisabledUnconscious".TranslateWithBackup("CommandCallRoyalAidUnconscious").Formatted(pawn));
			}
			if (pawn.IsSlave)
			{
				command_Action.Disable("CommandCallRoyalAidSlave".Translate());
			}
			if (pawn.IsQuestLodger())
			{
				command_Action.Disable("CommandCallRoyalAidLodger".Translate());
			}
			Map mapHeld = pawn.MapHeld;
			if (mapHeld != null && mapHeld.IsPocketMap && mapHeld.generatorDef.disableCallAid)
			{
				command_Action.Disable("CommandCallRoyalAidInvalidMap".Translate());
			}
			command_Action.action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (FactionPermit factionPermit in factionPermits)
				{
					IEnumerable<FloatMenuOption> royalAidOptions = factionPermit.Permit.Worker.GetRoyalAidOptions(pawn.MapHeld, pawn, factionPermit.Faction);
					if (royalAidOptions != null)
					{
						list.AddRange(royalAidOptions);
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
			Building_Throne assignedThrone = pawn.ownership.AssignedThrone;
			if (assignedThrone == null || !assignedThrone.SpawnedOrAnyParentSpawned || GenTicks.TicksGame - assignedThrone.MapHeld.generationTick < 10)
			{
				yield break;
			}
			RoyalTitle royalTitle = HighestTitleWithThroneRoomRequirements();
			if (royalTitle == null)
			{
				yield break;
			}
			Room throneRoom = assignedThrone.GetRoom();
			if (throneRoom == null)
			{
				yield break;
			}
			bool roomValid = RoomRoleWorker_ThroneRoom.Validate(throneRoom) == null;
			bool gracePeriodActive = royalTitle.RoomRequirementGracePeriodActive(pawn);
			RoyalTitleDef prevTitle = royalTitle.def.GetPreviousTitle(royalTitle.faction);
			foreach (RoomRequirement throneRoomRequirement in royalTitle.def.throneRoomRequirements)
			{
				if (!roomValid || !throneRoomRequirement.MetOrDisabled(throneRoom, pawn))
				{
					bool flag = (prevTitle != null && !prevTitle.HasSameThroneroomRequirement(throneRoomRequirement)) || MoveColonyUtility.TitleAndRoleRequirementsGracePeriodActive;
					if ((!onlyOnGracePeriod || (flag && gracePeriodActive)) && (!(gracePeriodActive && flag) || includeOnGracePeriod))
					{
						yield return throneRoomRequirement.LabelCap(throneRoom);
					}
				}
			}
		}

		public bool AnyUnmetThroneroomRequirements()
		{
			if (pawn.ownership.AssignedThrone == null)
			{
				return false;
			}
			RoyalTitle royalTitle = HighestTitleWithThroneRoomRequirements();
			if (royalTitle == null)
			{
				return false;
			}
			Room room = pawn.ownership.AssignedThrone.GetRoom();
			if (room == null)
			{
				return false;
			}
			if (RoomRoleWorker_ThroneRoom.Validate(room) != null)
			{
				return true;
			}
			foreach (RoomRequirement throneRoomRequirement in royalTitle.def.throneRoomRequirements)
			{
				if (!throneRoomRequirement.MetOrDisabled(room, pawn))
				{
					return true;
				}
			}
			return false;
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

		public void UpdateAvailableAbilities()
		{
			abilities.RemoveAll(delegate(Ability a)
			{
				for (int i = 0; i < AllTitlesInEffectForReading.Count; i++)
				{
					if (AllTitlesInEffectForReading[i].def.grantedAbilities.Contains(a.def))
					{
						return false;
					}
				}
				return true;
			});
			for (int num = 0; num < AllTitlesInEffectForReading.Count; num++)
			{
				RoyalTitle royalTitle = AllTitlesInEffectForReading[num];
				for (int num2 = 0; num2 < royalTitle.def.grantedAbilities.Count; num2++)
				{
					AbilityDef def = royalTitle.def.grantedAbilities[num2];
					if (!abilities.Any((Ability a) => a.def == def))
					{
						abilities.Add(AbilityUtility.MakeAbility(royalTitle.def.grantedAbilities[num2], pawn));
					}
				}
			}
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
				if (!bedroomRequirement.MetOrDisabled(bedroom, pawn))
				{
					bool flag = (prevTitle != null && !prevTitle.HasSameBedroomRequirement(bedroomRequirement)) || MoveColonyUtility.TitleAndRoleRequirementsGracePeriodActive;
					if ((!onlyOnGracePeriod || (flag && gracePeriodActive)) && (!(gracePeriodActive && flag) || includeOnGracePeriod))
					{
						yield return bedroomRequirement.LabelCap(bedroom);
					}
				}
			}
		}

		public bool AnyUnmetBedroomRequirements()
		{
			RoyalTitle royalTitle = HighestTitleWithBedroomRequirements();
			if (royalTitle == null)
			{
				return false;
			}
			if (!HasPersonalBedroom())
			{
				return false;
			}
			Room ownedRoom = pawn.ownership.OwnedRoom;
			foreach (RoomRequirement bedroomRequirement in royalTitle.def.GetBedroomRequirements(pawn))
			{
				if (!bedroomRequirement.MetOrDisabled(ownedRoom, pawn))
				{
					return true;
				}
			}
			return false;
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
				if (containedBed != ownedBed)
				{
					List<Pawn> loveCluster = pawn.GetLoveCluster();
					if (containedBed.OwnersForReading.Any((Pawn p) => p != pawn && !p.RaceProps.Animal && !loveCluster.Contains(p)))
					{
						return false;
					}
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
			Scribe_Values.Look(ref allowRoomRequirements, "allowRoomRequirements", defaultValue: true);
			Scribe_Values.Look(ref allowApparelRequirements, "allowApparelRequirements", defaultValue: true);
			Scribe_Collections.Look(ref factionPermits, "permits", LookMode.Deep);
			Scribe_Collections.Look(ref abilities, "abilities", LookMode.Deep, pawn);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (titles.RemoveAll((RoyalTitle x) => x.def == null) != 0)
				{
					Log.Error("Some RoyalTitles had null defs after loading.");
				}
				foreach (RoyalTitle title in titles)
				{
					title.pawn = pawn;
				}
				if (heirs == null)
				{
					heirs = new Dictionary<Faction, Pawn>();
				}
				if (factionPermits == null)
				{
					factionPermits = new List<FactionPermit>();
				}
				if (factionPermits.RemoveAll((FactionPermit x) => DefDatabase<RoyalTitlePermitDef>.AllDefs.Any((RoyalTitlePermitDef y) => y.prerequisite == x.Permit && HasPermit(y, x.Faction) && HasPermit(y.prerequisite, x.Faction))) != 0)
				{
					Log.Error("Removed some null permits.");
				}
				foreach (RoyalTitle title2 in titles)
				{
					AssignHeirIfNone(title2.def, title2.faction);
				}
				if (abilities == null)
				{
					abilities = new List<Ability>();
				}
				UpdateAvailableAbilities();
			}
			BackCompatibility.PostExposeData(this);
		}
	}
}
