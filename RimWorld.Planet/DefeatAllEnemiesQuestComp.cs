using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class DefeatAllEnemiesQuestComp : WorldObjectComp, IThingHolder
{
	private bool active;

	public Faction requestingFaction;

	public int relationsImprovement;

	public ThingOwner rewards;

	private static readonly List<Thing> tmpRewards = new List<Thing>();

	public bool Active => active;

	public DefeatAllEnemiesQuestComp()
	{
		rewards = new ThingOwner<Thing>(this);
	}

	public void StartQuest(Faction requestingFaction, int relationsImprovement, List<Thing> rewards)
	{
		StopQuest();
		active = true;
		this.requestingFaction = requestingFaction;
		this.relationsImprovement = relationsImprovement;
		this.rewards.ClearAndDestroyContents();
		this.rewards.TryAddRangeOrTransfer(rewards);
	}

	public void StopQuest()
	{
		active = false;
		requestingFaction = null;
		rewards.ClearAndDestroyContents();
	}

	public override void CompTickInterval(int delta)
	{
		base.CompTickInterval(delta);
		if (active && parent is MapParent mapParent)
		{
			CheckAllEnemiesDefeated(mapParent);
		}
	}

	private void CheckAllEnemiesDefeated(MapParent mapParent)
	{
		if (mapParent.HasMap && !GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map, countDormantPawnsAsHostile: true))
		{
			GiveRewardsAndSendLetter();
			StopQuest();
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref active, "active", defaultValue: false);
		Scribe_Values.Look(ref relationsImprovement, "relationsImprovement", 0);
		Scribe_References.Look(ref requestingFaction, "requestingFaction");
		Scribe_Deep.Look(ref rewards, "rewards", this);
	}

	private void GiveRewardsAndSendLetter()
	{
		Map map = Find.AnyPlayerHomeMap ?? ((MapParent)parent).Map;
		tmpRewards.AddRange(rewards);
		rewards.Clear();
		IntVec3 intVec = DropCellFinder.TradeDropSpot(map);
		DropPodUtility.DropThingsNear(intVec, map, tmpRewards, 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: false);
		tmpRewards.Clear();
		FactionRelationKind playerRelationKind = requestingFaction.PlayerRelationKind;
		TaggedString text = "LetterDefeatAllEnemiesQuestCompleted".Translate(requestingFaction.Name, relationsImprovement.ToString());
		Faction.OfPlayer.TryAffectGoodwillWith(requestingFaction, relationsImprovement, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.QuestGoodwillReward);
		requestingFaction.TryAppendRelationKindChangedInfo(ref text, playerRelationKind, requestingFaction.PlayerRelationKind);
		Find.LetterStack.ReceiveLetter("LetterLabelDefeatAllEnemiesQuestCompleted".Translate(), text, LetterDefOf.PositiveEvent, new GlobalTargetInfo(intVec, map), requestingFaction);
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return rewards;
	}

	public override void PostDestroy()
	{
		base.PostDestroy();
		rewards.ClearAndDestroyContents();
	}

	public override string CompInspectStringExtra()
	{
		if (active)
		{
			string text = GenThing.ThingsToCommaList(rewards, useAnd: true, aggregate: true, 5).CapitalizeFirst();
			return "QuestTargetDestroyInspectString".Translate(requestingFaction.Name, text, GenThing.GetMarketValue(rewards).ToStringMoney()).CapitalizeFirst();
		}
		return null;
	}
}
