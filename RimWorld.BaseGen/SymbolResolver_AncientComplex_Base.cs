using Verse;

namespace RimWorld.BaseGen;

public abstract class SymbolResolver_AncientComplex_Base : SymbolResolver
{
	protected abstract LayoutDef DefaultLayoutDef { get; }

	public void ResolveComplex(ResolveParams rp)
	{
		if (rp.ancientLayoutStructureSketch == null)
		{
			StructureGenParams parms = new StructureGenParams
			{
				size = new IntVec2(rp.rect.Width, rp.rect.Height)
			};
			rp.ancientLayoutStructureSketch = DefaultLayoutDef.Worker.GenerateStructureSketch(parms);
		}
		ResolveParams resolveParams = rp;
		resolveParams.ancientLayoutStructureSketch = rp.ancientLayoutStructureSketch;
		BaseGen.symbolStack.Push("ancientComplexSketch", resolveParams);
		ResolveParams resolveParams2 = rp;
		resolveParams2.floorDef = TerrainDefOf.AncientConcrete;
		resolveParams2.allowBridgeOnAnyImpassableTerrain = true;
		resolveParams2.floorOnlyIfTerrainSupports = false;
		foreach (LayoutRoom room in rp.ancientLayoutStructureSketch.structureLayout.Rooms)
		{
			foreach (CellRect rect in room.rects)
			{
				resolveParams2.rect = rect.MovedBy(rp.rect.Min);
				BaseGen.symbolStack.Push("floor", resolveParams2);
				BaseGen.symbolStack.Push("clear", resolveParams2);
			}
		}
	}
}
