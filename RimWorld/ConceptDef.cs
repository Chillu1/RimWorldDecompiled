using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ConceptDef : Def
	{
		public float priority = float.MaxValue;

		public bool noteTeaches;

		public bool needsOpportunity;

		public bool opportunityDecays = true;

		public ProgramState gameMode = ProgramState.Playing;

		[MustTranslate]
		private string helpText;

		[NoTranslate]
		public List<string> highlightTags;

		private static List<string> tmpParseErrors = new List<string>();

		public bool TriggeredDirect => priority <= 0f;

		public string HelpTextAdjusted => helpText.AdjustedForKeys();

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (priority > 9999999f)
			{
				yield return "priority isn't set";
			}
			if (helpText.NullOrEmpty())
			{
				yield return "no help text";
			}
			if (TriggeredDirect && label.NullOrEmpty())
			{
				yield return "no label";
			}
			tmpParseErrors.Clear();
			helpText.AdjustedForKeys(tmpParseErrors, resolveKeys: false);
			for (int i = 0; i < tmpParseErrors.Count; i++)
			{
				yield return "helpText error: " + tmpParseErrors[i];
			}
		}

		public static ConceptDef Named(string defName)
		{
			return DefDatabase<ConceptDef>.GetNamed(defName);
		}

		public void HighlightAllTags()
		{
			if (highlightTags != null)
			{
				for (int i = 0; i < highlightTags.Count; i++)
				{
					UIHighlighter.HighlightTag(highlightTags[i]);
				}
			}
		}
	}
}
