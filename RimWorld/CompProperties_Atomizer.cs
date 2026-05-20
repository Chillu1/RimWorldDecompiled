using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_Atomizer : CompProperties_ThingContainer
	{
		public ThingDef thingDef;

		public EffecterDef resolveEffecter;

		public EffecterDef workingEffecter;

		public SoundDef materialsAddedSound;

		public int resolveEffecterTicks = 40;

		public int ticksPerAtomize = 2500;

		public Vector3 contentsDrawOffset = Vector3.zero;

		public CompProperties_Atomizer()
		{
			compClass = typeof(CompAtomizer);
		}
	}
}
