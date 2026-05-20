using Verse;

namespace RimWorld;

public class Thought_IdeoMissingBuilding : Thought_Situational
{
	public IdeoBuildingPresenceDemand demand;

	public const int MinBelieversPresent = 3;

	public override string LabelCap => base.CurStage.LabelCap.Formatted(demand.parent.Named("BUILDING"));

	public override string Description => base.CurStage.description.Formatted(demand.parent.Named("BUILDING"));

	protected override ThoughtState CurrentStateInternal()
	{
		if (pawn.IsSlave)
		{
			return ThoughtState.Inactive;
		}
		if (!Faction.OfPlayer.ideos.Has(demand.parent.ideo))
		{
			return ThoughtState.Inactive;
		}
		if (!Faction.OfPlayer.ideos.IsPrimary(demand.parent.ideo) && demand.parent.ideo.ColonistBelieverCountCached < 3)
		{
			return ThoughtState.Inactive;
		}
		Map mapHeld = pawn.MapHeld;
		return mapHeld != null && demand.AppliesTo(mapHeld) && !demand.BuildingPresent(mapHeld) && !pawn.IsQuestLodger();
	}

	public override bool GroupsWith(Thought other)
	{
		if (other is Thought_IdeoMissingBuilding thought_IdeoMissingBuilding)
		{
			return thought_IdeoMissingBuilding.demand == demand;
		}
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref demand, "demand");
	}
}
