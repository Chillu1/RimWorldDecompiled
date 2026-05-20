using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_SelfDislikedXenotype : ThoughtWorker_Precept
{
	public override string PostProcessDescription(Pawn p, string description)
	{
		return description.Formatted(p.genes.XenotypeLabel.Named("XENOTYPENAME"));
	}

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive || p.genes == null)
		{
			return ThoughtState.Inactive;
		}
		return !p.Ideo.IsPreferredXenotype(p);
	}
}
