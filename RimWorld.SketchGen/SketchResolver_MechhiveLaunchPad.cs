using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_MechhiveLaunchPad : SketchResolver
{
	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		if (parms.sketch != null)
		{
			return parms.rect.HasValue;
		}
		return false;
	}

	protected override void ResolveInt(SketchResolveParams parms)
	{
		if (!ModLister.CheckOdyssey("Mechhive guard outpost"))
		{
			return;
		}
		Sketch sketch = new Sketch();
		CellRect value = parms.rect.Value;
		foreach (IntVec3 cell in value.Cells)
		{
			sketch.AddTerrain(TerrainDefOf.AncientTile, cell);
		}
		sketch.AddThing(Rand.Bool ? ThingDefOf.MechanoidDropPod : ThingDefOf.Filth_BlastMark, new IntVec3(1, 0, 1), Rot4.North);
		sketch.AddThing(Rand.Bool ? ThingDefOf.MechanoidDropPod : ThingDefOf.Filth_BlastMark, new IntVec3(1, 0, value.maxZ - 1), Rot4.North);
		sketch.AddThing(Rand.Bool ? ThingDefOf.MechanoidDropPod : ThingDefOf.Filth_BlastMark, new IntVec3(value.maxX - 1, 0, 1), Rot4.North);
		sketch.AddThing(Rand.Bool ? ThingDefOf.MechanoidDropPod : ThingDefOf.Filth_BlastMark, new IntVec3(value.maxX - 1, 0, value.maxZ - 1), Rot4.North);
		parms.sketch.Merge(sketch);
	}
}
