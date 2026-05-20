using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_Toxifier : CompProperties
	{
		public float radius = 5.9f;

		public int pollutionIntervalTicks = 60000;

		public int cellsToPollute = 6;

		public CompProperties_Toxifier()
		{
			compClass = typeof(CompToxifier);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (pollutionIntervalTicks <= 0)
			{
				yield return "Pollution interval ticks must be greater than zero.";
			}
			if (cellsToPollute <= 0)
			{
				yield return "Cells to pollute must be greater than zero.";
			}
			if (radius <= 0f)
			{
				yield return "Radius to pollute must be greater than zero.";
			}
		}
	}
}
