using Verse;
using Verse.AI;

namespace RimWorld;

public class LearningGiver_Lessontaking : LearningGiver
{
	public override bool CanDo(Pawn pawn)
	{
		if (!base.CanDo(pawn))
		{
			return false;
		}
		if (PawnUtility.WillSoonHaveBasicNeed(pawn))
		{
			return false;
		}
		Pawn pawn2 = SchoolUtility.FindTeacher(pawn);
		if (pawn2 == null)
		{
			return false;
		}
		Thing thing = SchoolUtility.ClosestSchoolDesk(pawn, pawn2);
		if (thing == null || !pawn.CanReserveSittableOrSpot(SchoolUtility.DeskSpotStudent(thing)))
		{
			return false;
		}
		return true;
	}

	public override Job TryGiveJob(Pawn pawn)
	{
		Pawn pawn2 = SchoolUtility.FindTeacher(pawn);
		if (pawn2 == null)
		{
			return null;
		}
		Thing thing = SchoolUtility.ClosestSchoolDesk(pawn, pawn2);
		if (thing == null)
		{
			return null;
		}
		return JobMaker.MakeJob(def.jobDef, thing);
	}
}
