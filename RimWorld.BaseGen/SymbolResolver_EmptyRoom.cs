using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_EmptyRoom : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		ThingDef thingDef = rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction);
		TerrainDef floorDef = rp.floorDef ?? BaseGenUtility.CorrespondingTerrainDef(thingDef, beautiful: false, rp.faction);
		MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects").Add(rp.rect);
		if (!rp.noRoof.HasValue || !rp.noRoof.Value)
		{
			BaseGen.symbolStack.Push("roof", rp);
		}
		ResolveParams resolveParams = rp;
		resolveParams.wallStuff = thingDef;
		BaseGen.symbolStack.Push("edgeWalls", resolveParams);
		ResolveParams resolveParams2 = rp;
		resolveParams2.floorDef = floorDef;
		BaseGen.symbolStack.Push("floor", resolveParams2);
		BaseGen.symbolStack.Push("clear", rp);
		if (rp.addRoomCenterToRootsToUnfog.HasValue && rp.addRoomCenterToRootsToUnfog.Value && Current.ProgramState == ProgramState.MapInitializing)
		{
			MapGenerator.rootsToUnfog.Add(rp.rect.CenterCell);
		}
	}
}
