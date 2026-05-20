using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_GrowthVatEssentialPregnant : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.IdeologyActive || !ModsConfig.BiotechActive)
		{
			return ThoughtState.Inactive;
		}
		return p.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman);
	}
}
