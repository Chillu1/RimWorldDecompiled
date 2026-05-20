using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class TileMutatorWorker_AbandonedColony : TileMutatorWorker
{
	private static readonly IntRange CorpsesRange = new IntRange(0, 3);

	private static readonly FloatRange DeadDaysRange = new FloatRange(20f, 60f);

	private const float LootMarketValue = 500f;

	public TileMutatorWorker_AbandonedColony(TileMutatorDef def)
		: base(def)
	{
	}

	protected abstract Faction GetFaction();

	public override void GenerateCriticalStructures(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			Faction faction = GetFaction();
			MapGenUtility.PostProcessSettlementParams postProcessSettlementParams = new MapGenUtility.PostProcessSettlementParams
			{
				clearBuildingFaction = true,
				faction = faction,
				damageBuildings = true,
				canDamageWalls = false,
				noFuel = true,
				ageCorpses = true
			};
			GenStep_Settlement genStep_Settlement = new GenStep_Settlement();
			genStep_Settlement.generatePawns = false;
			genStep_Settlement.overrideFaction = faction;
			genStep_Settlement.postProcessSettlementParams = postProcessSettlementParams;
			genStep_Settlement.count = 1;
			genStep_Settlement.lootThingSetMaker = ThingSetMakerDefOf.MapGen_AbandonedColonyStockpile;
			genStep_Settlement.lootMarketValue = 500f;
			genStep_Settlement.Generate(map, default(GenStepParams));
			CellRect var = MapGenerator.GetVar<CellRect>("SettlementRect");
			MapGenUtility.ScatterCorpses(var, map, faction, CorpsesRange, DeadDaysRange);
			MapGenUtility.DestroyTurrets(map);
			MapGenUtility.DestroyProcessedFood(map);
			MapGenUtility.ForbidAllItems(map);
			MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects").Add(var);
		}
	}
}
