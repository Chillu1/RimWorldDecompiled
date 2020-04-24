namespace RimWorld
{
	public class Thought_Incestuous : Thought_SituationalSocial
	{
		public override float OpinionOffset()
		{
			if (ThoughtUtility.ThoughtNullified(pawn, def))
			{
				return 0f;
			}
			return LovePartnerRelationUtility.IncestOpinionOffsetFor(otherPawn, pawn);
		}
	}
}
