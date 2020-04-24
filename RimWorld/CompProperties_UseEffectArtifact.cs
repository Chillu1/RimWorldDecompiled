using Verse;

namespace RimWorld
{
	public class CompProperties_UseEffectArtifact : CompProperties_UseEffect
	{
		public SoundDef sound;

		public CompProperties_UseEffectArtifact()
		{
			compClass = typeof(CompUseEffect_Artifact);
		}
	}
}
