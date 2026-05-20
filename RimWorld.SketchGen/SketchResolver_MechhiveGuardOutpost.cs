using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_MechhiveGuardOutpost : SketchResolver
{
	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return parms.sketch != null;
	}

	protected override void ResolveInt(SketchResolveParams parms)
	{
		if (!ModLister.CheckOdyssey("Mechhive guard outpost"))
		{
			return;
		}
		Sketch sketch = new Sketch();
		CellRect cellRect = CellRect.CenteredOn(IntVec3.Zero, 5, 5);
		foreach (IntVec3 edgeCell in cellRect.EdgeCells)
		{
			if (edgeCell.x != 0 && edgeCell.z != 0)
			{
				sketch.AddThing(ThingDefOf.Barricade, edgeCell, Rot4.North, ThingDefOf.Steel);
			}
		}
		foreach (IntVec3 cell in cellRect.ContractedBy(1).Cells)
		{
			sketch.AddTerrain(TerrainDefOf.AncientTile, cell);
		}
		sketch.AddThing(ThingDefOf.Turret_AutoMiniTurret, cellRect.CenterCell, Rot4.North);
		parms.sketch.Merge(sketch);
	}
}
