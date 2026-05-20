using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Gravcore_OrbitalMechanoidPlatform : QuestNode_Root_Gravcore
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
			Log.Error("Could not find valid site tile for orbital mechanoid platform quest.");
			return;
		}
		string text = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
		Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
		{
			new SitePartDefWithParams(SitePartDefOf.OrbitalMechanoidPlatform, new SitePartParams
			{
				points = slate.Get("points", 0f),
				threatPoints = slate.Get("points", 0f)
			})
		}, tile, null, hiddenSitePartsPossible: false, null, WorldObjectDefOf.ClaimableSpaceSite);
		slate.Set("site", site);
		quest.SpawnWorldObject(site);
		QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
		choice.rewards.Add(new Reward_DefinedThingDef(ThingDefOf.Gravcore));
		choice.rewards.Add(new Reward_DefinedThingDef(ThingDefOf.GravlitePanel));
		quest.RewardChoice().choices.Add(choice);
		quest.Letter(LetterDefOf.NeutralEvent, text, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, label: "OrbitalMechanoidPlatformLetterArrived".Translate(), text: "OrbitalMechanoidPlatformLetterArrivedText".Translate(), lookTargets: Gen.YieldSingle(site.Map));
		quest.End(QuestEndOutcome.Success, 0, null, text);
		quest.End(QuestEndOutcome.Unknown, 0, null, inSignal);
	}
}
