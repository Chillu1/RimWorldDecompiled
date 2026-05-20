namespace Verse
{
	public class HediffCompProperties_GetsPermanent : HediffCompProperties
	{
		public float becomePermanentChanceFactor = 1f;

		[MustTranslate]
		public string permanentLabel;

		[MustTranslate]
		public string instantlyPermanentLabel;

		public HediffCompProperties_GetsPermanent()
		{
			compClass = typeof(HediffComp_GetsPermanent);
		}
	}
}
