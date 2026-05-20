namespace RimWorld
{
	public class CompProperties_RitualEffectConstantCircle : CompProperties_RitualVisualEffect
	{
		public float radius = 5f;

		public int numCopies = 5;

		public CompProperties_RitualEffectConstantCircle()
		{
			compClass = typeof(CompRitualEffect_ConstantCircle);
		}
	}
}
