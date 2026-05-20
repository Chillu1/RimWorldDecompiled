using System.Collections.Generic;

namespace Verse.AI.Group;

public class Trigger_GameCondition : Trigger
{
	public List<GameConditionDef> gameConditions;

	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % 199 == 0)
		{
			foreach (GameConditionDef gameCondition in gameConditions)
			{
				if (lord.Map.GameConditionManager.ConditionIsActive(gameCondition))
				{
					return true;
				}
			}
		}
		return false;
	}
}
