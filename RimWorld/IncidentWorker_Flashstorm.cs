using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Flashstorm : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			return !((Map)parms.target).gameConditionManager.ConditionIsActive(GameConditionDefOf.Flashstorm);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			int duration = Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f);
			GameCondition_Flashstorm gameCondition_Flashstorm = (GameCondition_Flashstorm)GameConditionMaker.MakeCondition(GameConditionDefOf.Flashstorm, duration);
			map.gameConditionManager.RegisterCondition(gameCondition_Flashstorm);
			SendStandardLetter(def.letterLabel, GameConditionDefOf.Flashstorm.letterText, def.letterDef, parms, new TargetInfo(gameCondition_Flashstorm.centerLocation.ToIntVec3, map));
			if (map.weatherManager.curWeather.rainRate > 0.1f)
			{
				map.weatherDecider.StartNextWeather();
			}
			return true;
		}
	}
}
