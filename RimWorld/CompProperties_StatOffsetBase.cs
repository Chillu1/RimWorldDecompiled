using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class CompProperties_StatOffsetBase : CompProperties
{
	public StatDef statDef;

	public List<FocusStrengthOffset> offsets = new List<FocusStrengthOffset>();

	public virtual IEnumerable<string> GetExplanationAbstract(ThingDef def)
	{
		return Enumerable.Empty<string>();
	}

	public virtual float GetMaxOffset(Thing parent = null)
	{
		float num = 0f;
		for (int i = 0; i < offsets.Count; i++)
		{
			num += offsets[i].MaxOffset(parent);
		}
		return num;
	}
}
