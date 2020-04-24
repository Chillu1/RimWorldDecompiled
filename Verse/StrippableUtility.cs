using RimWorld;

namespace Verse
{
	public static class StrippableUtility
	{
		public static bool CanBeStrippedByColony(Thing th)
		{
			IStrippable strippable = th as IStrippable;
			if (strippable == null)
			{
				return false;
			}
			if (!strippable.AnythingToStrip())
			{
				return false;
			}
			Pawn pawn = th as Pawn;
			if (pawn == null)
			{
				return true;
			}
			if (pawn.IsQuestLodger())
			{
				return false;
			}
			if (pawn.Downed)
			{
				return true;
			}
			if (pawn.IsPrisonerOfColony && pawn.guest.PrisonerIsSecure)
			{
				return true;
			}
			return false;
		}
	}
}
