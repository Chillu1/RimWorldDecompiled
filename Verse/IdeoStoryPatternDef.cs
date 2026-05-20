using System.Collections.Generic;
using Verse.Grammar;

namespace Verse
{
	public class IdeoStoryPatternDef : Def
	{
		[NoTranslate]
		public List<string> segments = new List<string>();

		public List<string> noCapitalizeFirstSentence = new List<string>();

		public RulePack rules;

		public override IEnumerable<string> ConfigErrors()
		{
			if (!segments.Any())
			{
				yield return "Pattern must have at least one segment";
			}
		}
	}
}
