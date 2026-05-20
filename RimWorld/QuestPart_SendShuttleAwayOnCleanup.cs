using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_SendShuttleAwayOnCleanup : QuestPart
{
	public Thing shuttle;

	public bool dropEverything;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			yield return shuttle;
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (shuttle != null)
		{
			SendShuttleAwayQuestPartUtility.SendAway(shuttle, dropEverything);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref shuttle, "shuttle");
		Scribe_Values.Look(ref dropEverything, "dropEverything", defaultValue: false);
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		if (shuttle != null)
		{
			shuttle.TryGetComp<CompShuttle>().requiredPawns.Replace(replace, with);
		}
	}
}
