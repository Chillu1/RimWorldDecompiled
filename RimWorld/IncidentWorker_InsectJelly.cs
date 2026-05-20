using Verse;

namespace RimWorld
{
	public class IncidentWorker_InsectJelly : IncidentWorker
	{
		public const int JellyPointsCost = 8;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 cell;
			if (base.CanFireNowSub(parms) && Faction.OfInsects != null)
			{
				return InfestationCellFinder.TryFindCell(out cell, map);
			}
			return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Thing thing = InfestationUtility.SpawnJellyTunnels(Rand.Range(2, 3), (int)parms.points / 8, map);
			SendStandardLetter(parms, thing);
			return true;
		}
	}
}
