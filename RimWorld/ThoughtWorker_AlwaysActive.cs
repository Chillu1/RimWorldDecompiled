using Verse;

namespace RimWorld
{
	public class ThoughtWorker_AlwaysActive : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return true;
		}

		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
		{
			return true;
		}
	}
}
