using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class PreceptComp_Thought : PreceptComp
{
	public ThoughtDef thought;

	[MustTranslate]
	public List<string> thoughtStageDescriptions;

	public bool tooltipShowMoodRange;

	public bool AffectsMood
	{
		get
		{
			for (int i = 0; i < thought.stages.Count; i++)
			{
				if (thought.stages[i].baseMoodEffect != 0f)
				{
					return true;
				}
			}
			return false;
		}
	}

	private string ParseDescription(string description, int thoughtStage = -1)
	{
		bool flag = thoughtStage != -1;
		if (!flag)
		{
			thoughtStage = 0;
		}
		if (!description.NullOrEmpty() && thought.Worker is IPreceptCompDescriptionArgs preceptCompDescriptionArgs)
		{
			description = description.Formatted(preceptCompDescriptionArgs.GetDescriptionArgs());
		}
		if (description.NullOrEmpty())
		{
			description = ((!thought.stages[thoughtStage].LabelAbstractCap.NullOrEmpty()) ? thought.stages[thoughtStage].LabelAbstractCap : thought.LabelCap.Resolve());
			description = description.Formatted(Gender.Male.Named("PAWN_gender"));
		}
		if (AffectsMood)
		{
			ThoughtStage thoughtStage2 = thought.stages[thoughtStage];
			if (thought.minExpectation != null)
			{
				description += " (" + "MinExpectation".Translate() + ": " + thought.minExpectation.LabelCap + ")";
			}
			else if (thought.minExpectationForNegativeThought != null && thoughtStage2.baseMoodEffect < 0f)
			{
				description += " (" + "MinExpectation".Translate() + ": " + thought.minExpectationForNegativeThought.LabelCap + ")";
			}
			string text = "";
			if (tooltipShowMoodRange && !flag)
			{
				float num = float.PositiveInfinity;
				float num2 = float.NegativeInfinity;
				for (int i = 0; i < thought.stages.Count; i++)
				{
					num = Mathf.Min(num, thought.stages[i].baseMoodEffect);
					num2 = Mathf.Max(num2, thought.stages[i].baseMoodEffect);
				}
				text = "PreceptThoughtMoodRange".Translate(num.ToStringWithSign("F0"), num2.ToStringWithSign("F0"));
			}
			else
			{
				text = thoughtStage2.baseMoodEffect.ToStringWithSign("F0");
			}
			return description + ": " + text;
		}
		return description + ": " + thought.stages[thoughtStage].baseOpinionOffset.ToStringWithSign("F0");
	}

	public override IEnumerable<string> GetDescriptions()
	{
		if (!thoughtStageDescriptions.NullOrEmpty())
		{
			int stage = 0;
			foreach (string thoughtStageDescription in thoughtStageDescriptions)
			{
				yield return ParseDescription(thoughtStageDescription, stage);
				stage++;
			}
			yield break;
		}
		foreach (string description in base.GetDescriptions())
		{
			yield return ParseDescription(description);
		}
	}
}
