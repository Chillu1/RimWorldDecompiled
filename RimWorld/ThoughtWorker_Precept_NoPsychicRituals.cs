using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_NoPsychicRituals : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
{
	public const int TicksSinceLastPsychicRitualActiveTicks = 360000;

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!p.IsColonist)
		{
			return false;
		}
		return Find.TickManager.TicksGame > Find.IdeoManager.lastPsychicRitualPerformedTick + 360000;
	}

	public IEnumerable<NamedArgument> GetDescriptionArgs()
	{
		yield return 360000.ToStringTicksToPeriod().Named("DURATION");
	}
}
