using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class StatWorker_PossibleCompOffsets : StatWorker
{
	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		float num = base.GetValueUnfinalized(req, applyPostProcess);
		if (req.HasThing)
		{
			CompStatOffsetBase compStatOffsetBase = req.Thing.TryGetComp<CompStatOffsetBase>();
			if (compStatOffsetBase != null && compStatOffsetBase.Props.statDef == stat)
			{
				num += compStatOffsetBase.GetStatOffset(req.Pawn);
			}
		}
		return num;
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		string explanationUnfinalized = base.GetExplanationUnfinalized(req, numberSense);
		StringBuilder stringBuilder = new StringBuilder();
		if (req.Thing != null)
		{
			Thing thing = req.Thing;
			CompStatOffsetBase compStatOffsetBase = thing.TryGetComp<CompStatOffsetBase>();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			if (compStatOffsetBase != null && compStatOffsetBase.Props.statDef == stat)
			{
				stringBuilder.AppendLine();
				for (int i = 0; i < compStatOffsetBase.Props.offsets.Count; i++)
				{
					FocusStrengthOffset focusStrengthOffset = compStatOffsetBase.Props.offsets[i];
					if (focusStrengthOffset.CanApply(thing))
					{
						list.Add(focusStrengthOffset.GetExplanation(thing));
					}
					else
					{
						list2.Add(focusStrengthOffset.GetExplanationAbstract(thing.def));
					}
				}
				if (list.Count > 0)
				{
					stringBuilder.AppendLine(list.ToLineList());
				}
				if (list2.Count > 0)
				{
					if (list.Count > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine("StatReport_PossibleOffsets".Translate() + ":");
					stringBuilder.AppendLine(list2.ToLineList("  - "));
				}
			}
		}
		else if (req.Def is ThingDef thingDef)
		{
			CompProperties_MeditationFocus compProperties = thingDef.GetCompProperties<CompProperties_MeditationFocus>();
			if (compProperties != null && compProperties.offsets.Count > 0 && compProperties.statDef == stat)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("StatReport_PossibleOffsets".Translate() + ":");
				stringBuilder.AppendLine(compProperties.GetExplanationAbstract(thingDef).ToLineList("  - "));
			}
		}
		return explanationUnfinalized + stringBuilder;
	}

	public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
	{
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		if (optionalReq.Thing != null && optionalReq.Thing.Spawned)
		{
			num = (num2 = optionalReq.Thing.def.GetStatValueAbstract(stat));
			Thing thing = optionalReq.Thing;
			CompStatOffsetBase compStatOffsetBase = thing.TryGetComp<CompStatOffsetBase>();
			if (compStatOffsetBase != null && compStatOffsetBase.Props.statDef == stat)
			{
				for (int i = 0; i < compStatOffsetBase.Props.offsets.Count; i++)
				{
					FocusStrengthOffset focusStrengthOffset = compStatOffsetBase.Props.offsets[i];
					if (!focusStrengthOffset.DependsOnPawn)
					{
						if (focusStrengthOffset.CanApply(thing))
						{
							float offset = focusStrengthOffset.GetOffset(thing);
							num += offset;
							num2 += offset;
						}
					}
					else
					{
						flag = true;
					}
				}
			}
		}
		else if (optionalReq.Def is ThingDef)
		{
			(num2, num) = AbstractValueRange(optionalReq, numberSense);
		}
		string text = (flag ? " (+)" : "");
		return RangeToString(num2, num, numberSense, finalized) + text;
	}

	private (float, float) AbstractValueRange(StatRequest req, ToStringNumberSense numberSense)
	{
		ThingDef obj = (ThingDef)req.Def;
		float num2;
		float num = (num2 = obj.GetStatValueAbstract(stat));
		CompProperties_MeditationFocus compProperties = obj.GetCompProperties<CompProperties_MeditationFocus>();
		if (compProperties != null && compProperties.statDef == stat)
		{
			for (int i = 0; i < compProperties.offsets.Count; i++)
			{
				FocusStrengthOffset focusStrengthOffset = compProperties.offsets[i];
				if (!focusStrengthOffset.NeedsToBeSpawned && req.Thing != null)
				{
					num2 += focusStrengthOffset.GetOffset(req.Thing);
					continue;
				}
				float num3 = focusStrengthOffset.MinOffset();
				float num4 = focusStrengthOffset.MaxOffset();
				if (num4 > 0f)
				{
					num += num4;
				}
				if (num4 < 0f)
				{
					num2 += num4;
				}
				num2 += num3;
			}
		}
		return (num2, num);
	}

	private string RangeToString(float min, float max, ToStringNumberSense numberSense, bool finalized)
	{
		if (finalized)
		{
			min = Mathf.Clamp(min, stat.minValue, stat.maxValue);
			max = Mathf.Clamp(max, stat.minValue, stat.maxValue);
		}
		if (max - min >= float.Epsilon)
		{
			string text = min.ToStringByStyle(stat.toStringStyle, numberSense);
			string text2 = stat.ValueToString(max, numberSense, finalized);
			return text + " - " + text2;
		}
		return stat.ValueToString(max, numberSense, finalized);
	}

	public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
	{
		if (!req.HasThing || !req.Thing.Spawned)
		{
			var (min, max) = AbstractValueRange(req, numberSense);
			return "StatsReport_FinalValue".Translate() + ": " + RangeToString(min, max, numberSense, finalized: true);
		}
		return base.GetExplanationFinalizePart(req, numberSense, finalVal);
	}
}
