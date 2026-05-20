using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_IsBloodfeeder : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive)
		{
			return ThoughtState.Inactive;
		}
		return p.IsBloodfeeder();
	}
}
