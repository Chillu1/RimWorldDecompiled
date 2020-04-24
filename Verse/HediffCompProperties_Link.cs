namespace Verse
{
	public class HediffCompProperties_Link : HediffCompProperties
	{
		public bool showName = true;

		public float maxDistance = -1f;

		public HediffCompProperties_Link()
		{
			compClass = typeof(HediffComp_Link);
		}
	}
}
