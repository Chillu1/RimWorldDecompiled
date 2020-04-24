using RimWorld.BaseGen;
using Verse;

namespace RimWorld
{
	public class GenStep_Settlement : GenStep_Scatterer
	{
		private static readonly IntRange SettlementSizeRange = new IntRange(34, 38);

		public override int SeedPart => 1806208471;

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))
			{
				return false;
			}
			if (!c.Standable(map))
			{
				return false;
			}
			if (c.Roofed(map))
			{
				return false;
			}
			if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors)))
			{
				return false;
			}
			int min = SettlementSizeRange.min;
			if (!new CellRect(c.x - min / 2, c.z - min / 2, min, min).FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z)))
			{
				return false;
			}
			return true;
		}

		protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
		{
			int randomInRange = SettlementSizeRange.RandomInRange;
			int randomInRange2 = SettlementSizeRange.RandomInRange;
			CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
			Faction faction = (map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction();
			rect.ClipInsideMap(map);
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = rect;
			resolveParams.faction = faction;
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.BaseGen.globalSettings.minBuildings = 1;
			RimWorld.BaseGen.BaseGen.globalSettings.minBarracks = 1;
			RimWorld.BaseGen.BaseGen.symbolStack.Push("settlement", resolveParams);
			RimWorld.BaseGen.BaseGen.Generate();
		}
	}
}
