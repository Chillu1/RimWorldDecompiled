using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_MaintainHives : JobGiver_AIFightEnemies
	{
		private bool onlyIfDamagingState;

		private static readonly float CellsInScanRadius = GenRadial.NumCellsInRadius(7.9f);

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_MaintainHives obj = (JobGiver_MaintainHives)base.DeepCopy(resolve);
			obj.onlyIfDamagingState = onlyIfDamagingState;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Room room = pawn.GetRoom();
			for (int i = 0; (float)i < CellsInScanRadius; i++)
			{
				IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(pawn.Map) || intVec.GetRoom(pawn.Map) != room)
				{
					continue;
				}
				Hive hive = (Hive)pawn.Map.thingGrid.ThingAt(intVec, ThingDefOf.Hive);
				if (hive != null && pawn.CanReserve(hive))
				{
					CompMaintainable compMaintainable = hive.TryGetComp<CompMaintainable>();
					if (compMaintainable.CurStage != 0 && (!onlyIfDamagingState || compMaintainable.CurStage == MaintainableStage.Damaging))
					{
						return JobMaker.MakeJob(JobDefOf.Maintain, hive);
					}
				}
			}
			return null;
		}
	}
}
