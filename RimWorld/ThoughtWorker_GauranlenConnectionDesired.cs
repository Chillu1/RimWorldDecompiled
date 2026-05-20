using UnityEngine;
using Verse;

namespace RimWorld;

public class ThoughtWorker_GauranlenConnectionDesired : ThoughtWorker_Precept
{
	private const int TicksJoinedMin = 900000;

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (ModsConfig.BiotechActive && p.DevelopmentalStage.Juvenile())
		{
			return ThoughtState.Inactive;
		}
		if (p.connections == null || !p.connections.ConnectedThings.Any())
		{
			if (p.playerSettings == null || p.IsSlave || p.IsPrisoner || p.IsQuestLodger())
			{
				return ThoughtState.Inactive;
			}
			if (p.WorkTypeIsDisabled(WorkTypeDefOf.PlantCutting))
			{
				return ThoughtState.Inactive;
			}
			int num = Mathf.Max(0, p.ideo.joinTick, p.playerSettings.joinTick);
			if (Find.TickManager.TicksGame - num < 900000)
			{
				return ThoughtState.Inactive;
			}
		}
		int stageIndex = 0;
		if (p.connections == null || p.connections.ConnectedThings.NullOrEmpty())
		{
			if (p.MapHeld == null)
			{
				stageIndex = 1;
			}
			else
			{
				ExpectationDef expectationDef = ExpectationsUtility.CurrentExpectationFor(p.MapHeld);
				if (expectationDef != null)
				{
					stageIndex = ((expectationDef.order <= ExpectationDefOf.VeryLow.order) ? 1 : ((expectationDef.order <= ExpectationDefOf.Low.order) ? 2 : ((expectationDef.order <= ExpectationDefOf.Moderate.order) ? 3 : ((expectationDef.order > ExpectationDefOf.High.order) ? 5 : 4))));
				}
			}
		}
		return ThoughtState.ActiveAtStage(stageIndex);
	}
}
