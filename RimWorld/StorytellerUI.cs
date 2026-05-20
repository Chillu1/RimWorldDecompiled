using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class StorytellerUI
{
	private static Vector2 scrollPosition = default(Vector2);

	private static Vector2 explanationScrollPosition = default(Vector2);

	private static AnimationCurve explanationScrollPositionAnimated;

	private static Rect explanationInnerRect = default(Rect);

	private static float sectionHeightThreats = 0f;

	private static float sectionHeightGeneral = 0f;

	private static float sectionHeightPlayerTools = 0f;

	private static float sectionHeightEconomy = 0f;

	private static float sectionHeightAdaptation = 0f;

	private static float sectionHeightIdeology = 0f;

	private static float sectionHeightChildren = 0f;

	private static float sectionHeightAnomaly = 0f;

	private static readonly Texture2D StorytellerHighlightTex = ContentFinder<Texture2D>.Get("UI/HeroArt/Storytellers/Highlight");

	private const float CustomSettingsPrecision = 0.01f;

	public static void ResetStorytellerSelectionInterface()
	{
		scrollPosition = default(Vector2);
		explanationScrollPosition = default(Vector2);
		explanationScrollPositionAnimated = null;
	}

	public static void DrawStorytellerSelectionInterface(Rect rect, ref StorytellerDef chosenStoryteller, ref DifficultyDef difficulty, ref Difficulty difficultyValues, Listing_Standard infoListing)
	{
		Widgets.BeginGroup(rect);
		Rect outRect = new Rect(0f, 0f, Storyteller.PortraitSizeTiny.x + 16f, rect.height);
		Widgets.BeginScrollView(viewRect: new Rect(0f, 0f, Storyteller.PortraitSizeTiny.x, (float)DefDatabase<StorytellerDef>.AllDefs.Count() * (Storyteller.PortraitSizeTiny.y + 10f)), outRect: outRect, scrollPosition: ref scrollPosition);
		Rect rect2 = new Rect(0f, 0f, Storyteller.PortraitSizeTiny.x, Storyteller.PortraitSizeTiny.y).ContractedBy(4f);
		foreach (StorytellerDef item in DefDatabase<StorytellerDef>.AllDefs.OrderBy((StorytellerDef tel) => tel.listOrder))
		{
			if (item.listVisible)
			{
				bool flag = chosenStoryteller == item;
				Widgets.DrawOptionBackground(rect2, flag);
				if (Widgets.ButtonImage(rect2, item.portraitTinyTex, Color.white, new Color(0.72f, 0.68f, 0.59f)))
				{
					TutorSystem.Notify_Event("ChooseStoryteller");
					chosenStoryteller = item;
				}
				if (flag)
				{
					GUI.DrawTexture(rect2, StorytellerHighlightTex);
				}
				rect2.y += rect2.height + 8f;
			}
		}
		Widgets.EndScrollView();
		Rect outRect2 = new Rect(outRect.xMax + 8f, 0f, rect.width - outRect.width - 8f, rect.height);
		explanationInnerRect.width = outRect2.width - 16f;
		Widgets.BeginScrollView(outRect2, ref explanationScrollPosition, explanationInnerRect);
		Text.Font = GameFont.Small;
		Widgets.Label(new Rect(0f, 0f, 300f, 999f), "HowStorytellersWork".Translate());
		Rect rect3 = new Rect(0f, 120f, 290f, 9999f);
		float num = 300f;
		if (chosenStoryteller != null && chosenStoryteller.listVisible)
		{
			Rect position = new Rect(390f - outRect2.x, rect.height - Storyteller.PortraitSizeLarge.y - 1f, Storyteller.PortraitSizeLarge.x, Storyteller.PortraitSizeLarge.y);
			GUI.DrawTexture(position, chosenStoryteller.portraitLargeTex);
			Text.Anchor = TextAnchor.UpperLeft;
			infoListing.Begin(rect3);
			Text.Font = GameFont.Medium;
			infoListing.Indent(15f);
			infoListing.Label(chosenStoryteller.label);
			infoListing.Outdent(15f);
			Text.Font = GameFont.Small;
			infoListing.Gap(8f);
			infoListing.Label(chosenStoryteller.description, 160f);
			infoListing.Gap(6f);
			foreach (DifficultyDef allDef in DefDatabase<DifficultyDef>.AllDefs)
			{
				TaggedString labelCap = allDef.LabelCap;
				if (allDef.isCustom)
				{
					labelCap += "...";
				}
				if (infoListing.RadioButton(labelCap, difficulty == allDef, 0f, allDef.description.ResolveTags(), 0f))
				{
					if (!allDef.isCustom)
					{
						difficultyValues.CopyFrom(allDef);
					}
					else if (allDef != difficulty)
					{
						difficultyValues.CopyFrom(DifficultyDefOf.Rough);
						float time = Time.time;
						float num2 = 0.6f;
						explanationScrollPositionAnimated = AnimationCurve.EaseInOut(time, explanationScrollPosition.y, time + num2, explanationInnerRect.height);
					}
					difficulty = allDef;
				}
				infoListing.Gap(3f);
			}
			if (Current.ProgramState == ProgramState.Entry)
			{
				infoListing.Gap(15f);
				bool active = Find.GameInitData.permadeathChosen && Find.GameInitData.permadeath;
				bool active2 = Find.GameInitData.permadeathChosen && !Find.GameInitData.permadeath;
				if (infoListing.RadioButton("ReloadAnytimeMode".Translate(), active2, 0f, "ReloadAnytimeModeInfo".Translate()))
				{
					Find.GameInitData.permadeathChosen = true;
					Find.GameInitData.permadeath = false;
				}
				infoListing.Gap(3f);
				if (infoListing.RadioButton("CommitmentMode".TranslateWithBackup("PermadeathMode"), active, 0f, "PermadeathModeInfo".Translate()))
				{
					Find.GameInitData.permadeathChosen = true;
					Find.GameInitData.permadeath = true;
				}
				if (ModsConfig.AnomalyActive)
				{
					infoListing.Gap(15f);
					if (infoListing.ButtonText("AnomalySettings".Translate() + "..."))
					{
						if (difficulty == null)
						{
							Messages.Message("MustChooseDifficulty".Translate(), MessageTypeDefOf.RejectInput, historical: false);
						}
						else
						{
							Find.WindowStack.Add(new Dialog_AnomalySettings(difficultyValues));
						}
					}
				}
			}
			num = rect3.y + infoListing.CurHeight;
			infoListing.End();
			if (difficulty != null && difficulty.isCustom)
			{
				if (explanationScrollPositionAnimated != null)
				{
					float time2 = Time.time;
					if (time2 < explanationScrollPositionAnimated.keys.Last().time)
					{
						explanationScrollPosition.y = explanationScrollPositionAnimated.Evaluate(time2);
					}
					else
					{
						explanationScrollPositionAnimated = null;
					}
				}
				Listing_Standard listing_Standard = new Listing_Standard();
				float num3 = position.xMax - explanationInnerRect.x;
				listing_Standard.ColumnWidth = num3 / 2f - 17f;
				Rect rect4 = new Rect(0f, Math.Max(position.yMax, num), num3, 9999f);
				listing_Standard.Begin(rect4);
				Text.Font = GameFont.Medium;
				listing_Standard.Indent(15f);
				listing_Standard.Label("DifficultyCustomSectionLabel".Translate());
				listing_Standard.Outdent(15f);
				Text.Font = GameFont.Small;
				listing_Standard.Gap();
				if (listing_Standard.ButtonText("DifficultyReset".Translate()))
				{
					MakeResetDifficultyFloatMenu(difficultyValues);
				}
				float curHeight = listing_Standard.CurHeight;
				float gapHeight = outRect2.height / 2f;
				DrawCustomLeft(listing_Standard, difficultyValues);
				listing_Standard.Gap(gapHeight);
				listing_Standard.NewColumn();
				listing_Standard.Gap(curHeight);
				DrawCustomRight(listing_Standard, difficultyValues);
				listing_Standard.Gap(gapHeight);
				num = rect4.y + listing_Standard.MaxColumnHeightSeen;
				listing_Standard.End();
			}
		}
		explanationInnerRect.height = num;
		Widgets.EndScrollView();
		Widgets.EndGroup();
	}

	private static void DrawCustomLeft(Listing_Standard listing, Difficulty difficulty)
	{
		Listing_Standard listing_Standard = DrawCustomSectionStart(listing, sectionHeightThreats, "DifficultyThreatSection".Translate());
		DrawCustomDifficultySlider(listing_Standard, "threatScale", ref difficulty.threatScale, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		DrawCustomDifficultyCheckbox(listing_Standard, "allowBigThreats", ref difficulty.allowBigThreats);
		DrawCustomDifficultyCheckbox(listing_Standard, "allowViolentQuests", ref difficulty.allowViolentQuests);
		DrawCustomDifficultyCheckbox(listing_Standard, "allowIntroThreats", ref difficulty.allowIntroThreats);
		DrawCustomDifficultyCheckbox(listing_Standard, "predatorsHuntHumanlikes", ref difficulty.predatorsHuntHumanlikes);
		DrawCustomDifficultyCheckbox(listing_Standard, "allowExtremeWeatherIncidents", ref difficulty.allowExtremeWeatherIncidents);
		if (ModsConfig.BiotechActive)
		{
			DrawCustomDifficultySlider(listing_Standard, "wastepackInfestationChanceFactor", ref difficulty.wastepackInfestationChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		}
		DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightThreats);
		listing_Standard = DrawCustomSectionStart(listing, sectionHeightEconomy, "DifficultyEconomySection".Translate());
		DrawCustomDifficultySlider(listing_Standard, "cropYieldFactor", ref difficulty.cropYieldFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		DrawCustomDifficultySlider(listing_Standard, "mineYieldFactor", ref difficulty.mineYieldFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		DrawCustomDifficultySlider(listing_Standard, "butcherYieldFactor", ref difficulty.butcherYieldFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		if (ModsConfig.OdysseyActive)
		{
			DrawCustomDifficultySlider(listing_Standard, "fishingYieldFactor", ref difficulty.fishingYieldFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		}
		DrawCustomDifficultySlider(listing_Standard, "researchSpeedFactor", ref difficulty.researchSpeedFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		DrawCustomDifficultySlider(listing_Standard, "questRewardValueFactor", ref difficulty.questRewardValueFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		DrawCustomDifficultySlider(listing_Standard, "raidLootPointsFactor", ref difficulty.raidLootPointsFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		DrawCustomDifficultySlider(listing_Standard, "tradePriceFactorLoss", ref difficulty.tradePriceFactorLoss, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 0.5f);
		DrawCustomDifficultySlider(listing_Standard, "maintenanceCostFactor", ref difficulty.maintenanceCostFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0.01f, 1f);
		DrawCustomDifficultySlider(listing_Standard, "scariaRotChance", ref difficulty.scariaRotChance, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
		DrawCustomDifficultySlider(listing_Standard, "enemyDeathOnDownedChanceFactor", ref difficulty.enemyDeathOnDownedChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
		DrawCustomDifficultySlider(listing_Standard, "nomadicMineableResourcesFactor", ref difficulty.nomadicMineableResourcesFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 2f);
		DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightEconomy);
		if (ModsConfig.IdeologyActive)
		{
			listing_Standard = DrawCustomSectionStart(listing, sectionHeightIdeology, "DifficultyIdeologySection".Translate());
			DrawCustomDifficultySlider(listing_Standard, "lowPopConversionBoost", ref difficulty.lowPopConversionBoost, ToStringStyle.Integer, ToStringNumberSense.Factor, 1f, 5f, 1f);
			DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightIdeology);
		}
		if (ModsConfig.AnomalyActive && difficulty.AnomalyPlaystyleDef.enableAnomalyContent)
		{
			listing_Standard = DrawCustomSectionStart(listing, sectionHeightAnomaly, "DifficultyAnomalySection".Translate());
			if (difficulty.AnomalyPlaystyleDef.overrideThreatFraction)
			{
				float value = difficulty.overrideAnomalyThreatsFraction ?? 0.15f;
				DrawCustomDifficultySlider(listing_Standard, "Difficulty_AnomalyThreats_Label".Translate(), Dialog_AnomalySettings.GetFrequencyLabel(value), "Difficulty_AnomalyThreats_Info".Translate(), ref value, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
				difficulty.overrideAnomalyThreatsFraction = value;
			}
			else
			{
				DrawCustomDifficultySlider(listing_Standard, "Difficulty_AnomalyThreatsInactive_Label".Translate(), Dialog_AnomalySettings.GetFrequencyLabel(difficulty.anomalyThreatsInactiveFraction), "Difficulty_AnomalyThreatsInactive_Info".Translate(), ref difficulty.anomalyThreatsInactiveFraction, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
				float anomalyThreatsActiveFraction = difficulty.anomalyThreatsActiveFraction;
				DrawCustomDifficultySlider(listing_Standard, "Difficulty_AnomalyThreatsActive_Label".Translate(), Dialog_AnomalySettings.GetFrequencyLabel(anomalyThreatsActiveFraction), "Difficulty_AnomalyThreatsActive_Info".Translate(Mathf.Clamp01(anomalyThreatsActiveFraction).ToStringPercent(), Mathf.Clamp01(anomalyThreatsActiveFraction * 1.5f).ToStringPercent()), ref difficulty.anomalyThreatsActiveFraction, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0.1f, 1f);
			}
			DrawCustomDifficultySlider(listing_Standard, "Difficulty_StudyEfficiency_Label".Translate(), "Difficulty_StudyEfficiency_Info".Translate(), ref difficulty.studyEfficiencyFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
			DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightAnomaly);
		}
		if (!ModsConfig.BiotechActive)
		{
			return;
		}
		listing_Standard = DrawCustomSectionStart(listing, sectionHeightChildren, "DifficultyChildrenSection".Translate());
		DrawCustomDifficultyCheckbox(listing_Standard, "noBabiesOrChildren", ref difficulty.noBabiesOrChildren);
		DrawCustomDifficultyCheckbox(listing_Standard, "babiesAreHealthy", ref difficulty.babiesAreHealthy);
		if (!difficulty.noBabiesOrChildren)
		{
			DrawCustomDifficultyCheckbox(listing_Standard, "childRaidersAllowed", ref difficulty.childRaidersAllowed);
			if (ModsConfig.AnomalyActive)
			{
				DrawCustomDifficultyCheckbox(listing_Standard, "childShamblersAllowed", ref difficulty.childShamblersAllowed);
			}
		}
		else
		{
			DrawDisabledCustomDifficultySetting(listing_Standard, "childRaidersAllowed", "BabiesAreHealthyDisableReason".Translate());
			if (ModsConfig.AnomalyActive)
			{
				DrawDisabledCustomDifficultySetting(listing_Standard, "childShamblersAllowed", "BabiesAreHealthyDisableReason".Translate());
			}
		}
		DrawCustomDifficultySlider(listing_Standard, "childAgingRate", ref difficulty.childAgingRate, ToStringStyle.Integer, ToStringNumberSense.Factor, 1f, 6f, 1f);
		DrawCustomDifficultySlider(listing_Standard, "adultAgingRate", ref difficulty.adultAgingRate, ToStringStyle.Integer, ToStringNumberSense.Factor, 1f, 6f, 1f);
		DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightChildren);
	}

	private static void DrawCustomRight(Listing_Standard listing, Difficulty difficulty)
	{
		Listing_Standard listing_Standard = DrawCustomSectionStart(listing, sectionHeightGeneral, "DifficultyGeneralSection".Translate());
		DrawCustomDifficultySlider(listing_Standard, "colonistMoodOffset", ref difficulty.colonistMoodOffset, ToStringStyle.Integer, ToStringNumberSense.Offset, -20f, 20f, 1f);
		DrawCustomDifficultySlider(listing_Standard, "foodPoisonChanceFactor", ref difficulty.foodPoisonChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		DrawCustomDifficultySlider(listing_Standard, "manhunterChanceOnDamageFactor", ref difficulty.manhunterChanceOnDamageFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		DrawCustomDifficultySlider(listing_Standard, "playerPawnInfectionChanceFactor", ref difficulty.playerPawnInfectionChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		DrawCustomDifficultySlider(listing_Standard, "diseaseIntervalFactor", ref difficulty.diseaseIntervalFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f, 0.01f, reciprocate: true, 100f);
		DrawCustomDifficultySlider(listing_Standard, "enemyReproductionRateFactor", ref difficulty.enemyReproductionRateFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		DrawCustomDifficultySlider(listing_Standard, "deepDrillInfestationChanceFactor", ref difficulty.deepDrillInfestationChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
		DrawCustomDifficultySlider(listing_Standard, "friendlyFireChanceFactor", ref difficulty.friendlyFireChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
		DrawCustomDifficultySlider(listing_Standard, "allowInstantKillChance", ref difficulty.allowInstantKillChance, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
		DrawCustomDifficultyCheckbox(listing_Standard, "peacefulTemples", ref difficulty.peacefulTemples, invert: true);
		DrawCustomDifficultyCheckbox(listing_Standard, "allowCaveHives", ref difficulty.allowCaveHives);
		DrawCustomDifficultyCheckbox(listing_Standard, "unwaveringPrisoners", ref difficulty.unwaveringPrisoners);
		DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightGeneral);
		listing_Standard = DrawCustomSectionStart(listing, sectionHeightPlayerTools, "DifficultyPlayerToolsSection".Translate());
		DrawCustomDifficultyCheckbox(listing_Standard, "allowTraps", ref difficulty.allowTraps);
		DrawCustomDifficultyCheckbox(listing_Standard, "allowTurrets", ref difficulty.allowTurrets);
		DrawCustomDifficultyCheckbox(listing_Standard, "allowMortars", ref difficulty.allowMortars);
		DrawCustomDifficultyCheckbox(listing_Standard, "classicMortars", ref difficulty.classicMortars);
		DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightPlayerTools);
		listing_Standard = DrawCustomSectionStart(listing, sectionHeightAdaptation, "DifficultyAdaptationSection".Translate());
		DrawCustomDifficultySlider(listing_Standard, "adaptationGrowthRateFactorOverZero", ref difficulty.adaptationGrowthRateFactorOverZero, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
		DrawCustomDifficultySlider(listing_Standard, "adaptationEffectFactor", ref difficulty.adaptationEffectFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
		DrawCustomDifficultyCheckbox(listing_Standard, "fixedWealthMode", ref difficulty.fixedWealthMode);
		GUI.enabled = difficulty.fixedWealthMode;
		float value = Mathf.Round(12f / difficulty.fixedWealthTimeFactor);
		DrawCustomDifficultySlider(listing_Standard, "fixedWealthTimeFactor", ref value, ToStringStyle.Integer, ToStringNumberSense.Absolute, 1f, 20f, 1f);
		difficulty.fixedWealthTimeFactor = 12f / value;
		GUI.enabled = true;
		DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightAdaptation);
	}

	private static Listing_Standard DrawCustomSectionStart(Listing_Standard listing, float height, string label, string tooltip = null)
	{
		listing.Gap();
		listing.Label((TaggedString)label, -1f, tooltip);
		Listing_Standard listing_Standard = listing.BeginSection(height, 8f, 6f);
		listing_Standard.maxOneColumn = true;
		return listing_Standard;
	}

	private static void DrawCustomSectionEnd(Listing_Standard listing, Listing_Standard section, out float height)
	{
		listing.EndSection(section);
		height = section.CurHeight;
	}

	private static void MakeResetDifficultyFloatMenu(Difficulty difficultyValues)
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (DifficultyDef d in DefDatabase<DifficultyDef>.AllDefs)
		{
			if (!d.isCustom)
			{
				list.Add(new FloatMenuOption(d.LabelCap, delegate
				{
					difficultyValues.CopyFrom(d);
				}));
			}
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	private static void DrawCustomDifficultySlider(Listing_Standard listing, string optionName, ref float value, ToStringStyle style, ToStringNumberSense numberSense, float min, float max, float precision = 0.01f, bool reciprocate = false, float reciprocalCutoff = 1000f)
	{
		string text = (reciprocate ? "_Inverted" : "");
		string text2 = optionName.CapitalizeFirst();
		string key = "Difficulty_" + text2 + text + "_Label";
		string key2 = "Difficulty_" + text2 + text + "_Info";
		float num = value;
		if (reciprocate)
		{
			num = Reciprocal(num, reciprocalCutoff);
		}
		TaggedString label = key.Translate() + ": " + num.ToStringByStyle(style, numberSense);
		listing.Label(label, -1f, key2.Translate());
		float num2 = listing.Slider(num, min, max);
		if (num2 != num)
		{
			num = GenMath.RoundTo(num2, precision);
		}
		if (reciprocate)
		{
			num = Reciprocal(num, reciprocalCutoff);
		}
		value = num;
	}

	private static void DrawCustomDifficultySlider(Listing_Standard listing, string label, string tooltip, ref float value, ToStringStyle style, ToStringNumberSense numberSense, float min, float max, float precision = 0.01f, bool reciprocate = false, float reciprocalCutoff = 1000f)
	{
		DrawCustomDifficultySlider(listing, label, null, tooltip, ref value, style, numberSense, min, max, precision, reciprocate, reciprocalCutoff);
	}

	private static void DrawCustomDifficultySlider(Listing_Standard listing, string label, string labelSuffix, string tooltip, ref float value, ToStringStyle style, ToStringNumberSense numberSense, float min, float max, float precision = 0.01f, bool reciprocate = false, float reciprocalCutoff = 1000f)
	{
		float num = value;
		if (reciprocate)
		{
			num = Reciprocal(num, reciprocalCutoff);
		}
		label = label.CapitalizeFirst() + ": " + num.ToStringByStyle(style, numberSense);
		if (!labelSuffix.NullOrEmpty())
		{
			label = label + " - " + labelSuffix;
		}
		listing.Label((TaggedString)label, -1f, tooltip.CapitalizeFirst());
		float num2 = listing.Slider(num, min, max);
		if (num2 != num)
		{
			num = GenMath.RoundTo(num2, precision);
		}
		if (reciprocate)
		{
			num = Reciprocal(num, reciprocalCutoff);
		}
		value = num;
	}

	private static void DrawCustomDifficultyCheckbox(Listing_Standard listing, string optionName, ref bool value, bool invert = false, bool showTooltip = true)
	{
		string text = (invert ? "_Inverted" : "");
		string text2 = optionName.CapitalizeFirst();
		string key = "Difficulty_" + text2 + text + "_Label";
		string key2 = "Difficulty_" + text2 + text + "_Info";
		bool checkOn = (invert ? (!value) : value);
		listing.CheckboxLabeled(key.Translate(), ref checkOn, showTooltip ? key2.Translate() : ((TaggedString)null));
		value = (invert ? (!checkOn) : checkOn);
	}

	private static void DrawDisabledCustomDifficultySetting(Listing_Standard listing, string optionName, TaggedString disableReason)
	{
		string text = optionName.CapitalizeFirst();
		string key = "Difficulty_" + text + "_Label";
		string key2 = "Difficulty_" + text + "_Info";
		Color color = GUI.color;
		GUI.color = ColoredText.SubtleGrayColor;
		listing.Label(key.Translate(), -1f, (key2.Translate() + "\n\n" + disableReason.Colorize(ColoredText.WarningColor)).ToString());
		GUI.color = color;
	}

	private static float Reciprocal(float f, float cutOff)
	{
		cutOff *= 10f;
		if (Mathf.Abs(f) < 0.01f)
		{
			return cutOff;
		}
		if (f >= 0.99f * cutOff)
		{
			return 0f;
		}
		return 1f / f;
	}
}
