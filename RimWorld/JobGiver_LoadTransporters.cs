using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_LoadTransporters : ThinkNode_JobGiver
	{
		private static List<CompTransporter> tmpTransporters = new List<CompTransporter>();

		protected override Job TryGiveJob(Pawn pawn)
		{
			TransporterUtility.GetTransportersInGroup(pawn.mindState.duty.transportersGroup, pawn.Map, tmpTransporters);
			for (int i = 0; i < tmpTransporters.Count; i++)
			{
				CompTransporter transporter = tmpTransporters[i];
				if (LoadTransportersJobUtility.HasJobOnTransporter(pawn, transporter))
				{
					return LoadTransportersJobUtility.JobOnTransporter(pawn, transporter);
				}
			}
			return null;
		}
	}
}
