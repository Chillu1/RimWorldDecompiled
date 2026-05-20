using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_MechhiveProcessorArray : SketchResolver
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
		_ = parms.rect.Value;
		int num = parms.rect.Value.Width / (ThingDefOf.MechanoidProcessor.size.x + 1);
		int num2 = parms.rect.Value.Height / (ThingDefOf.MechanoidProcessor.size.z + 1);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				IntVec3 pos = new IntVec3(2, 0, 2) + new IntVec3(i * (ThingDefOf.MechanoidProcessor.size.x + 1), 0, j * (ThingDefOf.MechanoidProcessor.size.z + 1));
				sketch.AddThing(ThingDefOf.MechanoidProcessor, pos, Rot4.North);
			}
		}
		parms.sketch.Merge(sketch);
	}
}
