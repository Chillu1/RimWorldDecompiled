using System.Linq;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Hives : SymbolResolver
{
	private static readonly IntRange DefaultHivesCountRange = new IntRange(1, 3);

	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (Faction.OfInsects == null)
		{
			return false;
		}
		if (!TryFindFirstHivePos(rp.rect, out var _))
		{
			return false;
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		if (TryFindFirstHivePos(rp.rect, out var pos))
		{
			int count = rp.hivesCount ?? DefaultHivesCountRange.RandomInRange;
			HiveUtility.SpawnHives(pos, BaseGen.globalSettings.map, count, 5f, WipeMode.VanishOrMoveAside, spawnInsectsImmediately: false, canSpawnHives: true, canSpawnInsects: true, rp.disableHives == true);
		}
	}

	private bool TryFindFirstHivePos(CellRect rect, out IntVec3 pos)
	{
		Map map = BaseGen.globalSettings.map;
		return rect.Cells.Where((IntVec3 mc) => mc.Standable(map)).TryRandomElement(out pos);
	}
}
