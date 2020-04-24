using RimWorld;

namespace Verse.AI.Group
{
	public class Trigger_NoPawnsVeryTiredAndSleeping : Trigger
	{
		private float extraRestThreshOffset;

		public Trigger_NoPawnsVeryTiredAndSleeping(float extraRestThreshOffset = 0f)
		{
			this.extraRestThreshOffset = extraRestThreshOffset;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					Need_Rest rest = lord.ownedPawns[i].needs.rest;
					if (rest != null && rest.CurLevelPercentage < 0.14f + extraRestThreshOffset && !lord.ownedPawns[i].Awake())
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
