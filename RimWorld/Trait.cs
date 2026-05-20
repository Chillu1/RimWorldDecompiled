using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Trait : IExposable
{
	public Pawn pawn;

	public Gene sourceGene;

	public Gene suppressedByGene;

	public TraitDef def;

	private int degree;

	private bool scenForced;

	[LoadAlias("suppressed")]
	public bool suppressedByTrait;

	private List<string> suppressedReasons = new List<string>();

	private static List<WorkTypeDef> tmpDisabledWorktypes = new List<WorkTypeDef>();

	public int Degree => degree;

	public TraitDegreeData CurrentData => def.DataAtDegree(degree);

	public string Label => CurrentData.GetLabelFor(pawn);

	public string LabelCap => CurrentData.GetLabelCapFor(pawn);

	public bool ScenForced => scenForced;

	public bool Suppressed
	{
		get
		{
			if (ModsConfig.BiotechActive)
			{
				if (suppressedByTrait || suppressedByGene != null)
				{
					return true;
				}
				if (sourceGene != null && sourceGene.Overridden && sourceGene.overriddenByGene.def != sourceGene.def)
				{
					return true;
				}
			}
			return false;
		}
	}

	public Trait()
	{
	}

	public Trait(TraitDef def, int degree = 0, bool forced = false)
	{
		this.def = def;
		this.degree = degree;
		scenForced = forced;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_References.Look(ref sourceGene, "sourceGene");
		Scribe_Values.Look(ref degree, "degree", 0);
		Scribe_Values.Look(ref scenForced, "scenForced", defaultValue: false);
		Scribe_Values.Look(ref suppressedByTrait, "suppressedByTrait", defaultValue: false);
		Scribe_References.Look(ref suppressedByGene, "suppressedBy");
		if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && def == null)
		{
			def = DefDatabase<TraitDef>.GetRandom();
			degree = PawnGenerator.RandomTraitDegree(def);
		}
	}

	public float OffsetOfStat(StatDef stat)
	{
		if (Suppressed)
		{
			return 0f;
		}
		float num = 0f;
		TraitDegreeData currentData = CurrentData;
		if (currentData.statOffsets != null)
		{
			for (int i = 0; i < currentData.statOffsets.Count; i++)
			{
				if (currentData.statOffsets[i].stat == stat)
				{
					num += currentData.statOffsets[i].value;
				}
			}
		}
		return num;
	}

	public float MultiplierOfStat(StatDef stat)
	{
		if (Suppressed)
		{
			return 1f;
		}
		float num = 1f;
		TraitDegreeData currentData = CurrentData;
		if (currentData.statFactors != null)
		{
			for (int i = 0; i < currentData.statFactors.Count; i++)
			{
				if (currentData.statFactors[i].stat == stat)
				{
					num *= currentData.statFactors[i].value;
				}
			}
		}
		return num;
	}

	public string TipString(Pawn pawn)
	{
		StringBuilder stringBuilder = new StringBuilder();
		suppressedReasons.Clear();
		TraitDegreeData currentData = CurrentData;
		stringBuilder.AppendLine(currentData.description.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn).Resolve());
		bool num = CurrentData.skillGains.Count > 0;
		bool flag = GetPermaThoughts().Any();
		bool flag2 = currentData.statOffsets != null;
		bool flag3 = currentData.statFactors != null;
		if (num || flag || flag2 || flag3 || !Mathf.Approximately(currentData.painFactor, 1f) || currentData.painOffset != 0f || !currentData.aptitudes.NullOrEmpty())
		{
			stringBuilder.AppendLine();
		}
		if (num)
		{
			foreach (SkillGain skillGain in CurrentData.skillGains)
			{
				if (skillGain.amount != 0)
				{
					stringBuilder.AppendLine("    " + skillGain.skill.skillLabel.CapitalizeFirst() + ":   " + skillGain.amount.ToString("+##;-##"));
				}
			}
		}
		if (flag)
		{
			foreach (ThoughtDef permaThought in GetPermaThoughts())
			{
				stringBuilder.AppendLine("    " + "PermanentMoodEffect".Translate() + " " + permaThought.stages[0].baseMoodEffect.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset));
			}
		}
		if (flag2)
		{
			for (int i = 0; i < currentData.statOffsets.Count; i++)
			{
				StatModifier statModifier = currentData.statOffsets[i];
				string valueToStringAsOffset = statModifier.ValueToStringAsOffset;
				string value = "    " + statModifier.stat.LabelCap + " " + valueToStringAsOffset;
				stringBuilder.AppendLine(value);
			}
		}
		if (currentData.painOffset != 0f)
		{
			stringBuilder.AppendLine("    " + "Pain".Translate() + " " + currentData.painOffset.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Offset));
		}
		if (flag3)
		{
			for (int j = 0; j < currentData.statFactors.Count; j++)
			{
				StatModifier statModifier2 = currentData.statFactors[j];
				string toStringAsFactor = statModifier2.ToStringAsFactor;
				string value2 = "    " + statModifier2.stat.LabelCap + " " + toStringAsFactor;
				stringBuilder.AppendLine(value2);
			}
		}
		if (!Mathf.Approximately(currentData.hungerRateFactor, 1f))
		{
			string text = currentData.hungerRateFactor.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Factor);
			string value3 = "    " + "HungerRate".Translate() + " " + text;
			stringBuilder.AppendLine(value3);
		}
		if (!Mathf.Approximately(currentData.painFactor, 1f))
		{
			stringBuilder.AppendLine("    " + "Pain".Translate() + " " + currentData.painFactor.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Factor));
		}
		if (!currentData.aptitudes.NullOrEmpty())
		{
			stringBuilder.AppendLine().AppendLineTagged(("Aptitudes".Translate().CapitalizeFirst() + ":").AsTipTitle());
			stringBuilder.AppendLine(currentData.aptitudes.Select((Aptitude x) => x.skill.LabelCap.ToString() + " " + x.level.ToStringWithSign()).ToLineList("  - ", capitalizeItems: true));
		}
		if (!currentData.enablesNeeds.NullOrEmpty())
		{
			stringBuilder.AppendLine().AppendLineTagged(("AddsNeeds".Translate().CapitalizeFirst() + ":").AsTipTitle());
			stringBuilder.AppendLine(currentData.enablesNeeds.Select((NeedDef x) => x.LabelCap.ToString()).ToLineList("  - ", capitalizeItems: true));
		}
		if (!currentData.disablesNeeds.NullOrEmpty())
		{
			stringBuilder.AppendLine().AppendLineTagged(("DisablesNeeds".Translate().CapitalizeFirst() + ":").AsTipTitle());
			stringBuilder.AppendLine(currentData.disablesNeeds.Select((NeedDef x) => x.LabelCap.ToString()).ToLineList("  - ", capitalizeItems: true));
		}
		if (ModsConfig.RoyaltyActive)
		{
			List<MeditationFocusDef> allowedMeditationFocusTypes = CurrentData.allowedMeditationFocusTypes;
			if (!allowedMeditationFocusTypes.NullOrEmpty())
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("EnablesMeditationFocusType".Translate().Colorize(ColoredText.TipSectionTitleColor) + ":\n" + allowedMeditationFocusTypes.Select((MeditationFocusDef f) => f.LabelCap.Resolve()).ToLineList("  - "));
			}
		}
		if (ModsConfig.IdeologyActive)
		{
			List<IssueDef> affectedIssues = CurrentData.GetAffectedIssues(def);
			if (affectedIssues.Count != 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("OverridesSomePrecepts".Translate().Colorize(ColoredText.TipSectionTitleColor) + ":\n" + affectedIssues.Select((IssueDef x) => x.LabelCap.Resolve()).ToLineList("  - "));
			}
			List<MemeDef> affectedMemes = CurrentData.GetAffectedMemes(def, agreeable: true);
			if (affectedMemes.Count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("AgreeableMemes".Translate().Colorize(ColoredText.TipSectionTitleColor) + ":\n" + affectedMemes.Select((MemeDef x) => x.LabelCap.Resolve()).ToLineList("  - "));
			}
			List<MemeDef> affectedMemes2 = CurrentData.GetAffectedMemes(def, agreeable: false);
			if (affectedMemes2.Count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("DisagreeableMemes".Translate().Colorize(ColoredText.TipSectionTitleColor) + ":\n" + affectedMemes2.Select((MemeDef x) => x.LabelCap.Resolve()).ToLineList("  - "));
			}
		}
		if (ModsConfig.BiotechActive)
		{
			if (sourceGene != null)
			{
				stringBuilder.AppendLine().AppendLine(("AddedByGene".Translate() + ": " + sourceGene.LabelCap).Colorize(ColoredText.GeneColor));
			}
			if (Suppressed)
			{
				foreach (Trait allTrait in pawn.story.traits.allTraits)
				{
					if (allTrait != this && allTrait.def.CanSuppress(this))
					{
						suppressedReasons.Add(string.Format("{0} ({1})", allTrait.Label, "Trait".Translate()));
					}
				}
				if (sourceGene != null && sourceGene.Overridden && sourceGene.overriddenByGene.def != sourceGene.def)
				{
					suppressedReasons.Add(string.Format("{0} ({1})", sourceGene.overriddenByGene.Label, "Gene".Translate()));
				}
				if (suppressedByGene != null)
				{
					suppressedReasons.Add(string.Format("{0} ({1})", suppressedByGene.Label, "Gene".Translate()));
				}
				if (suppressedReasons.Any())
				{
					stringBuilder.AppendLine().AppendLine(("Suppressed".Translate().CapitalizeFirst() + ": " + "SuppressedDesc".Translate() + ":\n" + suppressedReasons.ToLineList("  - ", capitalizeItems: true)).Colorize(ColoredText.SubtleGrayColor));
				}
			}
		}
		if (stringBuilder.Length > 0)
		{
			if (stringBuilder[stringBuilder.Length - 1] == '\n')
			{
				if (stringBuilder.Length > 1)
				{
					if (stringBuilder[stringBuilder.Length - 2] == '\r')
					{
						stringBuilder.Remove(stringBuilder.Length - 2, 2);
						goto IL_0919;
					}
				}
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
		}
		goto IL_0919;
		IL_0919:
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		if (def == null)
		{
			return "Trait(null)";
		}
		return "Trait(" + def.ToString() + "-" + degree + ")";
	}

	private IEnumerable<ThoughtDef> GetPermaThoughts()
	{
		TraitDegreeData degree = CurrentData;
		List<ThoughtDef> allThoughts = DefDatabase<ThoughtDef>.AllDefsListForReading;
		for (int i = 0; i < allThoughts.Count; i++)
		{
			if (allThoughts[i].IsSituational && allThoughts[i].Worker is ThoughtWorker_AlwaysActive && allThoughts[i].requiredTraits != null && allThoughts[i].requiredTraits.Contains(def) && (!allThoughts[i].RequiresSpecificTraitsDegree || allThoughts[i].requiredTraitsDegree == degree.degree))
			{
				yield return allThoughts[i];
			}
		}
	}

	private bool AllowsWorkType(WorkTypeDef workDef)
	{
		return (def.disabledWorkTags & workDef.workTags) == 0;
	}

	public void Notify_MentalStateEndedOn(Pawn pawn, bool causedByMood)
	{
		if (causedByMood)
		{
			Notify_MentalStateEndedOn(pawn);
		}
	}

	public void Notify_MentalStateEndedOn(Pawn pawn)
	{
		TraitDegreeData currentData = CurrentData;
		if (!currentData.mentalBreakInspirationGainSet.NullOrEmpty() && !(Rand.Value > currentData.mentalBreakInspirationGainChance))
		{
			pawn.mindState.inspirationHandler.TryStartInspiration(currentData.mentalBreakInspirationGainSet.RandomElement(), currentData.mentalBreakInspirationGainReasonText);
		}
	}

	public List<WorkTypeDef> GetDisabledWorkTypes()
	{
		tmpDisabledWorktypes.Clear();
		for (int i = 0; i < def.disabledWorkTypes.Count; i++)
		{
			tmpDisabledWorktypes.Add(def.disabledWorkTypes[i]);
		}
		List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
		for (int j = 0; j < allDefsListForReading.Count; j++)
		{
			WorkTypeDef workTypeDef = allDefsListForReading[j];
			if (!AllowsWorkType(workTypeDef))
			{
				tmpDisabledWorktypes.Add(workTypeDef);
			}
		}
		return tmpDisabledWorktypes;
	}
}
