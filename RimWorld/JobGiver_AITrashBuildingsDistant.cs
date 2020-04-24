using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AITrashBuildingsDistant : ThinkNode_JobGiver
	{
		public bool attackAllInert;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_AITrashBuildingsDistant obj = (JobGiver_AITrashBuildingsDistant)base.DeepCopy(resolve);
			obj.attackAllInert = attackAllInert;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
			if (allBuildingsColonist.Count == 0)
			{
				return null;
			}
			for (int i = 0; i < 75; i++)
			{
				Building building = allBuildingsColonist.RandomElement();
				if (TrashUtility.ShouldTrashBuilding(pawn, building, attackAllInert))
				{
					Job job = TrashUtility.TrashJob(pawn, building, attackAllInert);
					if (job != null)
					{
						return job;
					}
				}
			}
			return null;
		}
	}
}
