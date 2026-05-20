using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class VoluntarilyJoinableLordsStarter : IExposable
{
	private Map map;

	private int lastLordStartTick = -999999;

	private bool startRandomGatheringASAP;

	private const int CheckStartGatheringIntervalTicks = 5000;

	private const float StartGatheringMTBDays = 40f;

	private static List<GatheringDef> tmpGatherings = new List<GatheringDef>();

	public VoluntarilyJoinableLordsStarter(Map map)
	{
		this.map = map;
	}

	public bool TryStartMarriageCeremony(Pawn firstFiance, Pawn secondFiance)
	{
		if (!GatheringDefOf.MarriageCeremony.CanExecute(firstFiance.Map, firstFiance, ignoreGameConditions: true))
		{
			return false;
		}
		GatheringDefOf.MarriageCeremony.Worker.TryExecute(firstFiance.Map, firstFiance);
		lastLordStartTick = Find.TickManager.TicksGame;
		return true;
	}

	public bool TryStartRandomGathering(bool forceStart = false)
	{
		tmpGatherings.Clear();
		foreach (GatheringDef item in DefDatabase<GatheringDef>.AllDefsListForReading)
		{
			if (item.IsRandomSelectable && item.CanExecute(map, null, forceStart))
			{
				tmpGatherings.Add(item);
			}
		}
		if (!tmpGatherings.TryRandomElementByWeight((GatheringDef def) => def.randomSelectionWeight, out var result))
		{
			return false;
		}
		return TryStartGathering(result);
	}

	public bool TryStartGathering(GatheringDef gatheringDef)
	{
		if (!gatheringDef.Worker.TryExecute(map))
		{
			return false;
		}
		lastLordStartTick = Find.TickManager.TicksGame;
		startRandomGatheringASAP = false;
		return true;
	}

	public void VoluntarilyJoinableLordsStarterTick()
	{
		Tick_TryStartRandomGathering();
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref lastLordStartTick, "lastLordStartTick", 0);
		Scribe_Values.Look(ref startRandomGatheringASAP, "startPartyASAP", defaultValue: false);
	}

	private void Tick_TryStartRandomGathering()
	{
		if (map.IsPlayerHome && Find.TickManager.TicksGame % 5000 == 0)
		{
			if (Rand.MTBEventOccurs(40f, 60000f, 5000f))
			{
				startRandomGatheringASAP = true;
			}
			if (startRandomGatheringASAP && Find.TickManager.TicksGame - lastLordStartTick >= 600000)
			{
				TryStartRandomGathering();
			}
		}
	}
}
