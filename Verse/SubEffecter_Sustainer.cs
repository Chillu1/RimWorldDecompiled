using Verse.Sound;

namespace Verse
{
	public class SubEffecter_Sustainer : SubEffecter
	{
		private int age;

		private Sustainer sustainer;

		public SubEffecter_Sustainer(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			age++;
			if (age > def.ticksBeforeSustainerStart)
			{
				if (sustainer == null)
				{
					SoundInfo info = SoundInfo.InMap(A, MaintenanceType.PerTick);
					sustainer = def.soundDef.TrySpawnSustainer(info);
				}
				else
				{
					sustainer.Maintain();
				}
			}
		}
	}
}
