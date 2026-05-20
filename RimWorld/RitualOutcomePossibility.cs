using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomePossibility : ILordJobOutcomePossibility
{
	[MustTranslate]
	public string label;

	[MustTranslate]
	public string description;

	[MustTranslate]
	public string potentialExtraOutcomeDesc;

	public float chance;

	public ThoughtDef memory;

	public int positivityIndex;

	[NoTranslate]
	public List<string> roleIdsNotGainingMemory;

	public float ideoCertaintyOffset;

	public TaggedString Label => label;

	public TaggedString ToolTip => potentialExtraOutcomeDesc;

	public bool Positive => positivityIndex >= 0;

	public bool BestPositiveOutcome(LordJob_Ritual ritual)
	{
		foreach (RitualOutcomePossibility outcomeChance in ritual.Ritual.outcomeEffect.def.outcomeChances)
		{
			if (outcomeChance.positivityIndex > positivityIndex)
			{
				return false;
			}
		}
		return true;
	}

	public float Weight(FloatRange qualityRange)
	{
		if (!Positive)
		{
			return chance;
		}
		return chance * qualityRange.min;
	}
}
