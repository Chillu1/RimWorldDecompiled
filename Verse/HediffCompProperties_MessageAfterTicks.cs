using System.Collections.Generic;

namespace Verse
{
	public class HediffCompProperties_MessageAfterTicks : HediffCompProperties
	{
		public int ticks;

		public MessageTypeDef messageType;

		public LetterDef letterType;

		[MustTranslate]
		public string message;

		[MustTranslate]
		public string letterLabel;

		[MustTranslate]
		public string letterText;

		public HediffCompProperties_MessageAfterTicks()
		{
			compClass = typeof(HediffComp_MessageAfterTicks);
		}

		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			if (ticks <= 0)
			{
				yield return "ticks must be a positive value";
			}
		}
	}
}
