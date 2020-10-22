using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Hives : SymbolResolver
	{
		private static readonly IntRange DefaultHivesCountRange = new IntRange(1, 3);

		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
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
			if (!TryFindFirstHivePos(rp.rect, out var pos))
			{
				return;
			}
			int num = rp.hivesCount ?? DefaultHivesCountRange.RandomInRange;
			Hive hive = (Hive)ThingMaker.MakeThing(ThingDefOf.Hive);
			hive.SetFaction(Faction.OfInsects);
			if (rp.disableHives.HasValue && rp.disableHives.Value)
			{
				hive.CompDormant.ToSleep();
			}
			hive = (Hive)GenSpawn.Spawn(hive, pos, BaseGen.globalSettings.map);
			for (int i = 0; i < num - 1; i++)
			{
				if (hive.GetComp<CompSpawnerHives>().TrySpawnChildHive(ignoreRoofedRequirement: true, out var newHive))
				{
					hive = newHive;
				}
			}
		}

		private bool TryFindFirstHivePos(CellRect rect, out IntVec3 pos)
		{
			Map map = BaseGen.globalSettings.map;
			return rect.Cells.Where((IntVec3 mc) => mc.Standable(map)).TryRandomElement(out pos);
		}
	}
}
