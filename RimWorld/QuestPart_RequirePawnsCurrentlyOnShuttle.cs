using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_RequirePawnsCurrentlyOnShuttle : QuestPart
{
	public string inSignal;

	public Thing shuttle;

	public int requiredColonistCount;

	public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
	{
		CompShuttle compShuttle = shuttle.TryGetComp<CompShuttle>();
		if (compShuttle.requiredPawns.Contains(pawn))
		{
			compShuttle.requiredPawns.Remove(pawn);
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (!(signal.tag == inSignal) || shuttle == null)
		{
			return;
		}
		CompShuttle compShuttle = shuttle.TryGetComp<CompShuttle>();
		compShuttle.requiredColonistCount = requiredColonistCount;
		compShuttle.requiredItems.Clear();
		compShuttle.requiredPawns.Clear();
		foreach (Thing item2 in (IEnumerable<Thing>)shuttle.TryGetComp<CompTransporter>().innerContainer)
		{
			if (item2 is Pawn item && !compShuttle.requiredPawns.Contains(item))
			{
				compShuttle.requiredPawns.Add(item);
			}
		}
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		shuttle.TryGetComp<CompShuttle>().requiredPawns.Replace(replace, with);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignl");
		Scribe_Values.Look(ref requiredColonistCount, "requiredColonistCount", 0);
		Scribe_References.Look(ref shuttle, "shuttle");
	}
}
