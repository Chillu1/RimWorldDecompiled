using System.Collections.Generic;
using System.Linq;
using RimWorld.BaseGen;
using Verse;

namespace RimWorld;

public class GenStep_Settlement : GenStep_Scatterer
{
	private static readonly IntRange SettlementSizeRange = new IntRange(34, 38);

	public const string RectVarName = "SettlementRect";

	public bool generatePawns = true;

	public MapGenUtility.PostProcessSettlementParams postProcessSettlementParams;

	public int requiredGravcoreRooms;

	public ThingSetMakerDef lootThingSetMaker;

	public float? lootMarketValue;

	public Faction overrideFaction;

	private static readonly List<IntVec3> tmpCandidates = new List<IntVec3>();

	private bool WillPostProcess => postProcessSettlementParams != null;

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
		CellRect cellRect = new CellRect(c.x - min / 2, c.z - min / 2, min, min);
		CellRect within = map.BoundsRect(12);
		if (!cellRect.FullyContainedWithin(within))
		{
			return false;
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
	{
		int randomInRange = SettlementSizeRange.RandomInRange;
		int randomInRange2 = SettlementSizeRange.RandomInRange;
		CellRect cellRect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
		Faction faction = ((overrideFaction != null) ? overrideFaction : ((map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction()));
		cellRect.ClipInsideMap(map);
		ResolveParams resolveParams = new ResolveParams
		{
			sitePart = parms.sitePart,
			rect = cellRect,
			faction = faction,
			settlementDontGeneratePawns = !generatePawns,
			thingSetMakerDef = lootThingSetMaker,
			lootMarketValue = lootMarketValue
		};
		if (postProcessSettlementParams != null)
		{
			postProcessSettlementParams.faction = faction;
		}
		MapGenerator.SetVar("SettlementRect", cellRect);
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.globalSettings.minBuildings = 1 + requiredGravcoreRooms;
		RimWorld.BaseGen.BaseGen.globalSettings.minBarracks = 1;
		RimWorld.BaseGen.BaseGen.globalSettings.requiredGravcoreRooms = requiredGravcoreRooms;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("settlement", resolveParams);
		if (faction != null && faction == Faction.OfEmpire)
		{
			RimWorld.BaseGen.BaseGen.globalSettings.minThroneRooms = 1;
			RimWorld.BaseGen.BaseGen.globalSettings.minLandingPads = 1;
		}
		List<Building> previous = null;
		if (WillPostProcess)
		{
			previous = new List<Building>(map.listerThings.GetThingsOfType<Building>());
		}
		RimWorld.BaseGen.BaseGen.Generate();
		if (faction != null && faction == Faction.OfEmpire && RimWorld.BaseGen.BaseGen.globalSettings.landingPadsGenerated == 0)
		{
			GenerateLandingPadNearby(resolveParams.rect, map, faction, out var _);
		}
		if (WillPostProcess)
		{
			List<Building> placed = (from b in map.listerThings.GetThingsOfType<Building>()
				where !previous.Contains(b)
				select b).ToList();
			previous.Clear();
			MapGenUtility.PostProcessSettlement(map, placed, postProcessSettlementParams);
		}
	}

	public static void GenerateLandingPadNearby(CellRect rect, Map map, Faction faction, out CellRect usedRect)
	{
		ResolveParams resolveParams = default(ResolveParams);
		MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var usedRects);
		tmpCandidates.Clear();
		int size = 9;
		tmpCandidates.Add(new IntVec3(rect.maxX + 1, 0, rect.CenterCell.z));
		tmpCandidates.Add(new IntVec3(rect.minX - size, 0, rect.CenterCell.z));
		tmpCandidates.Add(new IntVec3(rect.CenterCell.x, 0, rect.maxZ + 1));
		tmpCandidates.Add(new IntVec3(rect.CenterCell.x, 0, rect.minZ - size));
		if (!tmpCandidates.Where(delegate(IntVec3 x)
		{
			CellRect r = new CellRect(x.x, x.z, size, size);
			return r.InBounds(map) && (usedRects == null || !usedRects.Any((CellRect y) => y.Overlaps(r)));
		}).TryRandomElement(out var result))
		{
			usedRect = CellRect.Empty;
			return;
		}
		resolveParams.rect = new CellRect(result.x, result.z, size, size);
		resolveParams.faction = faction;
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("landingPad", resolveParams);
		RimWorld.BaseGen.BaseGen.Generate();
		usedRect = resolveParams.rect;
	}
}
