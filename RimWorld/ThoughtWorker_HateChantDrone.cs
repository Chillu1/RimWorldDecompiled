using System;
using Verse;

namespace RimWorld;

public class ThoughtWorker_HateChantDrone : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		HateChantDroneLevel hateChantDroneLevel = HateChantDroneLevel.None;
		Map mapHeld = p.MapHeld;
		if (mapHeld != null)
		{
			GameCondition_HateChantDrone activeCondition = mapHeld.gameConditionManager.GetActiveCondition<GameCondition_HateChantDrone>();
			if (activeCondition == null || (ModsConfig.AnomalyActive && p.Faction == Faction.OfHoraxCult))
			{
				return false;
			}
			if ((int)activeCondition.level > (int)hateChantDroneLevel)
			{
				hateChantDroneLevel = activeCondition.level;
			}
		}
		return hateChantDroneLevel switch
		{
			HateChantDroneLevel.None => false, 
			HateChantDroneLevel.VeryLow => ThoughtState.ActiveAtStage(0), 
			HateChantDroneLevel.Low => ThoughtState.ActiveAtStage(1), 
			HateChantDroneLevel.Medium => ThoughtState.ActiveAtStage(2), 
			HateChantDroneLevel.High => ThoughtState.ActiveAtStage(3), 
			HateChantDroneLevel.Extreme => ThoughtState.ActiveAtStage(4), 
			_ => throw new NotImplementedException(), 
		};
	}
}
