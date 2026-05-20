using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class SkillUI
{
	public enum SkillDrawMode : byte
	{
		Gameplay,
		Menu
	}

	private static float levelLabelWidth = -1f;

	private static List<SkillDef> skillDefsInListOrderCached;

	private const float SkillWidth = 230f;

	public const float SkillHeight = 24f;

	public const float SkillYSpacing = 3f;

	private const float LeftEdgeMargin = 6f;

	private const float IncButX = 205f;

	private const float IncButSpacing = 10f;

	private static readonly Color DisabledSkillColor = new Color(1f, 1f, 1f, 0.5f);

	public static Texture2D PassionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor");

	public static Texture2D PassionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor");

	private static Texture2D SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

	private static Texture2D SkillBarAptitudePositiveTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.8f, 1f, 0.6f, 0.25f));

	private static Texture2D SkillBarAptitudeNegativeTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.5f, 0.6f, 0.25f));

	public static void Reset()
	{
		skillDefsInListOrderCached = DefDatabase<SkillDef>.AllDefs.OrderByDescending((SkillDef sd) => sd.listOrder).ToList();
	}

	public static void DrawSkillsOf(Pawn p, Vector2 offset, SkillDrawMode mode, Rect container)
	{
		Text.Font = GameFont.Small;
		if (p.DevelopmentalStage.Baby())
		{
			Color color = GUI.color;
			GUI.color = Color.gray;
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(new Rect(offset.x, offset.y, 230f, container.height), "SkillsDevelopLaterBaby".Translate());
			GUI.color = color;
			Text.Anchor = anchor;
			return;
		}
		List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			float x = Text.CalcSize(allDefsListForReading[i].skillLabel.CapitalizeFirst()).x;
			if (x > levelLabelWidth)
			{
				levelLabelWidth = x;
			}
		}
		for (int j = 0; j < skillDefsInListOrderCached.Count; j++)
		{
			SkillDef skillDef = skillDefsInListOrderCached[j];
			float y = (float)j * 27f + offset.y;
			DrawSkill(p.skills.GetSkill(skillDef), new Vector2(offset.x, y), mode);
		}
	}

	public static void DrawSkill(SkillRecord skill, Vector2 topLeft, SkillDrawMode mode, string tooltipPrefix = "")
	{
		DrawSkill(skill, new Rect(topLeft.x, topLeft.y, 230f, 24f), mode);
	}

	public static void DrawSkill(SkillRecord skill, Rect holdingRect, SkillDrawMode mode, string tooltipPrefix = "")
	{
		if (Mouse.IsOver(holdingRect))
		{
			GUI.DrawTexture(holdingRect, TexUI.HighlightTex);
		}
		Widgets.BeginGroup(holdingRect);
		Text.Anchor = TextAnchor.MiddleLeft;
		Rect rect = new Rect(6f, 0f, levelLabelWidth + 6f, holdingRect.height);
		Widgets.Label(rect, skill.def.skillLabel.CapitalizeFirst());
		int level = skill.GetLevel();
		Rect position = new Rect(rect.xMax, 0f, 24f, 24f);
		if (!skill.TotallyDisabled)
		{
			if ((int)skill.passion > 0)
			{
				Texture2D image = ((skill.passion == Passion.Major) ? PassionMajorIcon : PassionMinorIcon);
				GUI.DrawTexture(position, image);
			}
			Rect rect2 = new Rect(position.xMax, 0f, holdingRect.width - position.xMax, holdingRect.height);
			float fillPercent = Mathf.Max(0.01f, (float)level / 20f);
			Texture2D fillTex = SkillBarFillTex;
			if ((ModsConfig.BiotechActive || ModsConfig.AnomalyActive) && skill.Aptitude != 0)
			{
				fillTex = ((skill.Aptitude > 0) ? SkillBarAptitudePositiveTex : SkillBarAptitudeNegativeTex);
			}
			Widgets.FillableBar(rect2, fillPercent, fillTex, null, doBorder: false);
		}
		Rect rect3 = new Rect(position.xMax + 4f, 0f, 999f, holdingRect.height);
		rect3.yMin += 3f;
		string label;
		if (skill.TotallyDisabled)
		{
			GUI.color = DisabledSkillColor;
			label = "-";
		}
		else
		{
			if ((ModsConfig.BiotechActive || ModsConfig.AnomalyActive) && level == 0 && skill.Aptitude != 0)
			{
				GUI.color = ((skill.Aptitude > 0) ? ColorLibrary.BrightGreen : ColorLibrary.RedReadable);
			}
			label = level.ToStringCached();
		}
		GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
		Widgets.Label(rect3, label);
		GenUI.ResetLabelAlign();
		GUI.color = Color.white;
		Widgets.EndGroup();
		if (Mouse.IsOver(holdingRect))
		{
			string text = GetSkillDescription(skill);
			if (tooltipPrefix != "")
			{
				text = tooltipPrefix + "\n\n" + text;
			}
			TooltipHandler.TipRegion(holdingRect, new TipSignal(text, skill.def.GetHashCode() * 397945));
		}
	}

	private static string GetSkillDescription(SkillRecord sk)
	{
		StringBuilder stringBuilder = new StringBuilder();
		TaggedString taggedString = sk.def.LabelCap.AsTipTitle();
		if (sk.TotallyDisabled)
		{
			taggedString += " (" + "DisabledLower".Translate() + ")";
		}
		stringBuilder.AppendLineTagged(taggedString);
		stringBuilder.AppendLineTagged(sk.def.description.Colorize(ColoredText.SubtleGrayColor)).AppendLine();
		stringBuilder.AppendLineTagged(("SkillLevel".Translate().CapitalizeFirst() + ": ").AsTipTitle() + sk.GetLevelForUI() + " - " + sk.LevelDescriptor);
		if (!sk.PermanentlyDisabled && sk.Aptitude != 0)
		{
			stringBuilder.AppendLine(string.Concat("  - " + "LearnedLevel".Translate() + ": ", sk.GetLevel(includeAptitudes: false).ToString()));
			if (ModsConfig.BiotechActive && sk.Pawn.genes != null)
			{
				foreach (Gene item in sk.Pawn.genes.GenesListForReading)
				{
					if (item.Active)
					{
						int num = item.def.AptitudeFor(sk.def);
						if (num != 0)
						{
							stringBuilder.AppendLine(string.Format("  - {0}: {1}", "GeneLabelWithDesc".Translate(item.def.Named("GENE")).CapitalizeFirst(), num.ToStringWithSign()));
						}
					}
				}
			}
			if (ModsConfig.AnomalyActive)
			{
				if (sk.Pawn.story?.traits != null)
				{
					foreach (Trait allTrait in sk.Pawn.story.traits.allTraits)
					{
						if (!allTrait.Suppressed)
						{
							int num2 = allTrait.CurrentData.AptitudeFor(sk.def);
							if (num2 != 0)
							{
								stringBuilder.AppendLine(string.Format("  - {0}: {1}", "TraitLabelWithDesc".Translate(allTrait.CurrentData.GetLabelFor(sk.Pawn).Named("TRAITLABEL")).CapitalizeFirst(), num2.ToStringWithSign()));
							}
						}
					}
				}
				if (sk.Pawn.health.hediffSet != null)
				{
					foreach (Hediff hediff in sk.Pawn.health.hediffSet.hediffs)
					{
						int num3 = hediff.def.AptitudeFor(sk.def);
						if (num3 != 0)
						{
							stringBuilder.AppendLine("  - " + hediff.LabelCap + ": " + num3.ToStringWithSign());
						}
					}
				}
			}
			int unclampedLevel = sk.GetUnclampedLevel();
			if (unclampedLevel < 0)
			{
				stringBuilder.AppendLine("  - " + "NegativeLevelAdjustedToZero".Translate());
			}
			else if (unclampedLevel > 20)
			{
				stringBuilder.AppendLine("  - " + "LevelCappedAtMax".Translate(20));
			}
		}
		if (Current.ProgramState == ProgramState.Playing)
		{
			stringBuilder.AppendLine();
			string text = ((sk.GetLevel(includeAptitudes: false) == 20) ? "Experience".Translate() : "ProgressToNextLevel".Translate());
			if (sk.PermanentlyDisabled)
			{
				stringBuilder.AppendLineTagged((text + ": ").AsTipTitle() + "0 / " + SkillRecord.XpRequiredToLevelUpFrom(0));
			}
			else
			{
				stringBuilder.AppendLineTagged((text + ": ").AsTipTitle() + sk.xpSinceLastLevel.ToString("F0") + " / " + sk.XpRequiredForLevelUp);
			}
		}
		if (!sk.TotallyDisabled)
		{
			if (sk.LearningSaturatedToday)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("LearnedMaxToday".Translate(sk.xpSinceMidnight.ToString("F0"), 4000, 0.2f.ToStringPercent("F0")));
			}
			float statValue = sk.Pawn.GetStatValue(StatDefOf.GlobalLearningFactor);
			float num4 = 1f;
			float num5 = statValue * sk.passion.GetLearningFactor();
			if (sk.def == SkillDefOf.Animals)
			{
				num4 = sk.Pawn.GetStatValue(StatDefOf.AnimalsLearningFactor);
				num5 *= num4;
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLineTagged(("LearningSpeed".Translate() + ": ").AsTipTitle() + num5.ToStringPercent());
			stringBuilder.AppendLine("  - " + "StatsReport_BaseValue".Translate() + ": " + 1f.ToStringPercent());
			stringBuilder.AppendLine("  - " + sk.passion.GetLabel() + ": x" + sk.passion.GetLearningFactor().ToStringPercent("F0"));
			if (statValue != 1f)
			{
				stringBuilder.AppendLine("  - " + StatDefOf.GlobalLearningFactor.LabelCap + ": x" + statValue.ToStringPercent());
			}
			if (Math.Abs(num4 - 1f) > float.Epsilon)
			{
				stringBuilder.AppendLine("  - " + StatDefOf.AnimalsLearningFactor.LabelCap + ": x" + num4.ToStringPercent());
			}
			if (statValue != 1f)
			{
				float baseValueFor = StatDefOf.GlobalLearningFactor.Worker.GetBaseValueFor(StatRequest.For(sk.Pawn));
				stringBuilder.AppendLine();
				stringBuilder.AppendLineTagged((StatDefOf.GlobalLearningFactor.LabelCap + ": ").AsTipTitle() + statValue.ToStringPercent());
				stringBuilder.AppendLine("  - " + "StatsReport_BaseValue".Translate() + ": " + baseValueFor.ToStringPercent());
				ListGlobalLearningSpeedOffsets(sk, stringBuilder);
				ListGlobalLearningSpeedFactors(sk, stringBuilder);
			}
			if (ModsConfig.BiotechActive && sk.Pawn.DevelopmentalStage.Child())
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLineTagged("ChildrenLearn".Translate().Colorize(ColoredText.SubtleGrayColor));
			}
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	private static void ListGlobalLearningSpeedOffsets(SkillRecord sk, StringBuilder sb)
	{
		foreach (Hediff hediff in sk.Pawn.health.hediffSet.hediffs)
		{
			if (!hediff.Visible)
			{
				continue;
			}
			HediffStage curStage = hediff.CurStage;
			if (curStage != null)
			{
				float statOffsetFromList = curStage.statOffsets.GetStatOffsetFromList(StatDefOf.GlobalLearningFactor);
				if (statOffsetFromList != 0f)
				{
					sb.AppendLine("  - " + hediff.LabelCap + ": " + ((statOffsetFromList >= 0f) ? "+" : "") + statOffsetFromList.ToStringPercent());
				}
			}
		}
		if (sk.Pawn.story?.traits != null)
		{
			foreach (Trait allTrait in sk.Pawn.story.traits.allTraits)
			{
				if (!allTrait.Suppressed)
				{
					float statOffsetFromList2 = allTrait.CurrentData.statOffsets.GetStatOffsetFromList(StatDefOf.GlobalLearningFactor);
					if (statOffsetFromList2 != 0f)
					{
						sb.AppendLine("  - " + allTrait.CurrentData.LabelCap + ": " + ((statOffsetFromList2 >= 0f) ? "+" : "") + statOffsetFromList2.ToStringPercent());
					}
				}
			}
		}
		if (!ModsConfig.BiotechActive || sk.Pawn.genes == null)
		{
			return;
		}
		foreach (Gene item in sk.Pawn.genes.GenesListForReading)
		{
			if (item.Active)
			{
				float statOffsetFromList3 = item.def.statOffsets.GetStatOffsetFromList(StatDefOf.GlobalLearningFactor);
				if (statOffsetFromList3 != 0f)
				{
					sb.AppendLine("  - " + item.def.LabelCap + ": " + ((statOffsetFromList3 >= 0f) ? "+" : "") + statOffsetFromList3.ToStringPercent());
				}
			}
		}
	}

	private static void ListGlobalLearningSpeedFactors(SkillRecord sk, StringBuilder sb)
	{
		foreach (Hediff hediff in sk.Pawn.health.hediffSet.hediffs)
		{
			if (!hediff.Visible)
			{
				continue;
			}
			HediffStage curStage = hediff.CurStage;
			if (curStage != null)
			{
				float statFactorFromList = curStage.statFactors.GetStatFactorFromList(StatDefOf.GlobalLearningFactor);
				if (!Mathf.Approximately(statFactorFromList, 1f))
				{
					sb.AppendLine("  - " + hediff.LabelCap + ": x" + statFactorFromList.ToStringPercent());
				}
			}
		}
		if (sk.Pawn.story?.traits != null)
		{
			foreach (Trait allTrait in sk.Pawn.story.traits.allTraits)
			{
				if (!allTrait.Suppressed)
				{
					float statFactorFromList2 = allTrait.CurrentData.statFactors.GetStatFactorFromList(StatDefOf.GlobalLearningFactor);
					if (!Mathf.Approximately(statFactorFromList2, 1f))
					{
						sb.AppendLine("  - " + allTrait.CurrentData.LabelCap + ": x" + statFactorFromList2.ToStringPercent());
					}
				}
			}
		}
		if (!ModsConfig.BiotechActive || sk.Pawn.genes == null)
		{
			return;
		}
		foreach (Gene item in sk.Pawn.genes.GenesListForReading)
		{
			if (item.Active)
			{
				float statFactorFromList3 = item.def.statFactors.GetStatFactorFromList(StatDefOf.GlobalLearningFactor);
				if (!Mathf.Approximately(statFactorFromList3, 1f))
				{
					sb.AppendLine("  - " + item.def.LabelCap + ": x" + statFactorFromList3.ToStringPercent());
				}
			}
		}
	}

	public static string GetLabel(this Passion passion)
	{
		switch (passion)
		{
		case Passion.None:
			return "PassionNone".Translate();
		case Passion.Minor:
			return "PassionMinor".Translate();
		case Passion.Major:
			return "PassionMajor".Translate();
		default:
			Log.Error("Unknown passion type.");
			return string.Empty;
		}
	}

	public static float GetLearningFactor(this Passion passion)
	{
		switch (passion)
		{
		case Passion.None:
			return 0.35f;
		case Passion.Minor:
			return 1f;
		case Passion.Major:
			return 1.5f;
		default:
			Log.Error("Unknown passion type.");
			return 1f;
		}
	}
}
