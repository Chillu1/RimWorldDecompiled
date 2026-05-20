using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_BloodfeederPresent : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive || p.IsBloodfeeder())
		{
			return ThoughtState.Inactive;
		}
		foreach (Pawn item in p.MapHeld.mapPawns.AllPawnsSpawned)
		{
			if (item.IsBloodfeeder() && (item.IsPrisonerOfColony || item.IsSlaveOfColony || item.IsColonist))
			{
				return ThoughtState.ActiveDefault;
			}
		}
		return ThoughtState.Inactive;
	}
}
