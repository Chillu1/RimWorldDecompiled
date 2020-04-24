using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class StatPart
	{
		public float priority;

		[Unsaved(false)]
		public StatDef parentStat;

		public abstract void TransformValue(StatRequest req, ref float val);

		public abstract string ExplanationPart(StatRequest req);

		public virtual IEnumerable<string> ConfigErrors()
		{
			yield break;
		}

		public virtual IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest req)
		{
			yield break;
		}
	}
}
