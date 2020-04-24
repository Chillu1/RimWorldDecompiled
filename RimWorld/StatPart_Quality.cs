using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatPart_Quality : StatPart
	{
		private bool applyToNegativeValues;

		private float factorAwful = 1f;

		private float factorPoor = 1f;

		private float factorNormal = 1f;

		private float factorGood = 1f;

		private float factorExcellent = 1f;

		private float factorMasterwork = 1f;

		private float factorLegendary = 1f;

		private float maxGainAwful = 9999999f;

		private float maxGainPoor = 9999999f;

		private float maxGainNormal = 9999999f;

		private float maxGainGood = 9999999f;

		private float maxGainExcellent = 9999999f;

		private float maxGainMasterwork = 9999999f;

		private float maxGainLegendary = 9999999f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (!(val <= 0f) || applyToNegativeValues)
			{
				float a = val * QualityMultiplier(req.QualityCategory) - val;
				a = Mathf.Min(a, MaxGain(req.QualityCategory));
				val += a;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && !applyToNegativeValues && req.Thing.GetStatValue(parentStat) <= 0f)
			{
				return null;
			}
			if (req.HasThing && req.Thing.TryGetQuality(out QualityCategory qc))
			{
				string text = "StatsReport_QualityMultiplier".Translate() + ": x" + QualityMultiplier(qc).ToStringPercent();
				float num = MaxGain(qc);
				if (num < 999999f)
				{
					text += "\n    (" + "StatsReport_MaxGain".Translate() + ": " + num.ToStringByStyle(parentStat.ToStringStyleUnfinalized, parentStat.toStringNumberSense) + ")";
				}
				return text;
			}
			return null;
		}

		private float QualityMultiplier(QualityCategory qc)
		{
			switch (qc)
			{
			case QualityCategory.Awful:
				return factorAwful;
			case QualityCategory.Poor:
				return factorPoor;
			case QualityCategory.Normal:
				return factorNormal;
			case QualityCategory.Good:
				return factorGood;
			case QualityCategory.Excellent:
				return factorExcellent;
			case QualityCategory.Masterwork:
				return factorMasterwork;
			case QualityCategory.Legendary:
				return factorLegendary;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private float MaxGain(QualityCategory qc)
		{
			switch (qc)
			{
			case QualityCategory.Awful:
				return maxGainAwful;
			case QualityCategory.Poor:
				return maxGainPoor;
			case QualityCategory.Normal:
				return maxGainNormal;
			case QualityCategory.Good:
				return maxGainGood;
			case QualityCategory.Excellent:
				return maxGainExcellent;
			case QualityCategory.Masterwork:
				return maxGainMasterwork;
			case QualityCategory.Legendary:
				return maxGainLegendary;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
