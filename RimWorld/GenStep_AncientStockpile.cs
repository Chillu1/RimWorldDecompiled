using Verse;

namespace RimWorld;

public class GenStep_AncientStockpile : GenStep
{
	private static readonly IntRange SizeRange = new IntRange(40, 50);

	public override int SeedPart => 928734;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModsConfig.OdysseyActive)
		{
			CellRect cellRect = map.Center.RectAbout(new IntVec2(SizeRange.RandomInRange, SizeRange.RandomInRange));
			StructureGenParams parms2 = new StructureGenParams
			{
				size = cellRect.Size
			};
			LayoutWorker obj = parms.layout?.Worker ?? LayoutDefOf.AncientStockpile.Worker;
			LayoutStructureSketch layoutStructureSketch = obj.GenerateStructureSketch(parms2);
			map.layoutStructureSketches.Add(layoutStructureSketch);
			float? threatPoints = null;
			if (parms.sitePart != null)
			{
				threatPoints = parms.sitePart.parms.points;
			}
			obj.Spawn(layoutStructureSketch, map, cellRect.Min, threatPoints);
		}
	}
}
