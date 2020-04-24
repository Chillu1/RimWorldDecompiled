using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Infestation : IncidentWorker
	{
		public const float HivePoints = 220f;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 cell;
			if (base.CanFireNowSub(parms) && HiveUtility.TotalSpawnedHivesCount(map) < 30)
			{
				return InfestationCellFinder.TryFindCell(out cell, map);
			}
			return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Thing t = InfestationUtility.SpawnTunnels(Mathf.Max(GenMath.RoundRandom(parms.points / 220f), 1), map);
			SendStandardLetter(parms, t);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}
	}
}
