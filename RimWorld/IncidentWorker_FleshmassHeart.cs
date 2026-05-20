using Verse;

namespace RimWorld;

public class IncidentWorker_FleshmassHeart : IncidentWorker
{
	public static readonly LargeBuildingSpawnParms HeartSpawnParms = new LargeBuildingSpawnParms
	{
		minDistToEdge = 10,
		minDistanceToColonyBuilding = 20f,
		preferFarFromColony = true
	};

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		return base.CanFireNowSub(parms);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!LargeBuildingCellFinder.TryFindCell(out var cell, map, HeartSpawnParms.ForThing(ThingDefOf.FleshmassHeart)))
		{
			return false;
		}
		BuildingGroundSpawner buildingGroundSpawner = (BuildingGroundSpawner)ThingMaker.MakeThing(ThingDefOf.FleshmassHeartSpawner);
		Building_FleshmassHeart obj = buildingGroundSpawner.ThingToSpawn as Building_FleshmassHeart;
		obj.Comp.threatPoints = parms.points;
		obj.SetFaction(Faction.OfEntities);
		GenSpawn.Spawn(buildingGroundSpawner, cell, map);
		SendStandardLetter(parms, buildingGroundSpawner);
		return true;
	}
}
