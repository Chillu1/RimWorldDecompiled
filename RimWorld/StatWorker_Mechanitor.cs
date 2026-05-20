using Verse;

namespace RimWorld;

public class StatWorker_Mechanitor : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		if (!base.ShouldShowFor(req))
		{
			return false;
		}
		if (req.Thing != null && req.Thing is Pawn pawn)
		{
			return MechanitorUtility.IsMechanitor(pawn);
		}
		return false;
	}
}
