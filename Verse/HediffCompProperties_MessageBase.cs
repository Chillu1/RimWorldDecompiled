namespace Verse
{
	public class HediffCompProperties_MessageBase : HediffCompProperties
	{
		public MessageTypeDef messageType;

		[MustTranslate]
		public string message;

		public bool onlyMessageForColonistsOrPrisoners = true;

		public HediffCompProperties_MessageBase()
		{
			compClass = typeof(HediffComp_MessageBase);
		}
	}
}
