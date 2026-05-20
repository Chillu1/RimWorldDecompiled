using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class ReadingOutcomeDoerGainResearch : BookOutcomeDoer
{
	protected Dictionary<ResearchProjectDef, float> values = new Dictionary<ResearchProjectDef, float>();

	private const float MultipleProjectFactor = 1.25f;

	public new BookOutcomeProperties_GainResearch Props => (BookOutcomeProperties_GainResearch)props;

	public virtual int RoundTo { get; } = 10;

	private bool IsValid(ResearchProjectDef project)
	{
		if (project.generalRules == null)
		{
			return false;
		}
		if (Props.exclude.Count > 0 && Props.exclude.ContainsAny((BookOutcomeProperties_GainResearch.BookResearchItem i) => i.project == project))
		{
			return false;
		}
		if ((!Props.ignoreZeroBaseCost || project.baseCost != 0f) && (Props.tabs.NullOrEmpty() || Props.tabs.Exists((BookOutcomeProperties_GainResearch.BookTabItem x) => x.tab == project.tab)))
		{
			return project.TechprintCount == 0;
		}
		return false;
	}

	public override bool BenefitDetailsCanChange(Pawn reader = null)
	{
		foreach (var (researchProjectDef2, _) in values)
		{
			if (researchProjectDef2.IsHidden)
			{
				return true;
			}
			if (!researchProjectDef2.IsFinished)
			{
				return true;
			}
		}
		return false;
	}

	public override void OnBookGenerated(Pawn author = null)
	{
		List<ResearchProjectDef> allDefsListForReading = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
		List<ResearchProjectDef> list = new List<ResearchProjectDef>(allDefsListForReading.Count);
		if (Props.include.Count > 0)
		{
			for (int i = 0; i < Props.include.Count; i++)
			{
				ResearchProjectDef project = Props.include[i].project;
				if (!project.IsFinished && IsValid(project))
				{
					list.Add(project);
				}
			}
		}
		else
		{
			for (int j = 0; j < allDefsListForReading.Count; j++)
			{
				if (allDefsListForReading[j].PrerequisitesCompleted && !allDefsListForReading[j].IsFinished && IsValid(allDefsListForReading[j]))
				{
					list.Add(allDefsListForReading[j]);
				}
			}
		}
		if (list.Count == 0)
		{
			for (int k = 0; k < allDefsListForReading.Count; k++)
			{
				if (IsValid(allDefsListForReading[k]))
				{
					list.Add(allDefsListForReading[k]);
				}
			}
		}
		if (list.Empty())
		{
			Log.ErrorOnce("Research book had no valid projects and failed to find any backup projects.\n" + $"ignoring zero base cost: {Props.ignoreZeroBaseCost}).\n" + "tabs: " + Props.tabs.Select((BookOutcomeProperties_GainResearch.BookTabItem x) => x.tab.generalTitle).ToCommaList() + ".\nexcludes: " + (Props.exclude.Empty() ? "(none)" : Props.exclude.Select((BookOutcomeProperties_GainResearch.BookResearchItem x) => x.project.label).ToCommaList()), 15242436);
			return;
		}
		int count = ((!Rand.Chance(0.25f)) ? 1 : 2);
		List<ResearchProjectDef> list2 = list.TakeRandomDistinct(count);
		float num = GetBaseValue();
		if (list2.Count > 1)
		{
			num *= 1.25f;
		}
		float value = num / (float)list2.Count;
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			values[list2[num2]] = value;
		}
	}

	public override void Reset()
	{
		values.Clear();
	}

	public override bool DoesProvidesOutcome(Pawn reader)
	{
		foreach (KeyValuePair<ResearchProjectDef, float> value2 in values)
		{
			value2.Deconstruct(out var key, out var _);
			if (!key.IsFinished)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual float GetBaseValue()
	{
		return BookUtility.GetResearchExpForQuality(base.Quality);
	}

	public override void OnReadingTick(Pawn reader, float factor)
	{
		foreach (var (researchProjectDef2, num2) in values)
		{
			if (IsProjectVisible(researchProjectDef2) && !researchProjectDef2.IsFinished)
			{
				Find.ResearchManager.AddProgress(researchProjectDef2, num2 * factor);
			}
		}
	}

	public override IEnumerable<Dialog_InfoCard.Hyperlink> GetHyperlinks()
	{
		foreach (var (researchProjectDef2, _) in values)
		{
			if (IsProjectVisible(researchProjectDef2))
			{
				yield return new Dialog_InfoCard.Hyperlink(researchProjectDef2);
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
		foreach (KeyValuePair<ResearchProjectDef, float> value2 in values)
		{
			value2.Deconstruct(out var key, out var value);
			ResearchProjectDef researchProjectDef = key;
			float num = value * 2500f;
			if (RoundTo != 0)
			{
				num = Mathf.Round(num / (float)RoundTo) * (float)RoundTo;
			}
			string text = string.Format("{0}: {1}", researchProjectDef.LabelCap, "PerHour".Translate(num.ToStringDecimalIfSmall()));
			if (researchProjectDef.IsFinished)
			{
				text += string.Format(" ({0})", "AlreadyUnlocked".Translate());
				text = text.Colorize(Color.gray);
			}
			else if (!IsProjectVisible(researchProjectDef))
			{
				text += string.Format(" ({0})", "WhenDiscovered".Translate());
				text = text.Colorize(Color.gray);
			}
			stringBuilder.AppendLine(" - " + text);
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	private bool IsProjectVisible(ResearchProjectDef project)
	{
		if (Props.usesHiddenProjects)
		{
			return project.CanStartNow;
		}
		return true;
	}

	public override IEnumerable<RulePack> GetTopicRulePacks()
	{
		return values.Keys.Select((ResearchProjectDef x) => x.generalRules);
	}

	public override void PostExposeData()
	{
		Scribe_Collections.Look(ref values, "values");
	}
}
