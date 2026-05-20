using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class BookOutcomeDoerGainSkillExp : BookOutcomeDoer
{
	private Dictionary<SkillDef, float> values = new Dictionary<SkillDef, float>();

	private const float MultipleSkillFactor = 1.25f;

	private static readonly SimpleCurve QualityMaxLevel = new SimpleCurve
	{
		new CurvePoint(0f, 2f),
		new CurvePoint(1f, 5f),
		new CurvePoint(2f, 8f),
		new CurvePoint(3f, 10f),
		new CurvePoint(4f, 12f),
		new CurvePoint(5f, 14f),
		new CurvePoint(6f, 16f)
	};

	public new BookOutcomeProperties_GainSkillExp Props => (BookOutcomeProperties_GainSkillExp)props;

	public IReadOnlyDictionary<SkillDef, float> Values => values;

	public override bool DoesProvidesOutcome(Pawn reader)
	{
		foreach (var (skill, _) in values)
		{
			if (CanProgressSkill(reader, skill, base.Quality))
			{
				return true;
			}
		}
		return false;
	}

	public override void OnBookGenerated(Pawn author = null)
	{
		List<SkillDef> availableSkills = GetAvailableSkills();
		int count = ((!Rand.Chance(0.25f)) ? 1 : 2);
		List<SkillDef> list = availableSkills.TakeRandomDistinct(count);
		float num = BookUtility.GetSkillExpForQuality(base.Quality);
		if (list.Count > 1)
		{
			num *= 1.25f;
		}
		float value = num / (float)list.Count;
		for (int i = 0; i < list.Count; i++)
		{
			values[list[i]] = value;
		}
	}

	protected virtual List<SkillDef> GetAvailableSkills(Pawn author = null)
	{
		List<SkillDef> list = new List<SkillDef>();
		if (Props.skills.Count > 0)
		{
			foreach (BookOutcomeProperties_GainSkillExp.BookStatReward skill in Props.skills)
			{
				list.Add(skill.skill);
			}
		}
		else
		{
			list = DefDatabase<SkillDef>.AllDefsListForReading;
		}
		return list;
	}

	public override void Reset()
	{
		values.Clear();
	}

	public override void OnReadingTick(Pawn reader, float factor)
	{
		foreach (var (skillDef2, num2) in values)
		{
			if (CanProgressSkill(reader, skillDef2, base.Quality))
			{
				reader.skills.GetSkill(skillDef2).Learn(num2 * factor);
			}
		}
	}

	public override string GetBenefitsString(Pawn reader = null)
	{
		if (values.Count == 0)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<SkillDef, float> value2 in values)
		{
			value2.Deconstruct(out var key, out var value);
			SkillDef skillDef = key;
			float num = value;
			float num2 = 1f;
			if (reader != null)
			{
				num2 = reader.skills.GetSkill(skillDef).LearnRateFactor();
				num *= num2;
			}
			float f = num * 2500f;
			string text = string.Format("{0}: {1}", skillDef.LabelCap, "XpPerHour".Translate(f.ToStringDecimalIfSmall()));
			int maxSkillLevel = GetMaxSkillLevel(base.Quality);
			text += string.Format(" ({0})", "BookMaxLevel".Translate(maxSkillLevel));
			if (!Mathf.Approximately(num2, 1f))
			{
				text += string.Format(" (x{0} {1})", num2.ToStringPercent("0"), "BookLearningModifier".Translate());
			}
			stringBuilder.AppendLine(" - " + text);
		}
		return stringBuilder.ToString();
	}

	public override IEnumerable<RulePack> GetTopicRulePacks()
	{
		return values.Keys.Select((SkillDef x) => x.generalRules);
	}

	public override void PostExposeData()
	{
		Scribe_Collections.Look(ref values, "values");
	}

	private static bool CanProgressSkill(Pawn pawn, SkillDef skill, QualityCategory quality)
	{
		if (pawn.skills.GetSkill(skill).TotallyDisabled)
		{
			return false;
		}
		return pawn.skills.GetSkill(skill).GetLevel(includeAptitudes: false) < GetMaxSkillLevel(quality);
	}

	private static int GetMaxSkillLevel(QualityCategory quality)
	{
		return Mathf.RoundToInt(QualityMaxLevel.Evaluate((int)quality));
	}
}
