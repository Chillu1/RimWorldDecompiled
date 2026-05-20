using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_PawnsAvailable : QuestPartActivable
{
	public ThingDef race;

	public int requiredCount;

	public MapParent mapParent;

	public string inSignalDecrement;

	public string outSignalPawnsNotAvailable;

	private const int CheckInterval = 500;

	public override void QuestPartTick()
	{
		if (mapParent == null || mapParent.Destroyed)
		{
			mapParent = quest.TryFindNewSuitableMapParentForRetarget();
		}
		if (requiredCount <= 0 || Find.TickManager.TicksAbs % 500 != 0)
		{
			return;
		}
		int num = 0;
		IReadOnlyList<Pawn> allPawnsSpawned = mapParent.Map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			if (allPawnsSpawned[i].def == race && allPawnsSpawned[i].Faction == null)
			{
				num++;
			}
		}
		if (num < requiredCount)
		{
			Find.SignalManager.SendSignal(new Signal(outSignalPawnsNotAvailable));
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignalDecrement)
		{
			requiredCount--;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref race, "race");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref requiredCount, "requiredCount", 0);
		Scribe_Values.Look(ref inSignalDecrement, "inSignalChangeCount");
		Scribe_Values.Look(ref outSignalPawnsNotAvailable, "outSignalPawnsNotAvailable");
	}
}
