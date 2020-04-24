namespace Verse
{
	public class HediffCompProperties_Discoverable : HediffCompProperties
	{
		public bool sendLetterWhenDiscovered;

		public string discoverLetterLabel;

		public string discoverLetterText;

		public MessageTypeDef messageType;

		public LetterDef letterType;

		public HediffCompProperties_Discoverable()
		{
			compClass = typeof(HediffComp_Discoverable);
		}
	}
}
