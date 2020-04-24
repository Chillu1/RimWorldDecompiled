using Verse.Sound;

namespace Verse
{
	public class SubEffecter_SoundIntermittent : SubEffecter
	{
		protected int ticksUntilSound;

		public SubEffecter_SoundIntermittent(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
			ticksUntilSound = def.intermittentSoundInterval.RandomInRange;
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			ticksUntilSound--;
			if (ticksUntilSound <= 0)
			{
				def.soundDef.PlayOneShot(A);
				ticksUntilSound = def.intermittentSoundInterval.RandomInRange;
			}
		}
	}
}
