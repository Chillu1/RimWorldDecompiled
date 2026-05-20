using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_Rechargeable : CompProperties
	{
		public int ticksToRecharge;

		public SoundDef chargedSoundDef;

		public SoundDef dischargeSoundDef;

		public CompProperties_Rechargeable()
		{
			compClass = typeof(CompRechargeable);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (ticksToRecharge <= 0)
			{
				yield return "ticksToRecharge must be a positive value";
			}
		}
	}
}
