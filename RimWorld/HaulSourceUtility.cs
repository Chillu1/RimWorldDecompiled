using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class HaulSourceUtility
{
	public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(IHaulSource source, Pawn pawn)
	{
		foreach (Thing item in (IEnumerable<Thing>)source.GetDirectlyHeldThings())
		{
			StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(item, forced: true);
			if (!item.MapHeld.listerHaulables.ThingsPotentiallyNeedingHauling().Contains(item) || !StoreUtility.TryFindBestBetterStorageFor(item, pawn, pawn.MapHeld, currentPriority, pawn.Faction, out var _, out var _))
			{
				continue;
			}
			yield return new FloatMenuOption("HaulFromSource".Translate(), delegate
			{
				bool flag = false;
				foreach (Thing item2 in (IEnumerable<Thing>)source.GetDirectlyHeldThings())
				{
					Job job = HaulAIUtility.HaulToStorageJob(pawn, item2, forced: true);
					if (job != null)
					{
						job.playerForced = true;
						if (!flag)
						{
							flag = true;
							pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
						}
						else
						{
							pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc, requestQueueing: true);
						}
					}
				}
			});
			break;
		}
	}
}
