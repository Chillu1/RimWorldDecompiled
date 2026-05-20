using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class GenStep_SettlementPawnsLoot : GenStep
{
	public FactionDef factionDef;

	public bool generatePawns = true;

	public ThingSetMakerDef lootThingSetMaker;

	public FloatRange? lootMarketValue;

	public bool requiresRoof;

	public override int SeedPart => 4615611;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!MapGenerator.TryGetVar<CellRect>("SpawnRect", out var var))
		{
			Log.Error("GenStep_SettlementPawnsLoot tried to execute but no SpawnRect was found in the map generator. This CellRect must be set.");
			return;
		}
		Faction faction = GetFaction(map);
		if (generatePawns)
		{
			Lord lord = LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, var.CenterCell, 25000), map);
			CellRect rect = var;
			PawnGroupKindDef settlement = PawnGroupKindDefOf.Settlement;
			bool flag = requiresRoof;
			MapGenUtility.GeneratePawns(map, rect, faction, lord, settlement, null, null, null, null, flag);
		}
		FloatRange? floatRange = lootMarketValue;
		if (!floatRange.HasValue || !floatRange.GetValueOrDefault().IsZeros)
		{
			ThingSetMakerDef setMakerDef = lootThingSetMaker ?? faction.def.settlementLootMaker ?? ThingSetMakerDefOf.MapGen_AbandonedColonyStockpile;
			CellRect rect2 = var;
			FloatRange? marketValueRange = lootMarketValue;
			Faction faction2 = faction;
			bool flag = requiresRoof;
			MapGenUtility.GenerateLoot(map, rect2, setMakerDef, marketValueRange, null, faction2, flag);
		}
	}

	private Faction GetFaction(Map map)
	{
		if (factionDef != null)
		{
			return Find.FactionManager.FirstFactionOfDef(factionDef);
		}
		if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
		{
			return Find.FactionManager.RandomEnemyFaction();
		}
		return map.ParentFaction;
	}
}
