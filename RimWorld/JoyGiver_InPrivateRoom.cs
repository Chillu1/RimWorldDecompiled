using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JoyGiver_InPrivateRoom : JoyGiver
{
	public override Job TryGiveJob(Pawn pawn)
	{
		Room room = null;
		if (ModsConfig.IdeologyActive)
		{
			room = MeditationUtility.UsableWorshipRooms(pawn).RandomElementWithFallback();
		}
		if (room == null)
		{
			room = pawn.ownership?.OwnedRoom;
		}
		if (room == null)
		{
			return null;
		}
		if (!room.Cells.Where(Validator).TryRandomElement(out var result))
		{
			return null;
		}
		return JobMaker.MakeJob(def.jobDef, result);
		bool Validator(IntVec3 c)
		{
			if (c.Standable(pawn.Map) && !c.Fogged(pawn.Map) && !c.IsForbidden(pawn))
			{
				return pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None);
			}
			return false;
		}
	}

	public override Job TryGiveJobWhileInBed(Pawn pawn)
	{
		return JobMaker.MakeJob(def.jobDef, pawn.CurrentBed());
	}
}
