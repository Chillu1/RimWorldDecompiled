using Verse;

namespace RimWorld;

public class ThoughtWorker_VoidFascination : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return ThoughtState.Inactive;
		}
		foreach (Thing item in p.Map.listerThings.ThingsInGroup(ThingRequestGroup.EntityHolder))
		{
			if (item?.TryGetComp<CompEntityHolder>()?.HeldPawn != null)
			{
				return ThoughtState.ActiveAtStage(0);
			}
		}
		return ThoughtState.Inactive;
	}
}
