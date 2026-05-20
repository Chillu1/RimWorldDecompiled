using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class LandingOutcomeWorker_GravNausea : LandingOutcomeWorker
{
	public LandingOutcomeWorker_GravNausea(LandingOutcomeDef def)
		: base(def)
	{
	}

	public override void ApplyOutcome(Gravship gravship)
	{
		foreach (Pawn pawn in gravship.Pawns)
		{
			if (pawn.RaceProps.IsFlesh)
			{
				pawn.health?.AddHediff(HediffDefOf.GravNausea);
			}
		}
		SendStandardLetter(gravship.Engine, null, new LookTargets(gravship.Pawns));
	}
}
