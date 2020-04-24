using Verse;

namespace RimWorld
{
	[DefOf]
	public static class LetterDefOf
	{
		public static LetterDef ThreatBig;

		public static LetterDef ThreatSmall;

		public static LetterDef NegativeEvent;

		public static LetterDef NeutralEvent;

		public static LetterDef PositiveEvent;

		public static LetterDef Death;

		public static LetterDef NewQuest;

		static LetterDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(LetterDefOf));
		}
	}
}
