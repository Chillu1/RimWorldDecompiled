using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_LandingPad : SymbolResolver
{
	private static readonly IntRange ShuttleLeaveAfterTicksRange = new IntRange(300, 3600);

	public override void Resolve(ResolveParams rp)
	{
		TerrainDef floorDef = rp.floorDef ?? Rand.Element(TerrainDefOf.PavedTile, TerrainDefOf.Concrete);
		ResolveParams resolveParams = rp;
		resolveParams.singleThingDef = ThingDefOf.Shuttle;
		resolveParams.rect = CellRect.SingleCell(rp.rect.CenterCell);
		resolveParams.postThingSpawn = delegate(Thing x)
		{
			TransportShip transportShip = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, null, x);
			ShipJob_WaitTime shipJob_WaitTime = (ShipJob_WaitTime)ShipJobMaker.MakeShipJob(ShipJobDefOf.WaitTime);
			shipJob_WaitTime.duration = ShuttleLeaveAfterTicksRange.RandomInRange;
			shipJob_WaitTime.showGizmos = false;
			transportShip.AddJob(shipJob_WaitTime);
			transportShip.AddJob(ShipJobDefOf.FlyAway);
			transportShip.Start();
		};
		BaseGen.symbolStack.Push("thing", resolveParams);
		foreach (IntVec3 corner in rp.rect.Corners)
		{
			ResolveParams resolveParams2 = rp;
			resolveParams2.singleThingDef = ThingDefOf.ShipLandingBeacon;
			resolveParams2.rect = CellRect.SingleCell(corner);
			BaseGen.symbolStack.Push("thing", resolveParams2);
		}
		ResolveParams resolveParams3 = rp;
		resolveParams3.floorDef = floorDef;
		BaseGen.symbolStack.Push("floor", resolveParams3);
		BaseGen.symbolStack.Push("clear", rp);
	}
}
