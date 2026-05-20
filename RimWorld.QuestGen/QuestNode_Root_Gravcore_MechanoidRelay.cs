using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Gravcore_MechanoidRelay : QuestNode_Root_Gravcore
{
	private const string StabilizerAlias = "stabilizer";

	private const string RelayAlias = "relay";

	private const int StablizerCount = 3;

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
			Log.Error("Could not find valid site tile for mechanoid relay quest.");
			return;
		}
		string questTagToAdd = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("MechanoidRelay");
		string text = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
		string text2 = QuestGen.GenerateNewSignal("StabilizerDealtWith");
		string text3 = QuestGen.GenerateNewSignal("AllStabilizersDealtWith");
		Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
		{
			new SitePartDefWithParams(SitePartDefOf.MechanoidRelay, new SitePartParams
			{
				points = slate.Get("points", 0f),
				threatPoints = slate.Get("points", 0f),
				stabilizerCount = 3
			})
		}, tile, Faction.OfMechanoids, hiddenSitePartsPossible: false, null, WorldObjectDefOf.ClaimableSite);
		slate.Set("site", site);
		quest.SpawnWorldObject(site);
		List<Thing> list = new List<Thing>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < site.parts[0].things.Count; i++)
		{
			Thing thing = site.parts[0].things[i];
			QuestUtility.AddQuestTag(ref thing.questTags, questTagToAdd);
			if (thing.def == ThingDefOf.MechStabilizer)
			{
				AddStabilizer(thing, site, quest, slate, text, list, list2);
			}
			else if (thing.def == ThingDefOf.MechRelay)
			{
				slate.Set("relay", thing);
			}
		}
		quest.SignalPassAny(null, list2, text2);
		QuestPart_Filter_AllThingsHackedOrDestroyed questPart_Filter_AllThingsHackedOrDestroyed = new QuestPart_Filter_AllThingsHackedOrDestroyed();
		questPart_Filter_AllThingsHackedOrDestroyed.things.AddRange(list);
		questPart_Filter_AllThingsHackedOrDestroyed.inSignal = text2;
		questPart_Filter_AllThingsHackedOrDestroyed.outSignal = text3;
		quest.AddPart(questPart_Filter_AllThingsHackedOrDestroyed);
		quest.AddPart(new QuestPart_MechRelay
		{
			inSignal = text3,
			relay = slate.Get<Thing>("relay")
		});
		quest.Delay(10000, delegate
		{
			quest.RandomRaid(site, FloatRange.One, Faction.OfMechanoids, null, PawnsArrivalModeDefOf.RandomDrop, RaidStrategyDefOf.ImmediateAttack, "GravcoreMechRelayRaidLetterLabel".Translate(), "GravcoreMechRelayRaidLetterText".Translate(), useCurrentThreatPoints: true);
		}, text3);
		quest.Letter(LetterDefOf.NeutralEvent, text, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[letterTextMapGenerated]", null, "[letterLabelMapGenerated]");
		QuestPart_RelayStabilizersRemainingAlert questPart_RelayStabilizersRemainingAlert = new QuestPart_RelayStabilizersRemainingAlert(site, text, text3, text2, list.Count);
		questPart_RelayStabilizersRemainingAlert.things.AddRange(list);
		quest.AddPart(questPart_RelayStabilizersRemainingAlert);
		QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
		choice.rewards.Add(new Reward_DefinedThingDef(ThingDefOf.Gravcore));
		choice.rewards.Add(new Reward_DefinedThingDef(ThingDefOf.GravlitePanel));
		quest.RewardChoice().choices.Add(choice);
		quest.End(QuestEndOutcome.Success, 0, null, text3);
		quest.End(QuestEndOutcome.Unknown, 0, null, inSignal);
	}

	private void AddStabilizer(Thing thing, Site site, Quest quest, Slate slate, string mapGeneratedSignal, List<Thing> stabilizers, List<string> signalsDealtWith)
	{
		string text = string.Format("{0}{1}", "stabilizer", stabilizers.Count);
		slate.Set(text, thing);
		string text2 = QuestGenUtility.HardcodedSignalWithQuestID(text + ".Destroyed");
		string text3 = QuestGenUtility.HardcodedSignalWithQuestID(text + ".Hacked");
		string inSignalTookDamage = QuestGenUtility.HardcodedSignalWithQuestID(text + ".TookDamage");
		string inSignalLockedOut = QuestGenUtility.HardcodedSignalWithQuestID(text + ".LockedOut");
		signalsDealtWith.Add(text2);
		signalsDealtWith.Add(text3);
		QuestPart_Filter_Hacked questPart_Filter_Hacked = new QuestPart_Filter_Hacked();
		questPart_Filter_Hacked.inSignal = text2;
		questPart_Filter_Hacked.outSignalElse = QuestGen.GenerateNewSignal("SendRaidStabilizerDestroyed");
		quest.AddPart(questPart_Filter_Hacked);
		float points = slate.Get("points", 0f);
		QuestPart_SleepingMechs part = new QuestPart_SleepingMechs
		{
			inSignal = mapGeneratedSignal,
			inSignalTookDamage = inSignalTookDamage,
			inSignalLockedOut = inSignalLockedOut,
			defendThing = thing,
			mapParent = site,
			points = points
		};
		quest.AddPart(part);
		quest.Message("MessageStabilizerDeactivated".Translate(), MessageTypeDefOf.PositiveEvent, getLookTargetsFromSignal: true, null, null, text3);
		quest.RandomRaid(site, FloatRange.One, Faction.OfMechanoids, questPart_Filter_Hacked.outSignalElse, PawnsArrivalModeDefOf.RandomDrop, RaidStrategyDefOf.ImmediateAttack, "GravcoreMechRelayRaidLetterLabel".Translate(), "GravcoreMechRelayRaidLetterText".Translate(), useCurrentThreatPoints: true);
		stabilizers.Add(thing);
	}
}
