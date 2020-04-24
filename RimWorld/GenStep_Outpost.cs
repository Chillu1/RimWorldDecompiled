using RimWorld.BaseGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class GenStep_Outpost : GenStep
	{
		public int size = 16;

		public FloatRange defaultPawnGroupPointsRange = SymbolResolver_Settlement.DefaultPawnsPoints;

		private static List<CellRect> possibleRects = new List<CellRect>();

		public override int SeedPart => 398638181;

		public override void Generate(Map map, GenStepParams parms)
		{
			if (!MapGenerator.TryGetVar("RectOfInterest", out CellRect var))
			{
				var = CellRect.SingleCell(map.Center);
			}
			if (!MapGenerator.TryGetVar("UsedRects", out List<CellRect> var2))
			{
				var2 = new List<CellRect>();
				MapGenerator.SetVar("UsedRects", var2);
			}
			Faction faction = (map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction();
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = GetOutpostRect(var, var2, map);
			resolveParams.faction = faction;
			resolveParams.edgeDefenseWidth = 2;
			resolveParams.edgeDefenseTurretsCount = Rand.RangeInclusive(0, 1);
			resolveParams.edgeDefenseMortarsCount = 0;
			if (parms.sitePart != null)
			{
				resolveParams.settlementPawnGroupPoints = parms.sitePart.parms.threatPoints;
				resolveParams.settlementPawnGroupSeed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms);
			}
			else
			{
				resolveParams.settlementPawnGroupPoints = defaultPawnGroupPointsRange.RandomInRange;
			}
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.BaseGen.globalSettings.minBuildings = 1;
			RimWorld.BaseGen.BaseGen.globalSettings.minBarracks = 1;
			RimWorld.BaseGen.BaseGen.symbolStack.Push("settlement", resolveParams);
			RimWorld.BaseGen.BaseGen.Generate();
			var2.Add(resolveParams.rect);
		}

		private CellRect GetOutpostRect(CellRect rectToDefend, List<CellRect> usedRects, Map map)
		{
			possibleRects.Add(new CellRect(rectToDefend.minX - 1 - size, rectToDefend.CenterCell.z - size / 2, size, size));
			possibleRects.Add(new CellRect(rectToDefend.maxX + 1, rectToDefend.CenterCell.z - size / 2, size, size));
			possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.minZ - 1 - size, size, size));
			possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.maxZ + 1, size, size));
			CellRect mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
			possibleRects.RemoveAll((CellRect x) => !x.FullyContainedWithin(mapRect));
			if (possibleRects.Any())
			{
				IEnumerable<CellRect> source = possibleRects.Where((CellRect x) => !usedRects.Any((CellRect y) => x.Overlaps(y)));
				if (!source.Any())
				{
					possibleRects.Add(new CellRect(rectToDefend.minX - 1 - size * 2, rectToDefend.CenterCell.z - size / 2, size, size));
					possibleRects.Add(new CellRect(rectToDefend.maxX + 1 + size, rectToDefend.CenterCell.z - size / 2, size, size));
					possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.minZ - 1 - size * 2, size, size));
					possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.maxZ + 1 + size, size, size));
				}
				if (source.Any())
				{
					return source.RandomElement();
				}
				return possibleRects.RandomElement();
			}
			return rectToDefend;
		}
	}
}
