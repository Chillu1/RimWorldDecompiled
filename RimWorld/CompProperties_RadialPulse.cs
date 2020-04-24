using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_RadialPulse : CompProperties
	{
		public int ticksBetweenPulses = 300;

		public int ticksPerPulse = 60;

		public Color color;

		public float radius;

		public CompProperties_RadialPulse()
		{
			compClass = typeof(CompRadialPulse);
		}
	}
}
