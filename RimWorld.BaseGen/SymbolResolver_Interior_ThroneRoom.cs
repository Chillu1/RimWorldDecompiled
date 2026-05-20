using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Interior_ThroneRoom : SymbolResolver
{
	private static List<Pair<IntVec3, Rot4>> tmpCells = new List<Pair<IntVec3, Rot4>>();

	public override void Resolve(ResolveParams rp)
	{
		Rot4 dir;
		IntVec3 throneCell = GetThroneCell(rp.rect, out dir);
		if (GetPossibleDrapeCells(throneCell, rp.rect).TryRandomElement(out var result))
		{
			ResolveParams resolveParams = rp;
			resolveParams.singleThingDef = ThingDefOf.Drape;
			resolveParams.rect = CellRect.SingleCell(result.First);
			resolveParams.thingRot = result.Second;
			BaseGen.symbolStack.Push("thing", resolveParams);
		}
		foreach (IntVec3 corner in rp.rect.Corners)
		{
			if (!(corner == throneCell) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(corner, BaseGen.globalSettings.map))
			{
				ResolveParams resolveParams2 = rp;
				resolveParams2.singleThingDef = ThingDefOf.Brazier;
				resolveParams2.rect = CellRect.SingleCell(corner);
				resolveParams2.postThingSpawn = delegate(Thing x)
				{
					x.TryGetComp<CompRefuelable>().Refuel(9999f);
				};
				BaseGen.symbolStack.Push("thing", resolveParams2);
			}
		}
		ResolveParams resolveParams3 = rp;
		resolveParams3.singleThingDef = ThingDefOf.Throne;
		resolveParams3.thingRot = dir;
		resolveParams3.rect = CellRect.SingleCell(throneCell);
		BaseGen.symbolStack.Push("thing", resolveParams3);
	}

	private IntVec3 GetThroneCell(CellRect rect, out Rot4 dir)
	{
		tmpCells.Clear();
		tmpCells.Add(new Pair<IntVec3, Rot4>(new IntVec3(rect.CenterCell.x, 0, rect.maxZ), Rot4.South));
		tmpCells.Add(new Pair<IntVec3, Rot4>(new IntVec3(rect.CenterCell.x, 0, rect.minZ), Rot4.North));
		tmpCells.Add(new Pair<IntVec3, Rot4>(new IntVec3(rect.minX, 0, rect.CenterCell.z), Rot4.East));
		tmpCells.Add(new Pair<IntVec3, Rot4>(new IntVec3(rect.maxX, 0, rect.CenterCell.z), Rot4.West));
		if (!tmpCells.Where((Pair<IntVec3, Rot4> x) => !BaseGenUtility.AnyDoorAdjacentCardinalTo(x.First, BaseGen.globalSettings.map)).TryRandomElement(out var result))
		{
			tmpCells.TryRandomElement(out result);
		}
		dir = result.Second;
		return result.First;
	}

	private IEnumerable<Pair<IntVec3, Rot4>> GetPossibleDrapeCells(IntVec3 throneCell, CellRect rect)
	{
		for (int d = 0; d < 4; d++)
		{
			foreach (IntVec3 edgeCell in rect.GetEdgeCells(new Rot4(d)))
			{
				bool flag = true;
				foreach (IntVec3 item in GenAdj.OccupiedRect(edgeCell, new Rot4(d), ThingDefOf.Drape.size))
				{
					if (item == throneCell || rect.IsCorner(item) || BaseGenUtility.AnyDoorAdjacentCardinalTo(item, BaseGen.globalSettings.map))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					yield return new Pair<IntVec3, Rot4>(edgeCell, new Rot4(d));
				}
			}
		}
	}
}
