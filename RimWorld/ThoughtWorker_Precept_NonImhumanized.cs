using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_NonImhumanized : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return ThoughtState.Inactive;
		}
		return !p.Inhumanized();
	}
}
