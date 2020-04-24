namespace Verse.AI.Group
{
	public class Trigger_UrgentlyHungry : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					if ((int)lord.ownedPawns[i].needs.food.CurCategory >= 2)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
