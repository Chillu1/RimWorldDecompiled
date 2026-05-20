using System.Collections.Generic;
using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class GenStep_ItemStash : GenStep_Scatterer
{
	public ThingSetMakerDef thingSetMakerDef;

	private const int Size = 7;

	public override int SeedPart => 913432591;

	protected override bool CanScatterAt(IntVec3 c, Map map)
	{
		if (!base.CanScatterAt(c, map))
		{
			return false;
		}
		if (!c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy))
		{
			return false;
		}
		CellRect rect = CellRect.CenteredOn(c, 7, 7);
		if (MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var var) && var.Any((CellRect x) => x.Overlaps(rect)))
		{
			return false;
		}
		foreach (IntVec3 item in rect)
		{
			if (!item.InBounds(map) || item.GetEdifice(map) != null)
			{
				return false;
			}
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		CellRect cellRect = CellRect.CenteredOn(loc, 7, 7).ClipInsideMap(map);
		List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		ResolveParams resolveParams = new ResolveParams
		{
			rect = cellRect,
			faction = map.ParentFaction
		};
		if (parms.sitePart != null && parms.sitePart.things != null && parms.sitePart.things.Any)
		{
			resolveParams.stockpileConcreteContents = parms.sitePart.things;
		}
		else
		{
			ItemStashContentsComp component = map.Parent.GetComponent<ItemStashContentsComp>();
			if (component != null && component.contents.Any)
			{
				resolveParams.stockpileConcreteContents = component.contents;
			}
			else
			{
				resolveParams.thingSetMakerDef = thingSetMakerDef ?? ThingSetMakerDefOf.MapGen_DefaultStockpile;
			}
		}
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("storage", resolveParams);
		RimWorld.BaseGen.BaseGen.Generate();
		MapGenerator.SetVar("RectOfInterest", cellRect);
		orGenerateVar.Add(cellRect);
	}
}
