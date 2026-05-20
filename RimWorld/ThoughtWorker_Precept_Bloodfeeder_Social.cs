using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_Bloodfeeder_Social : ThoughtWorker_Precept_Social
{
	protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
	{
		if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive)
		{
			return ThoughtState.Inactive;
		}
		return otherPawn.IsBloodfeeder();
	}
}
