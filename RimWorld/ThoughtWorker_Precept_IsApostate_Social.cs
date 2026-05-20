using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_IsApostate_Social : ThoughtWorker_Precept_Social
	{
		protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			return otherPawn.ideo != null && otherPawn.Ideo != p.Ideo && otherPawn.ideo.PreviousIdeos.Contains(p.Ideo);
		}
	}
}
