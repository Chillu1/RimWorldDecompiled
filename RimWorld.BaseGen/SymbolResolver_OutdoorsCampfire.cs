using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_OutdoorsCampfire : SymbolResolver
{
	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (!TryFindSpawnCell(rp.rect, out var _))
		{
			return false;
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		if (TryFindSpawnCell(rp.rect, out var result))
		{
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Campfire);
			thing.SetFaction(rp.faction);
			GenSpawn.Spawn(thing, result, BaseGen.globalSettings.map);
		}
	}

	private bool TryFindSpawnCell(CellRect rect, out IntVec3 result)
	{
		Map map = BaseGen.globalSettings.map;
		return CellFinder.TryFindRandomCellInsideWith(rect, (IntVec3 c) => c.Standable(map) && !c.Roofed(map) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(c, map) && c.GetFirstItem(map) == null && !GenSpawn.WouldWipeAnythingWith(c, Rot4.North, ThingDefOf.Campfire, map, (Thing x) => x.def.category == ThingCategory.Building), out result);
	}
}
