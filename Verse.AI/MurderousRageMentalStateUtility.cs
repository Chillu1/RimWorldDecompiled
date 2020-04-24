using System.Collections.Generic;

namespace Verse.AI
{
	public static class MurderousRageMentalStateUtility
	{
		private static List<Pawn> tmpTargets = new List<Pawn>();

		public static Pawn FindPawnToKill(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return null;
			}
			tmpTargets.Clear();
			List<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				Pawn pawn2 = allPawnsSpawned[i];
				if ((pawn2.Faction == pawn.Faction || (pawn2.IsPrisoner && pawn2.HostFaction == pawn.Faction)) && pawn2.RaceProps.Humanlike && pawn2 != pawn && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly) && (pawn2.CurJob == null || !pawn2.CurJob.exitMapOnArrival))
				{
					tmpTargets.Add(pawn2);
				}
			}
			if (!tmpTargets.Any())
			{
				return null;
			}
			Pawn result = tmpTargets.RandomElement();
			tmpTargets.Clear();
			return result;
		}
	}
}
