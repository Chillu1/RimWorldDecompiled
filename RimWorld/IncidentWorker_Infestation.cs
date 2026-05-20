using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Infestation : IncidentWorker
	{
		public const float HivePoints = 220f;

		public static readonly SimpleCurve PointsFactorCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0.7f),
			new CurvePoint(5000f, 0.45f)
		};

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 cell;
			if (base.CanFireNowSub(parms) && Faction.OfInsects != null && HiveUtility.TotalSpawnedHivesCount(map) < 30)
			{
				return InfestationCellFinder.TryFindCell(out cell, map);
			}
			return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			parms.points *= PointsFactorCurve.Evaluate(parms.points);
			Thing thing = InfestationUtility.SpawnTunnels(Mathf.Max(GenMath.RoundRandom(parms.points / 220f), 1), map, spawnAnywhereIfNoGoodCell: false, parms.infestationLocOverride.HasValue, null, parms.infestationLocOverride);
			SendStandardLetter(parms, thing);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}
	}
}
