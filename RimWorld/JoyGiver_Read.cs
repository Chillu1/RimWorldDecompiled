using Verse;
using Verse.AI;

namespace RimWorld;

public class JoyGiver_Read : JoyGiver
{
	public override bool CanBeGivenTo(Pawn pawn)
	{
		if (!BookUtility.CanReadNow(pawn))
		{
			return false;
		}
		if (PawnUtility.WillSoonHaveBasicNeed(pawn))
		{
			return false;
		}
		return base.CanBeGivenTo(pawn);
	}

	public override Job TryGiveJob(Pawn pawn)
	{
		if (BookUtility.TryGetRandomBookToRead(pawn, out var book))
		{
			return JobMaker.MakeJob(def.jobDef, book);
		}
		return null;
	}
}
