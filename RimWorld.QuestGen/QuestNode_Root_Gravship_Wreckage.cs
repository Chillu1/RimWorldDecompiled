using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Root_Gravship_Wreckage : QuestNode_Root_Gravcore
	{
		private bool? scenarioMechSignalDisabled;

		private bool ScenarioMechSignalDisabled
		{
			get
			{
				if (!scenarioMechSignalDisabled.HasValue)
				{
					scenarioMechSignalDisabled = false;
					Scenario scenario = Find.Scenario;
					for (int i = 0; i < scenario.parts.Count; i++)
					{
						if (scenario.parts[i] is ScenPart_DisableQuest scenPart_DisableQuest && scenPart_DisableQuest.questDef == QuestScriptDefOf.MechanoidSignal)
						{
							scenarioMechSignalDisabled = true;
						}
					}
				}
				return scenarioMechSignalDisabled.Value;
			}
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Quest quest = QuestGen.quest;
			if (!TryFindSiteTile(out var tile))
			{
				Log.Error("Could not find valid site tile for gravengine wreckage quest.");
				return;
			}
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
			string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
			Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
			{
				new SitePartDefWithParams(SitePartDefOf.GravshipWreckage, new SitePartParams
				{
					points = slate.Get("points", 0f),
					threatPoints = slate.Get("points", 0f)
				})
			}, tile, null, hiddenSitePartsPossible: false, null, WorldObjectDefOf.ClaimableSite);
			site.questTags = new List<string> { QuestGen.slate.CurrentPrefix };
			slate.Set("site", site);
			quest.SpawnWorldObject(site);
			Pawn pawn = FindAsker();
			slate.Set("asker", pawn);
			slate.Set("askerIsNull", pawn == null);
			QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
			choice.rewards.Add(new Reward_DefinedThingDef(ThingDefOf.GravEngine));
			quest.RewardChoice().choices.Add(choice);
			quest.End(QuestEndOutcome.Success, 0, null, inSignal);
			quest.End(QuestEndOutcome.Unknown, 0, null, inSignal2);
			quest.AddPart<QuestPart_EngineClaimed>();
		}

		protected override bool TestRunInt(Slate slate)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			if (!ResearchProjectDefOf.BasicGravtech.IsFinished && !ScenarioMechSignalDisabled)
			{
				return false;
			}
			if (GravshipUtility.PlayerHasGravEngine())
			{
				return false;
			}
			foreach (Quest item in Find.QuestManager.questsInDisplayOrder)
			{
				if (item.State == QuestState.Ongoing && item.root == QuestScriptDefOf.GravshipWreckage)
				{
					return false;
				}
			}
			return base.TestRunInt(slate);
		}

		private Pawn FindAsker()
		{
			if (Find.FactionManager.AllFactionsVisible.Where((Faction f) => f.def.humanlikeFaction && !f.IsPlayer && !f.HostileTo(Faction.OfPlayer) && (int)f.def.techLevel > 2 && f.leader != null && !f.temporary && !f.Hidden).TryRandomElement(out var result))
			{
				return result.leader;
			}
			return null;
		}
	}
}
