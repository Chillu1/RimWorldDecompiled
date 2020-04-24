namespace Verse
{
	public class SubEffecter_SprayerContinuous : SubEffecter_Sprayer
	{
		private int ticksUntilMote;

		public SubEffecter_SprayerContinuous(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			ticksUntilMote--;
			if (ticksUntilMote <= 0)
			{
				MakeMote(A, B);
				ticksUntilMote = def.ticksBetweenMotes;
			}
		}
	}
}
