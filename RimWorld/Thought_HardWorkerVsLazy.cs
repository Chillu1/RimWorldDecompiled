namespace RimWorld;

public class Thought_HardWorkerVsLazy : Thought_SituationalSocial
{
	public override float OpinionOffset()
	{
		if (ThoughtUtility.ThoughtNullified(pawn, def))
		{
			return 0f;
		}
		int num = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Industriousness);
		if (num > 0)
		{
			return 0f;
		}
		return num switch
		{
			0 => -5f, 
			-1 => -20f, 
			_ => -30f, 
		};
	}
}
