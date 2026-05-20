using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;

namespace Verse;

public static class DebugActionsQuests
{
	public static float lastQuestGeneratedRewardValue;

	private static readonly int[] QuestNumOptions = new int[7] { 1, 2, 5, 10, 25, 50, 100 };

	private static readonly float[] QuestRewardDebugPointLevels = new float[8] { 35f, 100f, 200f, 400f, 800f, 1600f, 3200f, 6000f };

	[DebugAction("Quests", "Generate quest", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static List<DebugActionNode> GenerateQuest()
	{
		return GenerateQuests(1);
	}

	[DebugAction("Quests", "Generate quests x#", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static List<DebugActionNode> GenerateQuests()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		for (int i = 0; i < QuestNumOptions.Length; i++)
		{
			int count = QuestNumOptions[i];
			DebugActionNode debugActionNode = new DebugActionNode(count.ToString());
			foreach (DebugActionNode item in GenerateQuests(count))
			{
				debugActionNode.AddChild(item);
			}
			list.Add(debugActionNode);
		}
		return list;
	}

	[DebugAction("Quests", "Generate quests (1x for each points)", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static List<DebugActionNode> GenerateQuestsSamples()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (QuestScriptDef scriptDef in DefDatabase<QuestScriptDef>.AllDefs.Where((QuestScriptDef x) => x.IsRootAny))
		{
			list.Add(new DebugActionNode(scriptDef.defName, DebugActionType.Action, delegate
			{
				foreach (float item in DebugActionsUtility.PointsOptions(extended: false))
				{
					try
					{
						QuestScriptDef questScriptDef = scriptDef;
						float points = item;
						IIncidentTarget randomPlayerHomeMap = Find.RandomPlayerHomeMap;
						if (!questScriptDef.CanRun(points, randomPlayerHomeMap ?? Find.World))
						{
							Log.Error("Cannot generate quest " + scriptDef.defName + " for " + item + " points!");
						}
						else
						{
							Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(scriptDef, item);
							quest.name = item + ": " + quest.name;
						}
					}
					catch (Exception ex)
					{
						Log.Error("Exception generating quest " + scriptDef.defName + " for " + item + " points!\n\n" + ex.Message + "\n-------------\n" + ex.StackTrace);
					}
				}
			}));
		}
		return list;
	}

	private static List<DebugActionNode> GenerateQuests(int count, bool logDescOnly = false)
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		list.Add(new DebugActionNode("*Natural random", DebugActionType.Action, delegate
		{
			Slate slate2 = new Slate();
			QuestScriptDef script = NaturalRandomQuestChooser.ChooseNaturalRandomQuest(slate2.Get("points", 0f), Find.CurrentMap);
			GetQuest(script, slate2);
		}));
		foreach (QuestScriptDef item in DefDatabase<QuestScriptDef>.AllDefs.Where((QuestScriptDef x) => x.IsRootAny))
		{
			QuestScriptDef localScriptDef = item;
			DebugActionNode debugActionNode = new DebugActionNode(localScriptDef.defName);
			Slate slate = new Slate();
			slate.Set("discoveryMethod", "QuestDiscoveredFromDebug".Translate());
			if (localScriptDef.affectedByPoints && localScriptDef.affectedByPopulation)
			{
				debugActionNode.childGetter = delegate
				{
					List<DebugActionNode> list2 = new List<DebugActionNode>();
					foreach (float item2 in DebugActionsUtility.PointsOptions(extended: false))
					{
						string text = $"{item2:F0} points";
						GetSlateValuesForPoints(localScriptDef, slate, item2);
						QuestScriptDef questScriptDef = localScriptDef;
						Slate slate2 = slate;
						IIncidentTarget currentMap = Find.CurrentMap;
						if (!questScriptDef.CanRun(slate2, currentMap ?? Find.World))
						{
							text += " [not now]";
						}
						DebugActionNode debugActionNode2 = new DebugActionNode(text);
						Slate localSlate = slate.DeepCopy();
						debugActionNode2.childGetter = () => SelectPopulation(localScriptDef, localSlate, generate: true);
						list2.Add(debugActionNode2);
					}
					return list2;
				};
			}
			else if (item.affectedByPoints)
			{
				debugActionNode.childGetter = () => SelectPoints(localScriptDef, slate, generate: true);
			}
			else if (localScriptDef.affectedByPopulation)
			{
				debugActionNode.childGetter = () => SelectPopulation(localScriptDef, slate, generate: true);
			}
			else
			{
				debugActionNode.action = delegate
				{
					GetQuest(localScriptDef, slate);
				};
			}
			list.Add(debugActionNode);
		}
		return list.OrderBy((DebugActionNode op) => op.label).ToList();
		void GetQuest(QuestScriptDef script, Slate slate2)
		{
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				if (script.IsRootDecree)
				{
					Pawn pawn = slate2.Get<Pawn>("asker");
					if (pawn.royalty.AllTitlesForReading.NullOrEmpty() && Faction.OfEmpire != null)
					{
						pawn.royalty.SetTitle(Faction.OfEmpire, RoyalTitleDefOf.Knight, grantRewards: false);
						Messages.Message("DEV: Gave " + RoyalTitleDefOf.Knight.label + " title to " + pawn.LabelCap, pawn, MessageTypeDefOf.NeutralEvent, historical: false);
					}
					Find.CurrentMap.StoryState.RecordDecreeFired(script);
				}
				IIncidentTarget currentMap = Find.CurrentMap;
				if (!script.CanRun(slate2, currentMap ?? Find.World))
				{
					if (count == 1 && !script.affectedByPoints && !script.affectedByPopulation)
					{
						Messages.Message("DEV: Failed to generate quest. CanRun returned false.", MessageTypeDefOf.RejectInput, historical: false);
					}
					else if (count > 1)
					{
						num++;
					}
				}
				else if (!logDescOnly)
				{
					Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(script, slate2);
					if (!quest.hidden && quest.root.sendAvailableLetter)
					{
						QuestUtility.SendLetterQuestAvailable(quest);
					}
				}
				else
				{
					Quest quest2 = QuestUtility.GenerateQuestAndMakeAvailable(script, slate2);
					string text = quest2.name;
					if (slate2.Exists("points"))
					{
						text = text + "(" + slate2.Get("points", 0f) + " points)";
					}
					if (slate2.Exists("population"))
					{
						text = text + "(" + slate2.Get("population", 0) + " population)";
					}
					text += "\n--------------\n" + quest2.description + "\n--------------";
					Log.Message(text);
					Find.QuestManager.Remove(quest2);
				}
			}
			if (num != 0)
			{
				Messages.Message("DEV: Generated only " + (count - num) + " quests.", MessageTypeDefOf.RejectInput, historical: false);
			}
		}
		static void GetSlateValuesForPoints(QuestScriptDef script, Slate slate2, float points)
		{
			if (script != null)
			{
				if (script.IsRootDecree)
				{
					slate2.Set("asker", PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.RandomElement());
				}
				if (script == QuestScriptDefOf.LongRangeMineralScannerLump)
				{
					slate2.Set("targetMineableThing", ThingDefOf.Gold);
					slate2.Set("targetMineable", ThingDefOf.MineableGold);
					slate2.Set("worker", PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
				}
				slate2.Set("points", points);
			}
		}
		List<DebugActionNode> SelectPoints(QuestScriptDef script, Slate slate2, bool generate)
		{
			List<DebugActionNode> list2 = new List<DebugActionNode>();
			foreach (float item3 in DebugActionsUtility.PointsOptions(extended: false))
			{
				float localPoints = item3;
				string text = $"{item3:F0} points";
				GetSlateValuesForPoints(script, slate2, item3);
				QuestScriptDef questScriptDef = script;
				Slate slate3 = slate2;
				IIncidentTarget currentMap = Find.CurrentMap;
				if (!questScriptDef.CanRun(slate3, currentMap ?? Find.World))
				{
					text += " [not now]";
				}
				list2.Add(new DebugActionNode(text, DebugActionType.Action, delegate
				{
					slate2.Set("points", localPoints);
					if (generate)
					{
						GetQuest(script, slate2);
					}
				}));
			}
			return list2;
		}
		List<DebugActionNode> SelectPopulation(QuestScriptDef script, Slate slate2, bool generate)
		{
			List<DebugActionNode> list2 = new List<DebugActionNode>
			{
				new DebugActionNode("*Don't set", DebugActionType.Action, delegate
				{
					slate2.Remove("population");
					if (generate)
					{
						GetQuest(script, slate2);
					}
				})
			};
			foreach (int item4 in DebugActionsUtility.PopulationOptions())
			{
				int localPopulation = item4;
				string text = $"{item4:F0} colony population";
				slate2.Set("population", localPopulation);
				QuestScriptDef questScriptDef = script;
				Slate slate3 = slate2;
				IIncidentTarget currentMap = Find.CurrentMap;
				if (!questScriptDef.CanRun(slate3, currentMap ?? Find.World))
				{
					text += " [not now]";
				}
				list2.Add(new DebugActionNode(text, DebugActionType.Action, delegate
				{
					slate2.Set("population", localPopulation);
					if (generate)
					{
						GetQuest(script, slate2);
					}
				}));
			}
			return list2;
		}
	}

	[DebugAction("Quests", "QuestPart test", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static List<DebugActionNode> TestQuestPart()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (Type item in typeof(QuestPart).AllSubclassesNonAbstract())
		{
			Type localQuestPartType = item;
			list.Add(new DebugActionNode(localQuestPartType.Name, DebugActionType.Action, delegate
			{
				Quest quest = Quest.MakeRaw();
				quest.name = "DEBUG QUEST (" + localQuestPartType.Name + ")";
				QuestPart questPart = (QuestPart)Activator.CreateInstance(localQuestPartType);
				quest.AddPart(questPart);
				questPart.AssignDebugData();
				quest.description = "A debug quest to test " + localQuestPartType.Name + "\n\n" + Scribe.saver.DebugOutputFor(questPart);
				Find.QuestManager.Add(quest);
				Find.LetterStack.ReceiveLetter("DEV: Quest", quest.description, LetterDefOf.PositiveEvent, LookTargets.Invalid, null, quest);
			}));
		}
		return list;
	}

	[DebugAction("Quests", "Log generated quest savedata", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	public static void QuestExample()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (QuestScriptDef allDef in DefDatabase<QuestScriptDef>.AllDefs)
		{
			QuestScriptDef localRuleDef = allDef;
			list.Add(new DebugMenuOption(localRuleDef.defName, DebugMenuOptionMode.Action, delegate
			{
				List<DebugMenuOption> list2 = new List<DebugMenuOption>();
				foreach (float item in DebugActionsUtility.PointsOptions(extended: true))
				{
					float localPoints = item;
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
	public static void QuestRewardsSampled()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (QuestScriptDef quest in DefDatabase<QuestScriptDef>.AllDefsListForReading)
		{
			if (!quest.IsRootAny)
			{
				continue;
			}
			QuestScriptDef localQuest = quest;
			list.Add(new DebugMenuOption(quest.defName, DebugMenuOptionMode.Action, delegate
			{
				Dictionary<float, int> numQuestsRating1 = new Dictionary<float, int>();
				Dictionary<float, int> numQuestsRating2 = new Dictionary<float, int>();
				Dictionary<float, int> numQuestsRating3 = new Dictionary<float, int>();
				Dictionary<float, float> rewardRating1min = new Dictionary<float, float>();
				Dictionary<float, float> rewardRating1max = new Dictionary<float, float>();
				Dictionary<float, float> rewardRating1accumulated = new Dictionary<float, float>();
				Dictionary<float, float> rewardRating2min = new Dictionary<float, float>();
				Dictionary<float, float> rewardRating2max = new Dictionary<float, float>();
				Dictionary<float, float> rewardRating2accumulated = new Dictionary<float, float>();
				Dictionary<float, float> rewardRating3min = new Dictionary<float, float>();
				Dictionary<float, float> rewardRating3max = new Dictionary<float, float>();
				Dictionary<float, float> rewardRating3accumulated = new Dictionary<float, float>();
				float[] questRewardDebugPointLevels = QuestRewardDebugPointLevels;
				foreach (float num in questRewardDebugPointLevels)
				{
					if (!(num < quest.rootMinPoints))
					{
						numQuestsRating1.Add(num, 0);
						numQuestsRating2.Add(num, 0);
						numQuestsRating3.Add(num, 0);
						Slate slate = new Slate();
						slate.Set("points", num);
						slate.Set("debugDontGenerateRewardThings", var: true);
						rewardRating1min.Add(num, 9999999f);
						rewardRating2min.Add(num, 9999999f);
						rewardRating3min.Add(num, 9999999f);
						rewardRating1max.Add(num, -9999999f);
						rewardRating2max.Add(num, -9999999f);
						rewardRating3max.Add(num, -9999999f);
						rewardRating1accumulated.Add(num, 0f);
						rewardRating2accumulated.Add(num, 0f);
						rewardRating3accumulated.Add(num, 0f);
						for (int j = 0; j < 20; j++)
						{
							lastQuestGeneratedRewardValue = 0f;
							Quest quest2 = QuestGen.Generate(localQuest, slate.DeepCopy());
							float num2 = lastQuestGeneratedRewardValue;
							if (quest2.challengeRating == 1)
							{
								numQuestsRating1[num]++;
								rewardRating1min[num] = Mathf.Min(rewardRating1min[num], num2);
								rewardRating1max[num] = Mathf.Max(rewardRating1max[num], num2);
								rewardRating1accumulated[num] += num2;
							}
							else if (quest2.challengeRating == 2)
							{
								numQuestsRating2[num]++;
								rewardRating2min[num] = Mathf.Min(rewardRating2min[num], num2);
								rewardRating2max[num] = Mathf.Max(rewardRating2max[num], num2);
								rewardRating2accumulated[num] += num2;
							}
							else if (quest2.challengeRating == 3)
							{
								numQuestsRating3[num]++;
								rewardRating3min[num] = Mathf.Min(rewardRating3min[num], num2);
								rewardRating3max[num] = Mathf.Max(rewardRating3max[num], num2);
								rewardRating3accumulated[num] += num2;
							}
						}
					}
				}
				DebugTables.MakeTablesDialog(QuestRewardDebugPointLevels, new TableDataGetter<float>("points", (float v) => v.ToString()), new TableDataGetter<float>("rating 1\nquest count\nof " + 20, (float v) => numQuestsRating1[v].ToString()), new TableDataGetter<float>("rating 1\nrewardValue\nmin", (float v) => (rewardRating1min[v] != 9999999f) ? ((int)rewardRating1min[v]).ToString() : "-"), new TableDataGetter<float>("rating 1\nrewardValue\navg", (float v) => (!(rewardRating1accumulated[v] <= 0f)) ? ((int)(rewardRating1accumulated[v] / (float)numQuestsRating1[v])).ToString() : "-"), new TableDataGetter<float>("rating 1\nrewardValue\nmax", (float v) => (rewardRating1max[v] != -9999999f) ? ((int)rewardRating1max[v]).ToString() : "-"), new TableDataGetter<float>("rating 2\nquest count\nof " + 20, (float v) => numQuestsRating2[v].ToString()), new TableDataGetter<float>("rating 2\nrewardValue\nmin", (float v) => (rewardRating2min[v] != 9999999f) ? ((int)rewardRating2min[v]).ToString() : "-"), new TableDataGetter<float>("rating 2\nrewardValue\navg", (float v) => (!(rewardRating2accumulated[v] <= 0f)) ? ((int)(rewardRating2accumulated[v] / (float)numQuestsRating2[v])).ToString() : "-"), new TableDataGetter<float>("rating 2\nrewardValue\nmax", (float v) => (rewardRating2max[v] != -9999999f) ? ((int)rewardRating2max[v]).ToString() : "-"), new TableDataGetter<float>("rating 3\nquest count\nof " + 20, (float v) => numQuestsRating3[v].ToString()), new TableDataGetter<float>("rating 3\nrewardValue\nmin", (float v) => (rewardRating3min[v] != 9999999f) ? ((int)rewardRating3min[v]).ToString() : "-"), new TableDataGetter<float>("rating 3\nrewardValue\navg", (float v) => (!(rewardRating3accumulated[v] <= 0f)) ? ((int)(rewardRating3accumulated[v] / (float)numQuestsRating3[v])).ToString() : "-"), new TableDataGetter<float>("rating 3\nrewardValue\nmax", (float v) => (rewardRating3max[v] != -9999999f) ? ((int)rewardRating3max[v]).ToString() : "-"));
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
			select x, new TableDataGetter<QuestScriptDef>("defName", (QuestScriptDef d) => d.defName), new TableDataGetter<QuestScriptDef>("points\nmin", (QuestScriptDef d) => (!(d.rootMinPoints > 0f)) ? "" : d.rootMinPoints.ToString()), new TableDataGetter<QuestScriptDef>("progress\nmin", (QuestScriptDef d) => (!(d.rootMinProgressScore > 0f)) ? "" : d.rootMinProgressScore.ToString()), new TableDataGetter<QuestScriptDef>("increases\npop", (QuestScriptDef d) => d.rootIncreasesPopulation.ToStringCheckBlank()), new TableDataGetter<QuestScriptDef>("root\nweight", (QuestScriptDef d) => (!(d.rootSelectionWeight > 0f)) ? "" : d.rootSelectionWeight.ToString()), new TableDataGetter<QuestScriptDef>("decree\nweight", (QuestScriptDef d) => (!(d.decreeSelectionWeight > 0f)) ? "" : d.decreeSelectionWeight.ToString()), new TableDataGetter<QuestScriptDef>("decree\ntags", (QuestScriptDef d) => d.decreeTags.ToCommaList()), new TableDataGetter<QuestScriptDef>("auto\naccept", (QuestScriptDef d) => d.autoAccept.ToStringCheckBlank()), new TableDataGetter<QuestScriptDef>("expiry\ndays", (QuestScriptDef d) => (!(d.expireDaysRange.TrueMax > 0f)) ? "" : d.expireDaysRange.ToString("0.##")), new TableDataGetter<QuestScriptDef>("refire\ndays", (QuestScriptDef d) => (!(d.minRefireDays > 0f)) ? "" : d.minRefireDays.ToString("0.##")), new TableDataGetter<QuestScriptDef>("CanRun\nnow", delegate(QuestScriptDef d)
		{
			Slate slate2 = slate;
			IIncidentTarget currentMap = Find.CurrentMap;
			return d.CanRun(slate2, currentMap ?? Find.World).ToStringCheckBlank();
		}), new TableDataGetter<QuestScriptDef>("canGiveRoyalFavor", (QuestScriptDef d) => d.canGiveRoyalFavor.ToStringCheckBlank()), new TableDataGetter<QuestScriptDef>("possible rewards", delegate(QuestScriptDef d)
		{
			RewardsGeneratorParams? rewardsParams = null;
			bool multiple = false;
			slate.Set<Action<QuestNode, Slate>>("testRunCallback", delegate(QuestNode node, Slate curSlate)
			{
				if (node is QuestNode_GiveRewards questNode_GiveRewards)
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
			Slate slate2 = slate;
			IIncidentTarget currentMap = Find.CurrentMap;
			bool flag = d.CanRun(slate2, currentMap ?? Find.World);
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
