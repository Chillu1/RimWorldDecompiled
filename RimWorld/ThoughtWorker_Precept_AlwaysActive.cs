using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_AlwaysActive : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		return ThoughtState.ActiveDefault;
	}
}
