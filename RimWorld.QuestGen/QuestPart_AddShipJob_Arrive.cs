using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_AddShipJob_Arrive : QuestPart_AddShipJob
{
	public IntVec3 cell;

	public MapParent mapParent;

	public Pawn mapOfPawn;

	public Faction factionForArrival;

	public override bool QuestPartReserves(Faction faction)
	{
		if (factionForArrival != null)
		{
			return faction == factionForArrival;
		}
		return false;
	}

	public override void Notify_FactionRemoved(Faction faction)
	{
		if (factionForArrival == faction)
		{
			factionForArrival = null;
		}
	}

	public override ShipJob GetShipJob()
	{
		if (mapOfPawn == null && (mapParent == null || !mapParent.HasMap || !quest.IsParentSuitableForQuest(mapParent)))
		{
			mapParent = quest.TryFindNewSuitableMapParentForRetarget();
			cell = DropCellFinder.TryFindSafeLandingSpotCloseToColony(mapParent.Map, ThingDefOf.Shuttle.Size + new IntVec2(2, 2));
		}
		ShipJob_Arrive obj = (ShipJob_Arrive)ShipJobMaker.MakeShipJob(shipJobDef);
		obj.cell = cell;
		obj.mapParent = mapParent;
		obj.mapOfPawn = mapOfPawn;
		obj.factionForArrival = factionForArrival;
		return obj;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref cell, "cell");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_References.Look(ref mapOfPawn, "mapOfPawn");
		Scribe_References.Look(ref factionForArrival, "factionForArrival");
	}
}
