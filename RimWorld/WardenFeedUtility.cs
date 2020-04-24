using Verse;

namespace RimWorld
{
	public static class WardenFeedUtility
	{
		public static bool ShouldBeFed(Pawn p)
		{
			if (!p.IsPrisonerOfColony)
			{
				return false;
			}
			if (!p.InBed())
			{
				return false;
			}
			if (!p.guest.CanBeBroughtFood)
			{
				return false;
			}
			if (!HealthAIUtility.ShouldSeekMedicalRest(p))
			{
				return false;
			}
			return true;
		}
	}
}
