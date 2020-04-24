namespace Verse.AI.Group
{
	public class Trigger_NoMentalState : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					if (lord.ownedPawns[i].InMentalState)
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
	}
}
