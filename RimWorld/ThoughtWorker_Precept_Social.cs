using Verse;

namespace RimWorld;

public abstract class ThoughtWorker_Precept_Social : ThoughtWorker
{
	protected abstract ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn);

	protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
	{
		Ideo ideo = p.Ideo;
		if (ideo == null)
		{
			return ThoughtState.Inactive;
		}
		if (!ideo.cachedPossibleSituationalThoughts.Contains(def))
		{
			return ThoughtState.Inactive;
		}
		if (def.gender != Gender.None && otherPawn.gender != def.gender)
		{
			return ThoughtState.Inactive;
		}
		return ShouldHaveThought(p, otherPawn);
	}

	public override string PostProcessLabel(Pawn p, string label)
	{
		return base.PostProcessLabel(p, label) + " (" + "Ideo".Translate() + ")";
	}

	public override string PostProcessDescription(Pawn p, string description)
	{
		return base.PostProcessDescription(p, description) + "\n\n" + "ComesFromIdeo".Translate() + ": " + p.Ideo.name;
	}
}
