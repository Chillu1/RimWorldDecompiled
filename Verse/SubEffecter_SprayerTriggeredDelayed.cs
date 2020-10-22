namespace Verse
{
	public class SubEffecter_SprayerTriggeredDelayed : SubEffecter_SprayerTriggered
	{
		private int ticksLeft = -1;

		public SubEffecter_SprayerTriggeredDelayed(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubTrigger(TargetInfo A, TargetInfo B)
		{
			ticksLeft = def.initialDelayTicks;
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			if (ticksLeft == 0)
			{
				MakeMote(A, B);
			}
			if (ticksLeft >= 0)
			{
				ticksLeft--;
			}
		}
	}
}
