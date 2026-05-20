using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_SendShuttleAway : QuestPart
{
	public string inSignal;

	public Thing shuttle;

	public bool dropEverything;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			yield return shuttle;
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignal && shuttle != null)
		{
			SendShuttleAwayQuestPartUtility.SendAway(shuttle, dropEverything);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref shuttle, "shuttle");
		Scribe_Values.Look(ref dropEverything, "dropEverything", defaultValue: false);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		if (Find.AnyPlayerHomeMap != null)
		{
			Map randomPlayerHomeMap = Find.RandomPlayerHomeMap;
			IntVec3 center = DropCellFinder.RandomDropSpot(randomPlayerHomeMap);
			shuttle = ThingMaker.MakeThing(ThingDefOf.Shuttle);
			GenPlace.TryPlaceThing(SkyfallerMaker.MakeSkyfaller(ThingDefOf.ShuttleIncoming, shuttle), center, randomPlayerHomeMap, ThingPlaceMode.Near);
		}
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		if (shuttle != null)
		{
			shuttle.TryGetComp<CompShuttle>().requiredPawns.Replace(replace, with);
		}
	}
}
