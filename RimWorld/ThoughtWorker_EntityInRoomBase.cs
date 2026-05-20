using Verse;

namespace RimWorld;

public class ThoughtWorker_EntityInRoomBase : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return ThoughtState.Inactive;
		}
		Room bedroom = p.ownership.Bedroom;
		if (bedroom == null || bedroom.TouchesMapEdge)
		{
			return ThoughtState.Inactive;
		}
		foreach (Pawn item in bedroom.ContainedThings<Pawn>())
		{
			if (item.Faction != p.Faction && (item.RaceProps.IsAnomalyEntity || item.IsSubhuman))
			{
				return ThoughtState.ActiveAtStage(0);
			}
		}
		foreach (Building_HoldingPlatform item2 in bedroom.ContainedThings<Building_HoldingPlatform>())
		{
			if (item2.HeldPawn != null && item2.HeldPawn.Faction != p.Faction && (item2.HeldPawn.RaceProps.IsAnomalyEntity || item2.HeldPawn.IsSubhuman))
			{
				return ThoughtState.ActiveAtStage(0);
			}
		}
		return ThoughtState.Inactive;
	}
}
