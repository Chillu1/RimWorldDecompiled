using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_RitualEffectStaticAreaRandomMote : CompProperties_RitualEffectIntervalSpawnArea
	{
		public float minDist = 1.5f;

		public List<ThingDef> moteDefs;

		public CompProperties_RitualEffectStaticAreaRandomMote()
		{
			compClass = typeof(CompRitualEffect_StaticAreaRandomMote);
		}
	}
}
