using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Gravcore_AbandonedSettlement : QuestNode_Root_Gravcore
{
	private static readonly IntRange TicksBetweenRaidsRange = new IntRange(30000, 60000);

	private static readonly IntRange RaidsCountRange = new IntRange(3, 4);

	private const int SettlementPoints = 3000;

	private const string GravcoreAlias = "gravcore";

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Quest quest = QuestGen.quest;
		if (!TryFindSiteTile(out var tile) || !TryGetFactions(out var settlementFaction, out var hostileFaction))
		{
			return;
		}
		slate.Set("settlementFaction", settlementFaction);
		slate.Set("hostileFaction", hostileFaction);
		float points = slate.Get("points", 0f);
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
		string inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID("gravcore.Unfogged");
		Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
		{
			new SitePartDefWithParams(SitePartDefOf.AbandonedSettlement, new SitePartParams
			{
				points = 3000f,
				threatPoints = slate.Get("points", 0f)
			})
		}, tile, settlementFaction, hiddenSitePartsPossible: false, null, WorldObjectDefOf.ClaimableSite);
		site.desiredThreatPoints = 100f;
		site.doorsAlwaysOpenForPlayerPawns = true;
		slate.Set("site", site);
		quest.SpawnWorldObject(site);
		slate.Set("gravcore", site.parts[0].things.FirstOrDefault((Thing t) => t.def == ThingDefOf.Gravcore));
		quest.Letter(LetterDefOf.NeutralEvent, inSignal, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[letterTextMapGenerated]", null, "[letterLabelMapGenerated]");
		if (Find.Storyteller.difficulty.allowViolentQuests)
		{
			quest.Delay(Rand.RangeInclusive(60, 120), delegate
			{
				int num = 0;
				int raidsCount = RaidsCountRange.RandomInRange;
				for (int i = 0; i < raidsCount; i++)
				{
					int localIndex = i;
					int num2 = ((i != 0) ? TicksBetweenRaidsRange.RandomInRange : 0);
					quest.Delay(num + num2, delegate
					{
						string text = "GravcoreAbandonedSettlementRaid".Translate(hostileFaction.Named("FACTION")).Resolve();
						if (localIndex < raidsCount - 1)
						{
							text = text + "\n\n" + "GravcoreAbandonedSettlementRaid_Reinforcements".Translate().Resolve();
						}
						quest.RandomRaid(site, new FloatRange(points), hostileFaction, null, raidStrategy: RaidStrategyDefOf.ImmediateAttack, arrivalMode: PawnsArrivalModeDefOf.EdgeWalkInGroups, customLetterLabel: "LetterLabelAmbushInExistingMap".Translate().CapitalizeFirst() + ": " + hostileFaction.Name, customLetterText: text);
						if (localIndex == raidsCount - 1)
						{
							quest.End(QuestEndOutcome.Success, sendLetter: false, playSound: false);
						}
					}).debugLabel = "Raid delay";
					num += num2;
				}
			}, inSignalEnable);
		}
		QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
		choice.rewards.Add(new Reward_DefinedThingDef(ThingDefOf.Gravcore));
		quest.RewardChoice().choices.Add(choice);
		quest.End(QuestEndOutcome.Unknown, 0, null, inSignal2);
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!base.TestRunInt(slate))
		{
			return false;
		}
		if (!TryGetFactions(out var _, out var _))
		{
			return false;
		}
		return true;
	}

	private bool TryGetFactions(out Faction settlementFaction, out Faction hostileFaction)
	{
		IEnumerable<Faction> source = Find.FactionManager.AllFactionsVisible.Where((Faction x) => x.def.humanlikeFaction && !x.IsPlayer && !x.temporary);
		if (!source.TryRandomElement(out var setFac))
		{
			hostileFaction = null;
			settlementFaction = null;
			return false;
		}
		settlementFaction = setFac;
		if (!source.Where((Faction x) => x.HostileTo(Faction.OfPlayer) && x != setFac).TryRandomElement(out hostileFaction))
		{
			hostileFaction = null;
			return false;
		}
		return true;
	}
}
