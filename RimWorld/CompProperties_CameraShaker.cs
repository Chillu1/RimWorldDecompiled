using Verse;

namespace RimWorld
{
	public class CompProperties_CameraShaker : CompProperties
	{
		public float mag = 0.05f;

		public CompProperties_CameraShaker()
		{
			compClass = typeof(CompCameraShaker);
		}
	}
}
