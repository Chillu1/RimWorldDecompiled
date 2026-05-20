using Verse;

namespace RimWorld;

public class StatWorker_PsyfocusCost : StatWorker
{
	public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
	{
		if (optionalReq.ForAbility)
		{
			AbilityDef abilityDef = optionalReq.AbilityDef;
			if (abilityDef.AnyCompOverridesPsyfocusCost)
			{
				if (abilityDef.PsyfocusCostRange.Span > float.Epsilon)
				{
					return (abilityDef.PsyfocusCostRange.min * 100f).ToString("0.##") + "-" + stat.ValueToString(abilityDef.PsyfocusCostRange.max, numberSense, finalized);
				}
				return stat.ValueToString(abilityDef.PsyfocusCostRange.max, numberSense, finalized);
			}
		}
		return base.GetStatDrawEntryLabel(stat, value, numberSense, optionalReq, finalized);
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		if (req.ForAbility)
		{
			foreach (AbilityCompProperties comp in req.AbilityDef.comps)
			{
				if (comp.OverridesPsyfocusCost)
				{
					return comp.PsyfocusCostExplanation;
				}
			}
		}
		return base.GetExplanationUnfinalized(req, numberSense);
	}
}
