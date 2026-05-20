using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Gravcore_InsectLair : QuestNode_Root_Gravcore
{
	protected override bool TestRunInt(Slate slate)
	{
		if (Faction.OfInsects == null)
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
			Log.Error("Could not find valid site tile for insect lair quest.");
			return;
		}
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
		Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
		{
			new SitePartDefWithParams(SitePartDefOf.InsectLair, new SitePartParams
			{
				points = slate.Get("points", 0f),
				threatPoints = slate.Get("points", 0f)
			})
		}, tile, Faction.OfInsects, hiddenSitePartsPossible: false, null, WorldObjectDefOf.ClaimableSite);
		slate.Set("site", site);
		quest.SpawnWorldObject(site);
		QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
		choice.rewards.Add(new Reward_DefinedThingDef(ThingDefOf.Gravcore));
		choice.rewards.Add(new Reward_DefinedThingDef(ThingDefOf.GravlitePanel));
		quest.RewardChoice().choices.Add(choice);
		quest.End(QuestEndOutcome.Success, 0, null, inSignal);
		quest.End(QuestEndOutcome.Unknown, 0, null, inSignal2);
	}
}
