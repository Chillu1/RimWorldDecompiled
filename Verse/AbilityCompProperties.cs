using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public class AbilityCompProperties
{
	[TranslationHandle]
	public Type compClass;

	public virtual bool OverridesPsyfocusCost => false;

	public virtual FloatRange PsyfocusCostRange => FloatRange.ZeroToOne;

	public virtual string PsyfocusCostExplanation => "";

	public virtual IEnumerable<string> ExtraStatSummary()
	{
		return Enumerable.Empty<string>();
	}

	public virtual IEnumerable<string> ConfigErrors(AbilityDef parentDef)
	{
		if (compClass == null)
		{
			yield return "compClass is null";
		}
		for (int i = 0; i < parentDef.comps.Count; i++)
		{
			if (parentDef.comps[i] != this && parentDef.comps[i].compClass == compClass)
			{
				yield return "two comps with same compClass: " + compClass;
			}
		}
	}
}
