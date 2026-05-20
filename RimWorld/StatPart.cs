using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public abstract class StatPart
{
	public float priority;

	[Unsaved(false)]
	public StatDef parentStat;

	public abstract void TransformValue(StatRequest req, ref float val);

	public abstract string ExplanationPart(StatRequest req);

	public virtual IEnumerable<string> ConfigErrors()
	{
		return Enumerable.Empty<string>();
	}

	public virtual IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest req)
	{
		return Enumerable.Empty<Dialog_InfoCard.Hyperlink>();
	}

	public virtual bool ForceShow(StatRequest req)
	{
		return false;
	}
}
