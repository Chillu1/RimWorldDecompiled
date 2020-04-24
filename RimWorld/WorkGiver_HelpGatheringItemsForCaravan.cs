using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class WorkGiver_HelpGatheringItemsForCaravan : WorkGiver
	{
		public override Job NonScanJob(Pawn pawn)
		{
			List<Lord> lords = pawn.Map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan = lords[i].LordJob as LordJob_FormAndSendCaravan;
				if (lordJob_FormAndSendCaravan != null && lordJob_FormAndSendCaravan.GatheringItemsNow)
				{
					Thing thing = GatherItemsForCaravanUtility.FindThingToHaul(pawn, lords[i]);
					if (thing != null && AnyReachableCarrierOrColonist(pawn, lords[i]))
					{
						Job job = JobMaker.MakeJob(JobDefOf.PrepareCaravan_GatherItems, thing);
						job.lord = lords[i];
						return job;
					}
				}
			}
			return null;
		}

		private bool AnyReachableCarrierOrColonist(Pawn forPawn, Lord lord)
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				if (JobDriver_PrepareCaravan_GatherItems.IsUsableCarrier(lord.ownedPawns[i], forPawn, allowColonists: false) && !lord.ownedPawns[i].IsForbidden(forPawn) && forPawn.CanReach(lord.ownedPawns[i], PathEndMode.Touch, Danger.Deadly))
				{
					return true;
				}
			}
			return false;
		}
	}
}
