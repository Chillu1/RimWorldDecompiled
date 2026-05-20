using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class IdeoDescriptionMaker
	{
		public class PatternEntry
		{
			public IdeoStoryPatternDef def;

			public float weight = 1f;
		}

		public List<PatternEntry> patterns;

		public RulePack rules;

		public Dictionary<string, string> constants;
	}
}
