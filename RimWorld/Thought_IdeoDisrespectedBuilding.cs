using Verse;

namespace RimWorld;

public class Thought_IdeoDisrespectedBuilding : Thought_Situational
{
	public IdeoBuildingPresenceDemand demand;

	public override string LabelCap => base.CurStage.LabelCap.Formatted(demand.parent.Named("BUILDING"));

	public override string Description => base.CurStage.description.Formatted(demand.parent.Named("BUILDING"));

	protected override ThoughtState CurrentStateInternal()
	{
		if (!Faction.OfPlayer.ideos.Has(demand.parent.ideo))
		{
			return ThoughtState.Inactive;
		}
		Map mapHeld = pawn.MapHeld;
		return mapHeld != null && demand.AppliesTo(mapHeld) && demand.BuildingPresent(mapHeld) && !demand.RequirementsSatisfied(mapHeld);
	}

	public override bool GroupsWith(Thought other)
	{
		if (other is Thought_IdeoDisrespectedBuilding thought_IdeoDisrespectedBuilding && thought_IdeoDisrespectedBuilding.demand == demand)
		{
			return !pawn.IsQuestLodger();
		}
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref demand, "demand");
	}
}
