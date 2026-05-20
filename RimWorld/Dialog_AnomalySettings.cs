using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_AnomalySettings : Window
{
	private Difficulty difficulty;

	private float anomalyThreatsInactiveFraction;

	private float anomalyThreatsActiveFraction;

	private float studyEfficiencyFactor;

	private float overrideAnomalyThreatsFraction;

	private AnomalyPlaystyleDef anomalyPlaystyleDef;

	private Vector2 scrollPosition;

	private float scrollHeight;

	private Listing_Standard listing;

	private static readonly Vector2 AcceptButtonSize = new Vector2(100f, 35f);

	private const float DefaultOverrideThreatFraction = 0.15f;

	private const float MaxStudyEfficiencyFactor = 5f;

	private static (float, string)[] FrequencyLabels = new(float, string)[6]
	{
		(0f, "AnomalyFrequency_None"),
		(0.05f, "AnomalyFrequency_VeryRare"),
		(0.2f, "AnomalyFrequency_Rare"),
		(0.5f, "AnomalyFrequency_Balanced"),
		(0.75f, "AnomalyFrequency_Intense"),
		(1f, "AnomalyFrequency_Overwhelming")
	};

	public override Vector2 InitialSize => new Vector2(500f, 475f);

	public Dialog_AnomalySettings(Difficulty difficulty)
	{
		doCloseX = true;
		absorbInputAroundWindow = true;
		listing = new Listing_Standard();
		listing.maxOneColumn = true;
		this.difficulty = difficulty;
		anomalyThreatsInactiveFraction = difficulty.anomalyThreatsInactiveFraction;
		anomalyThreatsActiveFraction = difficulty.anomalyThreatsActiveFraction;
		overrideAnomalyThreatsFraction = difficulty.overrideAnomalyThreatsFraction.GetValueOrDefault();
		studyEfficiencyFactor = difficulty.studyEfficiencyFactor;
		anomalyPlaystyleDef = difficulty.AnomalyPlaystyleDef;
	}

	public override void DoWindowContents(Rect inRect)
	{
		using (new TextBlock(GameFont.Medium))
		{
			Widgets.Label(inRect, "AnomalySettings".Translate());
			inRect.yMin += Text.LineHeight + 10f;
		}
		Text.Font = GameFont.Small;
		if (Widgets.ButtonText(new Rect((inRect.x + inRect.width - AcceptButtonSize.x) / 2f, inRect.yMax - AcceptButtonSize.y, AcceptButtonSize.x, AcceptButtonSize.y), "Accept".Translate()))
		{
			if (anomalyPlaystyleDef.overrideThreatFraction)
			{
				difficulty.overrideAnomalyThreatsFraction = overrideAnomalyThreatsFraction;
			}
			else
			{
				difficulty.overrideAnomalyThreatsFraction = null;
			}
			difficulty.anomalyThreatsInactiveFraction = anomalyThreatsInactiveFraction;
			difficulty.anomalyThreatsActiveFraction = anomalyThreatsActiveFraction;
			difficulty.studyEfficiencyFactor = studyEfficiencyFactor;
			difficulty.AnomalyPlaystyleDef = anomalyPlaystyleDef;
			Close();
		}
		inRect.yMax -= AcceptButtonSize.y + 17f;
		Rect rect = new Rect(inRect.x, inRect.yMax - Text.LineHeight, inRect.width, Text.LineHeight);
		rect.xMin += 10f;
		rect.xMax -= 10f;
		rect.height = Text.LineHeight + 5f;
		if (Widgets.ButtonText(rect, "SetToStandardPlaystyle".Translate() + "..."))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (DifficultyDef d in DefDatabase<DifficultyDef>.AllDefs)
			{
				if (!d.isCustom)
				{
					list.Add(new FloatMenuOption(d.LabelCap, delegate
					{
						anomalyThreatsInactiveFraction = d.anomalyThreatsInactiveFraction;
						anomalyThreatsActiveFraction = d.anomalyThreatsActiveFraction;
						studyEfficiencyFactor = d.studyEfficiencyFactor;
						anomalyPlaystyleDef = AnomalyPlaystyleDefOf.Standard;
					}));
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		inRect.yMax -= rect.height + 4f;
		Rect rect2 = new Rect(0f, 0f, inRect.width - 16f, scrollHeight);
		Widgets.BeginScrollView(inRect, ref scrollPosition, rect2);
		Rect rect3 = rect2;
		rect3.height = 99999f;
		listing.Begin(rect3);
		DrawPlaystyles();
		DrawExtraSettings();
		scrollHeight = listing.CurHeight;
		listing.End();
		Widgets.EndScrollView();
	}

	private void DrawPlaystyles()
	{
		listing.Label("ChooseAnomalyPlaystyle".Translate());
		listing.Gap();
		foreach (AnomalyPlaystyleDef allDef in DefDatabase<AnomalyPlaystyleDef>.AllDefs)
		{
			bool flag = !Find.Scenario.standardAnomalyPlaystyleOnly || allDef == AnomalyPlaystyleDefOf.Standard;
			string text = allDef.LabelCap.AsTipTitle() + "\n" + allDef.description;
			if (!flag)
			{
				text = text + "\n\n" + ("DisabledByScenario".Translate() + ": " + Find.Scenario.name).Colorize(ColorLibrary.RedReadable);
			}
			if (listing.RadioButton(allDef.LabelCap, anomalyPlaystyleDef == allDef, 30f, 80f, text, 0f, !flag))
			{
				if (flag)
				{
					if (!anomalyPlaystyleDef.overrideThreatFraction && allDef.overrideThreatFraction)
					{
						overrideAnomalyThreatsFraction = 0.15f;
					}
					anomalyPlaystyleDef = allDef;
				}
				else
				{
					Messages.Message("DisabledByScenario".Translate() + ": " + Find.Scenario.name, MessageTypeDefOf.RejectInput, historical: false);
				}
			}
			listing.Gap(3f);
		}
		listing.Gap();
	}

	private void DrawExtraSettings()
	{
		if (anomalyPlaystyleDef.displayThreatFractionSliders || anomalyPlaystyleDef.overrideThreatFraction || anomalyPlaystyleDef.displayThreatFractionSliders)
		{
			listing.Label("CanBeEditedInStorytellerSettings".Translate() + ":");
		}
		if (anomalyPlaystyleDef.displayThreatFractionSliders)
		{
			TaggedString taggedString = "Difficulty_AnomalyThreatsInactive_Info".Translate();
			listing.Label("Difficulty_AnomalyThreatsInactive_Label".Translate() + ": " + anomalyThreatsInactiveFraction.ToStringPercent() + " - " + GetFrequencyLabel(anomalyThreatsInactiveFraction), -1f, taggedString);
			anomalyThreatsInactiveFraction = listing.Slider(anomalyThreatsInactiveFraction, 0f, 1f);
			TaggedString taggedString2 = "Difficulty_AnomalyThreatsActive_Info".Translate(Mathf.Clamp01(anomalyThreatsActiveFraction).ToStringPercent(), Mathf.Clamp01(anomalyThreatsActiveFraction * 1.5f).ToStringPercent());
			listing.Label("Difficulty_AnomalyThreatsActive_Label".Translate() + ": " + anomalyThreatsActiveFraction.ToStringPercent() + " - " + GetFrequencyLabel(anomalyThreatsActiveFraction), -1f, taggedString2);
			anomalyThreatsActiveFraction = listing.Slider(anomalyThreatsActiveFraction, 0f, 1f);
		}
		else if (anomalyPlaystyleDef.overrideThreatFraction)
		{
			TaggedString taggedString3 = "Difficulty_AnomalyThreats_Info".Translate();
			listing.Label("Difficulty_AnomalyThreats_Label".Translate() + ": " + overrideAnomalyThreatsFraction.ToStringPercent() + " - " + GetFrequencyLabel(overrideAnomalyThreatsFraction), -1f, taggedString3);
			overrideAnomalyThreatsFraction = listing.Slider(overrideAnomalyThreatsFraction, 0f, 1f);
		}
		if (anomalyPlaystyleDef.displayStudyFactorSlider)
		{
			listing.Label("Difficulty_StudyEfficiency_Label".Translate() + ": " + studyEfficiencyFactor.ToStringPercent(), -1f, "Difficulty_StudyEfficiency_Info".Translate());
			studyEfficiencyFactor = listing.Slider(studyEfficiencyFactor, 0f, 5f);
		}
	}

	public static string GetFrequencyLabel(float freq)
	{
		for (int i = 0; i < FrequencyLabels.Length; i++)
		{
			if (freq <= FrequencyLabels[i].Item1)
			{
				return FrequencyLabels[i].Item2.Translate();
			}
		}
		return FrequencyLabels[FrequencyLabels.Length - 1].Item2.Translate();
	}
}
