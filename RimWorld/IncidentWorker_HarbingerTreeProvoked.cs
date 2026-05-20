using Verse;

namespace RimWorld
{
	public class IncidentWorker_HarbingerTreeProvoked : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!IncidentWorker_HarbingerTreeSpawn.TryGetHarbingerTreeSpawnCell(parms.target as Map, out var _))
			{
				return false;
			}
			return base.CanFireNowSub(parms);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = parms.target as Map;
			if (!IncidentWorker_HarbingerTreeSpawn.TryGetHarbingerTreeSpawnCell(map, out var cell))
			{
				return false;
			}
			Plant plant = (Plant)ThingMaker.MakeThing(ThingDefOf.Plant_TreeHarbinger);
			plant.Growth = 0.15f;
			GenSpawn.Spawn(plant, cell, map);
			SendStandardLetter(def.letterLabel, def.letterText, def.letterDef, parms, plant);
			return true;
		}
	}
}
