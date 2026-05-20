using Verse;
using Verse.AI;

namespace RimWorld;

public class LearningGiver_Reading : LearningGiver
{
	public override bool CanDo(Pawn pawn)
	{
		if (!base.CanDo(pawn) || !BookUtility.CanReadNow(pawn))
		{
			return false;
		}
		if (PawnUtility.WillSoonHaveBasicNeed(pawn))
		{
			return false;
		}
		Book book;
		return BookUtility.TryGetRandomBookToRead(pawn, out book);
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
