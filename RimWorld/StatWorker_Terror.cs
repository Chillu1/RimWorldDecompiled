using Verse;

namespace RimWorld;

public class StatWorker_Terror : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		if (!base.ShouldShowFor(req))
		{
			return false;
		}
		if (!(req.Thing is Pawn pawn))
		{
			return false;
		}
		return pawn.IsSlave;
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		return ((Pawn)req.Thing).GetTerrorLevel();
	}
}
