using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_StatEntry : CompProperties
	{
		public StatCategoryDef statCategoryDef;

		[MustTranslate]
		public string statLabel;

		[MustTranslate]
		public string reportText;

		public string valueString;

		public Func<CompStatEntry, string> valueFunc;

		public int displayPriorityInCategory;

		public CompProperties_StatEntry()
		{
			compClass = typeof(CompStatEntry);
		}
	}
}
