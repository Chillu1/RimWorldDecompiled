using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_PreferredXenotype_Social : ThoughtWorker_Precept_Social
{
	protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
	{
		if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive || otherPawn.genes == null)
		{
			return ThoughtState.Inactive;
		}
		if (!p.Ideo.PreferredXenotypes.Any() && !p.Ideo.PreferredCustomXenotypes.Any())
		{
			return ThoughtState.Inactive;
		}
		if (p.Ideo.IsPreferredXenotype(otherPawn))
		{
			return ThoughtState.ActiveAtStage(0);
		}
		return ThoughtState.ActiveAtStage(1);
	}
}
