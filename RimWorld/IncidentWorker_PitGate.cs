using Verse;

namespace RimWorld;

public class IncidentWorker_PitGate : IncidentWorker
{
	public static readonly LargeBuildingSpawnParms PitGateSpawnParms = new LargeBuildingSpawnParms
	{
		ignoreTerrainAffordance = true
	};

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		Map map = (Map)parms.target;
		if (base.CanFireNowSub(parms) && map.listerThings.ThingsOfDef(ThingDefOf.PitGate).Count == 0)
		{
			return map.listerThings.ThingsOfDef(ThingDefOf.PitGateSpawner).Count == 0;
		}
		return false;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!LargeBuildingCellFinder.TryFindCell(out var cell, map, PitGateSpawnParms.ForThing(ThingDefOf.PitGate)))
		{
			return false;
		}
		BuildingGroundSpawner buildingGroundSpawner = (BuildingGroundSpawner)ThingMaker.MakeThing(ThingDefOf.PitGateSpawner);
		PitGate obj = buildingGroundSpawner.ThingToSpawn as PitGate;
		obj.SetFaction(Faction.OfEntities);
		obj.pointsMultiplier = parms.pointMultiplier;
		GenSpawn.Spawn(buildingGroundSpawner, cell, map);
		SendStandardLetter(parms, buildingGroundSpawner);
		return true;
	}
}
