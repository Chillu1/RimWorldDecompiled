using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AITrashColonyClose : ThinkNode_JobGiver
	{
		private const int CloseSearchRadius = 5;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.HostileTo(Faction.OfPlayer))
			{
				return null;
			}
			bool flag = pawn.natives.IgniteVerb != null && pawn.natives.IgniteVerb.IsStillUsableBy(pawn) && pawn.HostileTo(Faction.OfPlayer);
			CellRect cellRect = CellRect.CenteredOn(pawn.Position, 5);
			for (int i = 0; i < 35; i++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				if (!randomCell.InBounds(pawn.Map))
				{
					continue;
				}
				Building edifice = randomCell.GetEdifice(pawn.Map);
				if (edifice != null && TrashUtility.ShouldTrashBuilding(pawn, edifice) && GenSight.LineOfSight(pawn.Position, randomCell, pawn.Map))
				{
					if (DebugViewSettings.drawDestSearch && Find.CurrentMap == pawn.Map)
					{
						Find.CurrentMap.debugDrawer.FlashCell(randomCell, 1f, "trash bld");
					}
					Job job = TrashUtility.TrashJob(pawn, edifice);
					if (job != null)
					{
						return job;
					}
				}
				if (flag)
				{
					Plant plant = randomCell.GetPlant(pawn.Map);
					if (plant != null && TrashUtility.ShouldTrashPlant(pawn, plant) && GenSight.LineOfSight(pawn.Position, randomCell, pawn.Map))
					{
						if (DebugViewSettings.drawDestSearch && Find.CurrentMap == pawn.Map)
						{
							Find.CurrentMap.debugDrawer.FlashCell(randomCell, 0.5f, "trash plant");
						}
						Job job2 = TrashUtility.TrashJob(pawn, plant);
						if (job2 != null)
						{
							return job2;
						}
					}
				}
				if (DebugViewSettings.drawDestSearch && Find.CurrentMap == pawn.Map)
				{
					Find.CurrentMap.debugDrawer.FlashCell(randomCell, 0f, "trash no");
				}
			}
			return null;
		}
	}
}
