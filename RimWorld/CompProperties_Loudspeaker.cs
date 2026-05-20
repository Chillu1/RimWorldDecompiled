using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_Loudspeaker : CompProperties
	{
		public CompProperties_Loudspeaker()
		{
			compClass = typeof(CompLoudspeaker);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (parentDef.comps.FirstOrDefault((CompProperties c) => c.compClass == typeof(CompPowerTrader)) == null)
			{
				yield return "Can't use CompLoudspeaker without CompPowerTrader.";
			}
		}
	}
}
