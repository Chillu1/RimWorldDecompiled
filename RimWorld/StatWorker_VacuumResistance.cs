using Verse;

namespace RimWorld;

public class StatWorker_VacuumResistance : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		if (!base.ShouldShowFor(req))
		{
			return false;
		}
		if (req.Thing is Pawn pawn)
		{
			return pawn.RaceProps.IsFlesh;
		}
		return false;
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		if (req.Thing is Pawn { IsMutant: not false } pawn && !pawn.mutant.Def.breathesAir)
		{
			return 1f;
		}
		return base.GetValueUnfinalized(req, applyPostProcess);
	}
}
