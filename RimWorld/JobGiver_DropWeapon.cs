using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_DropWeapon : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.equipment != null)
			{
				foreach (ThingWithComps item in pawn.equipment.AllEquipmentListForReading)
				{
					if (item.def.IsWeapon)
					{
						return JobMaker.MakeJob(JobDefOf.DropEquipment, item);
					}
				}
			}
			return null;
		}
	}
}
