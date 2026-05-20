using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Gravcore_Mechhive : QuestNode_Root_Gravcore
{
	protected override bool TestRunInt(Slate slate)
	{
		if (Faction.OfMechanoids == null)
		{
			return false;
		}
		return base.TestRunInt(slate);
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Quest quest = QuestGen.quest;
		if (!TryFindSiteTile(out var tile))
		{
			Log.Error("Could not find valid site tile for mechhive quest.");
			return;
		}
		string text = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("core");
		string stabilizerTag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("stabilizer");
		string text2 = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
		string text3 = QuestGenUtility.HardcodedSignalWithQuestID(text + ".CoreDefeated");
		Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
		{
			new SitePartDefWithParams(SitePartDefOf.OrbitalMechhive, new SitePartParams
			{
				points = slate.Get("points", 0f),
				threatPoints = slate.Get("points", 0f)
			})
		}, tile, null, hiddenSitePartsPossible: false, null, WorldObjectDefOf.Mechhive);
		slate.Set("site", site);
		quest.SpawnWorldObject(site);
		QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
		choice.rewards.Add(new Reward_Unknown());
		quest.RewardChoice().choices.Add(choice);
		quest.Letter(LetterDefOf.NeutralEvent, text2, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, label: "OrbitalMechhiveLetterArrived".Translate(), text: "OrbitalMechhiveLetterArrivedText".Translate(), lookTargets: Gen.YieldSingle(site.Map));
		quest.AddPart(new QuestPart_CerebrexCore(site, text2, text, stabilizerTag));
		quest.AddPart(new QuestPart_StabilizersRemainingAlert(site, text2, text3));
		quest.End(QuestEndOutcome.Success, 0, null, text3);
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal);
		QuestPart_QuestEndParent questPart_QuestEndParent = new QuestPart_QuestEndParent();
		questPart_QuestEndParent.inSignal = text3;
		questPart_QuestEndParent.outcome = QuestEndOutcome.Success;
		questPart_QuestEndParent.sendLetter = true;
		questPart_QuestEndParent.signalListenMode = QuestPart.SignalListenMode.Always;
		quest.AddPart(questPart_QuestEndParent);
	}
}
