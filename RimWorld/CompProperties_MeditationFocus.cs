using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_MeditationFocus : CompProperties_StatOffsetBase
{
	public List<MeditationFocusDef> focusTypes = new List<MeditationFocusDef>();

	public CompProperties_MeditationFocus()
	{
		compClass = typeof(CompMeditationFocus);
	}

	public override IEnumerable<string> GetExplanationAbstract(ThingDef def)
	{
		for (int i = 0; i < offsets.Count; i++)
		{
			string explanationAbstract = offsets[i].GetExplanationAbstract(def);
			if (!explanationAbstract.NullOrEmpty())
			{
				yield return explanationAbstract;
			}
		}
	}

	public override void ResolveReferences(ThingDef parent)
	{
		base.PostLoadSpecial(parent);
		for (int i = 0; i < offsets.Count; i++)
		{
			offsets[i].ResolveReferences();
		}
	}
}
