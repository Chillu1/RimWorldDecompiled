using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_LandingPad : SymbolResolver
	{
		private static readonly IntRange ShuttleLeaveAfterTicksRange = new IntRange(300, 3600);

		public override void Resolve(ResolveParams rp)
		{
			TerrainDef floorDef = rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction);
			ResolveParams resolveParams = rp;
			resolveParams.singleThingDef = ThingDefOf.Shuttle;
			resolveParams.rect = CellRect.SingleCell(rp.rect.CenterCell);
			resolveParams.postThingSpawn = delegate(Thing x)
			{
				x.TryGetComp<CompShuttle>().leaveAfterTicks = ShuttleLeaveAfterTicksRange.RandomInRange;
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
}
