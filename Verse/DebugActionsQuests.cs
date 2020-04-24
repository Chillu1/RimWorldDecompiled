using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public static class DebugActionsQuests
	{
		[DebugAction("Quests", "Generate quest", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GenerateQuest()
		{
			GenerateQuests(1, logDescOnly: false);
		}

		[DebugAction("Quests", "Generate quests x10", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GenerateQuests10()
		{
			GenerateQuests(10, logDescOnly: false);
		}

		[DebugAction("Quests", "Generate quests x30", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GenerateQuests30()
		{
			GenerateQuests(30, logDescOnly: false);
		}

		private static void GenerateQuests(int count, bool logDescOnly)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			float localPoints = default(float);
			Slate testSlate = default(Slate);
			Action<QuestScriptDef> generate = delegate(QuestScriptDef script)
			{
				List<DebugMenuOption> list2 = new List<DebugMenuOption>();
				foreach (float item in DebugActionsUtility.PointsOptions(extended: false))
				{
					localPoints = item;
					string text = item.ToString("F0");
					testSlate = new Slate();
					testSlate.Set("points", localPoints);
					if (script != null)
					{
						if (script.IsRootDecree)
						{
							testSlate.Set("asker", PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.RandomElement());
						}
						if (script == QuestScriptDefOf.LongRangeMineralScannerLump)
						{
							testSlate.Set("targetMineable", ThingDefOf.MineableGold);
							testSlate.Set("worker", PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
						}
						if (!script.CanRun(testSlate))
						{
							text += " [not now]";
						}
					}
					list2.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						int num = 0;
						bool flag = script == null;
						for (int i = 0; i < count; i++)
						{
							if (flag)
							{
								script = NaturalRandomQuestChooser.ChooseNaturalRandomQuest(localPoints, Find.CurrentMap);
								Find.CurrentMap.StoryState.RecordRandomQuestFired(script);
							}
							if (script.IsRootDecree)
							{
								Pawn pawn = testSlate.Get<Pawn>("asker");
								if (pawn.royalty.AllTitlesForReading.NullOrEmpty())
								{
									pawn.royalty.SetTitle(Faction.Empire, RoyalTitleDefOf.Knight, grantRewards: false);
									Messages.Message("Dev: Gave " + RoyalTitleDefOf.Knight.label + " title to " + pawn.LabelCap, pawn, MessageTypeDefOf.NeutralEvent, historical: false);
								}
								Find.CurrentMap.StoryState.RecordDecreeFired(script);
							}
							if (count != 1 && !script.CanRun(testSlate))
							{
								num++;
							}
							else if (!logDescOnly)
							{
								QuestUtility.SendLetterQuestAvailable(QuestUtility.GenerateQuestAndMakeAvailable(script, testSlate));
							}
							else
							{
								Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(script, testSlate);
								Log.Message(quest.name + " (" + localPoints + " points)\n--------------\n" + quest.description + "\n--------------");
								Find.QuestManager.Remove(quest);
							}
						}
						if (num != 0)
						{
							Messages.Message("Dev: Generated only " + (count - num) + " quests.", MessageTypeDefOf.RejectInput, historical: false);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
			};
			list.Add(new DebugMenuOption("*Natural random", DebugMenuOptionMode.Action, delegate
			{
				generate(null);
			}));
			foreach (QuestScriptDef item2 in DefDatabase<QuestScriptDef>.AllDefs.Where((QuestScriptDef x) => x.IsRootAny))
			{
				QuestScriptDef localRuleDef = item2;
				string defName = localRuleDef.defName;
				list.Add(new DebugMenuOption(defName, DebugMenuOptionMode.Action, delegate
				{
					generate(localRuleDef);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list.OrderBy((DebugMenuOption op) => op.label)));
		}

		[DebugAction("Quests", "QuestPart test", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TestQuestPart()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type item in typeof(QuestPart).AllSubclassesNonAbstract())
			{
				Type localQuestPartType = item;
				list.Add(new DebugMenuOption(localQuestPartType.Name, DebugMenuOptionMode.Action, delegate
				{
					Quest quest = Quest.MakeRaw();
					quest.name = "DEBUG QUEST (" + localQuestPartType.Name + ")";
					QuestPart questPart = (QuestPart)Activator.CreateInstance(localQuestPartType);
					quest.AddPart(questPart);
					questPart.AssignDebugData();
					quest.description = "A debug quest to test " + localQuestPartType.Name + "\n\n" + Scribe.saver.DebugOutputFor(questPart);
					Find.QuestManager.Add(quest);
					Find.LetterStack.ReceiveLetter("Dev: Quest", quest.description, LetterDefOf.PositiveEvent, LookTargets.Invalid, null, quest);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Quests", "Log generated quest savedata", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void QuestExample()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (QuestScriptDef allDef in DefDatabase<QuestScriptDef>.AllDefs)
			{
				QuestScriptDef localRuleDef = allDef;
				float localPoints = default(float);
				list.Add(new DebugMenuOption(localRuleDef.defName, DebugMenuOptionMode.Action, delegate
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					foreach (float item in DebugActionsUtility.PointsOptions(extended: true))
					{
						localPoints = item;
						list2.Add(new DebugMenuOption(item.ToString("F0"), DebugMenuOptionMode.Action, delegate
						{
							Slate slate = new Slate();
							slate.Set("points", localPoints);
							Quest saveable = QuestGen.Generate(localRuleDef, slate);
							Log.Message(Scribe.saver.DebugOutputFor(saveable));
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void QuestDefs()
		{
			Slate slate = new Slate();
			slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(Find.World));
			DebugTables.MakeTablesDialog(from x in DefDatabase<QuestScriptDef>.AllDefs
				orderby x.IsRootRandomSelected descending, x.IsRootDecree descending
				select x, new TableDataGetter<QuestScriptDef>("defName", (QuestScriptDef d) => d.defName), new TableDataGetter<QuestScriptDef>("points\nmin", (QuestScriptDef d) => (!(d.rootMinPoints > 0f)) ? "" : d.rootMinPoints.ToString()), new TableDataGetter<QuestScriptDef>("increases\npop", (QuestScriptDef d) => d.rootIncreasesPopulation.ToStringCheckBlank()), new TableDataGetter<QuestScriptDef>("root\nweight", (QuestScriptDef d) => (!(d.rootSelectionWeight > 0f)) ? "" : d.rootSelectionWeight.ToString()), new TableDataGetter<QuestScriptDef>("decree\nweight", (QuestScriptDef d) => (!(d.decreeSelectionWeight > 0f)) ? "" : d.decreeSelectionWeight.ToString()), new TableDataGetter<QuestScriptDef>("decree\ntags", (QuestScriptDef d) => d.decreeTags.ToCommaList()), new TableDataGetter<QuestScriptDef>("auto\naccept", (QuestScriptDef d) => d.autoAccept.ToStringCheckBlank()), new TableDataGetter<QuestScriptDef>("expiry\ndays", (QuestScriptDef d) => (!(d.expireDaysRange.TrueMax > 0f)) ? "" : d.expireDaysRange.ToString()), new TableDataGetter<QuestScriptDef>("CanRun\nnow", (QuestScriptDef d) => d.CanRun(slate).ToStringCheckBlank()), new TableDataGetter<QuestScriptDef>("canGiveRoyalFavor", (QuestScriptDef d) => d.canGiveRoyalFavor.ToStringCheckBlank()), new TableDataGetter<QuestScriptDef>("possible rewards", delegate(QuestScriptDef d)
			{
				RewardsGeneratorParams? rewardsParams = null;
				bool multiple = false;
				slate.Set<Action<QuestNode, Slate>>("testRunCallback", delegate(QuestNode node, Slate curSlate)
				{
					QuestNode_GiveRewards questNode_GiveRewards = node as QuestNode_GiveRewards;
					if (questNode_GiveRewards != null)
					{
						if (rewardsParams.HasValue)
						{
							multiple = true;
						}
						else
						{
							rewardsParams = questNode_GiveRewards.parms.GetValue(curSlate);
						}
					}
				});
				bool flag = d.CanRun(slate);
				slate.Remove("testRunCallback");
				if (multiple)
				{
					return "complex";
				}
				if (rewardsParams.HasValue)
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (rewardsParams.Value.allowGoodwill)
					{
						stringBuilder.AppendWithComma("goodwill");
					}
					if (rewardsParams.Value.allowRoyalFavor)
					{
						stringBuilder.AppendWithComma("favor");
					}
					return stringBuilder.ToString();
				}
				return (!flag) ? "unknown" : "";
			}), new TableDataGetter<QuestScriptDef>("weight histogram", delegate(QuestScriptDef d)
			{
				string text = "";
				for (float num = 0f; num < d.rootSelectionWeight; num += 0.05f)
				{
					text += "*";
					if (num > 3f)
					{
						text += "*";
						break;
					}
				}
				return text;
			}));
		}

		[DebugOutput]
		public static void QuestSelectionWeightsNow()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (float item in DebugActionsUtility.PointsOptions(extended: true))
			{
				float localPoints = item;
				list.Add(new DebugMenuOption(localPoints + " points", DebugMenuOptionMode.Action, delegate
				{
					IIncidentTarget target = Find.CurrentMap;
					string label = "selection weight now\ntarget: " + target.ToString() + "\npoints: " + localPoints.ToString("F0") + "\npopIntentQuest: " + StorytellerUtilityPopulation.PopulationIntentForQuest;
					DebugTables.MakeTablesDialog(DefDatabase<QuestScriptDef>.AllDefsListForReading.Where((QuestScriptDef x) => x.IsRootRandomSelected), new TableDataGetter<QuestScriptDef>("defName", (QuestScriptDef x) => x.defName), new TableDataGetter<QuestScriptDef>(label, (QuestScriptDef x) => NaturalRandomQuestChooser.GetNaturalRandomSelectionWeight(x, localPoints, target.StoryState).ToString("F3")), new TableDataGetter<QuestScriptDef>("increases\npopulation", (QuestScriptDef x) => x.rootIncreasesPopulation.ToStringCheckBlank()), new TableDataGetter<QuestScriptDef>("recency\nindex", (QuestScriptDef x) => (!target.StoryState.RecentRandomQuests.Contains(x)) ? "" : target.StoryState.RecentRandomQuests.IndexOf(x).ToString()), new TableDataGetter<QuestScriptDef>("total\nselection\nchance\nnow", (QuestScriptDef x) => NaturalRandomQuestChooser.DebugTotalNaturalRandomSelectionWeight(x, localPoints, target).ToString("F3")));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list.OrderBy((DebugMenuOption op) => op.label)));
		}

		[DebugOutput]
		public static void DecreeSelectionWeightsNow()
		{
			IIncidentTarget target = Find.CurrentMap;
			string label = "selection weight now\ntarget: " + target.ToString();
			DebugTables.MakeTablesDialog(DefDatabase<QuestScriptDef>.AllDefsListForReading.Where((QuestScriptDef x) => x.IsRootDecree), new TableDataGetter<QuestScriptDef>("defName", (QuestScriptDef x) => x.defName), new TableDataGetter<QuestScriptDef>(label, (QuestScriptDef x) => NaturalRandomQuestChooser.GetNaturalDecreeSelectionWeight(x, target.StoryState).ToString("F3")));
		}
	}
}
