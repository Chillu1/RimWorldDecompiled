using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class LandingOutcomeWorker_OverheatedGravEngine : LandingOutcomeWorker
{
	public LandingOutcomeWorker_OverheatedGravEngine(LandingOutcomeDef def)
		: base(def)
	{
	}

	public override void ApplyOutcome(Gravship gravship)
	{
		gravship.Engine.cooldownCompleteTick += Rand.Range(120000, 240000);
		SendStandardLetter(gravship.Engine, null, gravship.Engine);
	}
}
