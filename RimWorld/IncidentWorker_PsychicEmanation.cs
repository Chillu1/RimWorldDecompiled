using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class IncidentWorker_PsychicEmanation : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.PsychicDrone) || map.gameConditionManager.ConditionIsActive(GameConditionDefOf.PsychicSoothe))
			{
				return false;
			}
			if (map.listerThings.ThingsOfDef(ThingDefOf.PsychicDronerShipPart).Count > 0)
			{
				return false;
			}
			if (map.mapPawns.FreeColonistsCount == 0)
			{
				return false;
			}
			return true;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			DoConditionAndLetter(parms, map, Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f), map.mapPawns.FreeColonists.RandomElement().gender, parms.points);
			return true;
		}

		protected abstract void DoConditionAndLetter(IncidentParms parms, Map map, int duration, Gender gender, float points);
	}
}
