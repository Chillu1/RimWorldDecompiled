using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public class HediffCompProperties_GiveRandomSituationalThought : HediffCompProperties
	{
		public List<ThoughtDef> thoughtDefs;

		public HediffCompProperties_GiveRandomSituationalThought()
		{
			compClass = typeof(HediffComp_GiveRandomSituationalThought);
		}

		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (thoughtDefs.NullOrEmpty())
			{
				yield return "There must be at least one item defined in thoughtDefs";
			}
		}
	}
}
