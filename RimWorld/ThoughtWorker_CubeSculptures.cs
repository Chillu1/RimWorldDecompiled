using Verse;

namespace RimWorld;

public class ThoughtWorker_CubeSculptures : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!p.health.hediffSet.TryGetHediff<Hediff_CubeInterest>(out var hediff))
		{
			return false;
		}
		return hediff.Sculptures > 0;
	}

	public override float MoodMultiplier(Pawn p)
	{
		if (!p.health.hediffSet.HasHediff(HediffDefOf.CubeInterest))
		{
			return 1f;
		}
		return p.health.hediffSet.GetFirstHediff<Hediff_CubeInterest>().Sculptures;
	}
}
