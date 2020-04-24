using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_InPrivateRoom : JoyGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.ownership == null)
			{
				return null;
			}
			Room ownedRoom = pawn.ownership.OwnedRoom;
			if (ownedRoom == null)
			{
				return null;
			}
			if (!ownedRoom.Cells.Where((IntVec3 c) => c.Standable(pawn.Map) && !c.IsForbidden(pawn) && pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None)).TryRandomElement(out IntVec3 result))
			{
				return null;
			}
			return JobMaker.MakeJob(def.jobDef, result);
		}

		public override Job TryGiveJobWhileInBed(Pawn pawn)
		{
			return JobMaker.MakeJob(def.jobDef, pawn.CurrentBed());
		}
	}
}
