using Verse;

namespace RimWorld
{
	public interface ISocialThought
	{
		float OpinionOffset();

		Pawn OtherPawn();
	}
}
