using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public static class DebugActionsRoyalty
	{
		[DebugAction("General", "Award 4 royal favor", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Award4RoyalFavor()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Faction item in Find.FactionManager.AllFactions.Where((Faction f) => f.def.RoyalTitlesAwardableInSeniorityOrderForReading.Count > 0))
			{
				Faction localFaction = item;
				list.Add(new DebugMenuOption(localFaction.Name, DebugMenuOptionMode.Tool, delegate
				{
					UI.MouseCell().GetFirstPawn(Find.CurrentMap)?.royalty.GainFavor(localFaction, 4);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", "Reduce royal title", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ReduceRoyalTitle()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Faction item in Find.FactionManager.AllFactions.Where((Faction f) => f.def.RoyalTitlesAwardableInSeniorityOrderForReading.Count > 0))
			{
				Faction localFaction = item;
				list.Add(new DebugMenuOption(localFaction.Name, DebugMenuOptionMode.Tool, delegate
				{
					UI.MouseCell().GetFirstPawn(Find.CurrentMap)?.royalty.ReduceTitle(localFaction);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", "Set royal title", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SetTitleForced()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Faction item in Find.FactionManager.AllFactions.Where((Faction f) => f.def.RoyalTitlesAwardableInSeniorityOrderForReading.Count > 0))
			{
				Faction localFaction = item;
				list.Add(new DebugMenuOption(localFaction.Name, DebugMenuOptionMode.Action, delegate
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					RoyalTitleDef localTitleDef = default(RoyalTitleDef);
					foreach (RoyalTitleDef item2 in DefDatabase<RoyalTitleDef>.AllDefsListForReading)
					{
						localTitleDef = item2;
						list2.Add(new DebugMenuOption(localTitleDef.defName, DebugMenuOptionMode.Tool, delegate
						{
							UI.MouseCell().GetFirstPawn(Find.CurrentMap)?.royalty.SetTitle(localFaction, localTitleDef, grantRewards: true);
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		private static void RoyalTitles()
		{
			DebugTables.MakeTablesDialog(DefDatabase<RoyalTitleDef>.AllDefsListForReading, new TableDataGetter<RoyalTitleDef>("defName", (RoyalTitleDef title) => title.defName), new TableDataGetter<RoyalTitleDef>("seniority", (RoyalTitleDef title) => title.seniority), new TableDataGetter<RoyalTitleDef>("favorCost", (RoyalTitleDef title) => title.favorCost), new TableDataGetter<RoyalTitleDef>("Awardable", (RoyalTitleDef title) => title.Awardable), new TableDataGetter<RoyalTitleDef>("minimumExpectationLock", (RoyalTitleDef title) => (title.minExpectation != null) ? title.minExpectation.defName : "NULL"), new TableDataGetter<RoyalTitleDef>("requiredMinimumApparelQuality", (RoyalTitleDef title) => (title.requiredMinimumApparelQuality != 0) ? title.requiredMinimumApparelQuality.ToString() : "None"), new TableDataGetter<RoyalTitleDef>("requireApparel", (RoyalTitleDef title) => (title.requiredApparel != null) ? string.Join(",\r\n", title.requiredApparel.Select((RoyalTitleDef.ApparelRequirement a) => a.ToString()).ToArray()) : "NULL"), new TableDataGetter<RoyalTitleDef>("awardThought", (RoyalTitleDef title) => (title.awardThought != null) ? title.awardThought.defName : "NULL"), new TableDataGetter<RoyalTitleDef>("lostThought", (RoyalTitleDef title) => (title.lostThought != null) ? title.lostThought.defName : "NULL"), new TableDataGetter<RoyalTitleDef>("factions", (RoyalTitleDef title) => string.Join(",", (from f in DefDatabase<FactionDef>.AllDefsListForReading
				where f.RoyalTitlesAwardableInSeniorityOrderForReading.Contains(title)
				select f.defName).ToArray())));
		}

		[DebugOutput(name = "Royal Favor Availability (slow)")]
		private static void RoyalFavorAvailability()
		{
			StorytellerCompProperties_OnOffCycle storytellerCompProperties_OnOffCycle = (StorytellerCompProperties_OnOffCycle)StorytellerDefOf.Cassandra.comps.Find(delegate(StorytellerCompProperties x)
			{
				StorytellerCompProperties_OnOffCycle storytellerCompProperties_OnOffCycle2 = x as StorytellerCompProperties_OnOffCycle;
				if (storytellerCompProperties_OnOffCycle2 == null)
				{
					return false;
				}
				if (storytellerCompProperties_OnOffCycle2.IncidentCategory != IncidentCategoryDefOf.GiveQuest)
				{
					return false;
				}
				return (storytellerCompProperties_OnOffCycle2.enableIfAnyModActive != null && storytellerCompProperties_OnOffCycle2.enableIfAnyModActive.Any((string m) => m.ToLower() == ModContentPack.RoyaltyModPackageId)) ? true : false;
			});
			float onDays = storytellerCompProperties_OnOffCycle.onDays;
			float average = storytellerCompProperties_OnOffCycle.numIncidentsRange.Average;
			float num = average / onDays;
			SimpleCurve simpleCurve = new SimpleCurve
			{
				new CurvePoint(0f, 35f),
				new CurvePoint(15f, 150f),
				new CurvePoint(150f, 5000f)
			};
			int num2 = 0;
			List<RoyalTitleDef> royalTitlesAwardableInSeniorityOrderForReading = FactionDefOf.Empire.RoyalTitlesAwardableInSeniorityOrderForReading;
			for (int i = 0; i < royalTitlesAwardableInSeniorityOrderForReading.Count; i++)
			{
				num2 += royalTitlesAwardableInSeniorityOrderForReading[i].favorCost;
				if (royalTitlesAwardableInSeniorityOrderForReading[i] == RoyalTitleDefOf.Count)
				{
					break;
				}
			}
			float num3 = 0f;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = -1;
			int num9 = -1;
			int num10 = -1;
			int ticksGame = Find.TickManager.TicksGame;
			StoryState storyState = new StoryState(Find.World);
			for (int j = 0; j < 200; j++)
			{
				Find.TickManager.DebugSetTicksGame(j * 60000);
				num3 += num * storytellerCompProperties_OnOffCycle.acceptFractionByDaysPassedCurve.Evaluate(j);
				while (num3 >= 1f)
				{
					num3 -= 1f;
					num4++;
					float points = simpleCurve.Evaluate(j);
					Slate slate = new Slate();
					slate.Set("points", points);
					QuestScriptDef questScriptDef = DefDatabase<QuestScriptDef>.AllDefsListForReading.Where((QuestScriptDef x) => x.IsRootRandomSelected && x.CanRun(slate)).RandomElementByWeight((QuestScriptDef x) => NaturalRandomQuestChooser.GetNaturalRandomSelectionWeight(x, points, storyState));
					Quest quest = QuestGen.Generate(questScriptDef, slate);
					if (quest.InvolvedFactions.Contains(Faction.Empire))
					{
						num7++;
					}
					QuestPart_GiveRoyalFavor questPart_GiveRoyalFavor = (QuestPart_GiveRoyalFavor)quest.PartsListForReading.Find((QuestPart x) => x is QuestPart_GiveRoyalFavor);
					if (questPart_GiveRoyalFavor != null)
					{
						num5 += questPart_GiveRoyalFavor.amount;
						num6++;
						if (num5 >= num2 && num8 < 0)
						{
							num8 = j;
						}
						if (num9 < 0 || questPart_GiveRoyalFavor.amount < num9)
						{
							num9 = questPart_GiveRoyalFavor.amount;
						}
						if (num10 < 0 || questPart_GiveRoyalFavor.amount > num10)
						{
							num10 = questPart_GiveRoyalFavor.amount;
						}
					}
					storyState.RecordRandomQuestFired(questScriptDef);
					quest.CleanupQuestParts();
				}
			}
			Find.TickManager.DebugSetTicksGame(ticksGame);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Results for: Days=" + 200 + ", intervalDays=" + onDays + ", questsPerInterval=" + average + ":");
			stringBuilder.AppendLine("Quests: " + num4);
			stringBuilder.AppendLine("Quests with royal favor: " + num6);
			stringBuilder.AppendLine("Quests from Empire: " + num7);
			stringBuilder.AppendLine("Min royal favor reward: " + num9);
			stringBuilder.AppendLine("Max royal favor reward: " + num10);
			stringBuilder.AppendLine("Total royal favor: " + num5);
			stringBuilder.AppendLine("Royal favor required for Count: " + num2);
			stringBuilder.AppendLine("Count title possible on day: " + num8);
			Log.Message(stringBuilder.ToString());
		}
	}
}
