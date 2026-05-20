using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_Lightball : CompProperties
	{
		public List<SoundDef> soundDefsPerSpeakerCount;

		public int maxSpeakerDistance;

		public CompProperties_Lightball()
		{
			compClass = typeof(CompLightball);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (parentDef.comps.FirstOrDefault((CompProperties c) => c.compClass == typeof(CompPowerTrader)) == null)
			{
				yield return "Can't use CompLightball without CompPowerTrader.";
			}
		}
	}
}
