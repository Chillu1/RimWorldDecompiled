using System.Collections.Generic;
using LudeonTK;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class PsychicEntropyGizmo : Gizmo
{
	private Pawn_PsychicEntropyTracker tracker;

	private float selectedPsyfocusTarget = -1f;

	private static bool draggingBar;

	private float lastTargetValue;

	private float targetValue;

	private Texture2D LimitedTex;

	private Texture2D UnlimitedTex;

	private const string LimitedIconPath = "UI/Icons/EntropyLimit/Limited";

	private const string UnlimitedIconPath = "UI/Icons/EntropyLimit/Unlimited";

	public const float CostPreviewFadeIn = 0.1f;

	public const float CostPreviewSolid = 0.15f;

	public const float CostPreviewFadeInSolid = 0.25f;

	public const float CostPreviewFadeOut = 0.6f;

	private static readonly Color PainBoostColor = new Color(0.2f, 0.65f, 0.35f);

	private static readonly Texture2D EntropyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.46f, 0.34f, 0.35f));

	private static readonly Texture2D EntropyBarTexAdd = SolidColorMaterials.NewSolidColorTexture(new Color(0.78f, 0.72f, 0.66f));

	private static readonly Texture2D OverLimitBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.75f, 0.2f, 0.15f));

	private static readonly Texture2D PsyfocusBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

	private static readonly Texture2D PsyfocusBarTexReduce = SolidColorMaterials.NewSolidColorTexture(new Color(0.65f, 0.83f, 0.83f));

	private static readonly Texture2D PsyfocusBarHighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.43f, 0.54f, 0.55f));

	private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

	private static readonly Texture2D PsyfocusTargetTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.74f, 0.97f, 0.8f));

	public PsychicEntropyGizmo(Pawn_PsychicEntropyTracker tracker)
	{
		this.tracker = tracker;
		Order = -100f;
		targetValue = tracker.TargetPsyfocus;
		LimitedTex = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Limited");
		UnlimitedTex = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Unlimited");
	}

	private void DrawThreshold(Rect rect, float percent, float entropyValue)
	{
		Rect position = new Rect
		{
			x = rect.x + 3f + (rect.width - 8f) * percent,
			y = rect.y + rect.height - 9f,
			width = 2f,
			height = 6f
		};
		if (entropyValue < percent)
		{
			GUI.DrawTexture(position, BaseContent.GreyTex);
		}
		else
		{
			GUI.DrawTexture(position, BaseContent.BlackTex);
		}
	}

	private void DrawPsyfocusTarget(Rect rect, float percent)
	{
		float num = Mathf.Round((rect.width - 8f) * percent);
		GUI.DrawTexture(new Rect
		{
			x = rect.x + 3f + num,
			y = rect.y,
			width = 2f,
			height = rect.height
		}, PsyfocusTargetTex);
		float num2 = UIScaling.AdjustCoordToUIScalingFloor(rect.x + 2f + num);
		float xMax = UIScaling.AdjustCoordToUIScalingCeil(num2 + 4f);
		Rect obj = new Rect
		{
			y = rect.y - 3f,
			height = 5f,
			xMin = num2,
			xMax = xMax
		};
		GUI.DrawTexture(obj, PsyfocusTargetTex);
		Rect position = obj;
		position.y = rect.yMax - 2f;
		GUI.DrawTexture(position, PsyfocusTargetTex);
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Rect rect2 = rect.ContractedBy(6f);
		Command_Psycast command_Psycast = MapGizmoUtility.LastMouseOverGizmo as Command_Psycast;
		float num = Mathf.Repeat(Time.time, 0.85f);
		float num2 = 1f;
		if (num < 0.1f)
		{
			num2 = num / 0.1f;
		}
		else if (num >= 0.25f)
		{
			num2 = 1f - (num - 0.25f) / 0.6f;
		}
		Widgets.DrawWindowBackground(rect);
		Text.Font = GameFont.Small;
		Rect rect3 = rect2;
		rect3.y += 6f;
		rect3.height = Text.LineHeight;
		Widgets.Label(rect3, "PsychicEntropyShort".Translate());
		Rect rect4 = rect2;
		rect4.y += 38f;
		rect4.height = Text.LineHeight;
		Widgets.Label(rect4, "PsyfocusLabelGizmo".Translate());
		Rect rect5 = rect2;
		rect5.x += 63f;
		rect5.y += 6f;
		rect5.width = 100f;
		rect5.height = 22f;
		float entropyRelativeValue = tracker.EntropyRelativeValue;
		Widgets.FillableBar(rect5, Mathf.Min(entropyRelativeValue, 1f), EntropyBarTex, EmptyBarTex, doBorder: true);
		if (tracker.EntropyValue > tracker.MaxEntropy)
		{
			Widgets.FillableBar(rect5, Mathf.Min(entropyRelativeValue - 1f, 1f), OverLimitBarTex, EntropyBarTex, doBorder: true);
		}
		if (command_Psycast != null)
		{
			Ability ability = command_Psycast.Ability;
			if (ability.def.EntropyGain > float.Epsilon)
			{
				Rect rect6 = rect5.ContractedBy(3f);
				float width = rect6.width;
				float num3 = tracker.EntropyToRelativeValue(tracker.EntropyValue + ability.def.EntropyGain);
				float num4 = entropyRelativeValue;
				if (num4 > 1f)
				{
					num4 -= 1f;
					num3 -= 1f;
				}
				rect6.xMin = UIScaling.AdjustCoordToUIScalingFloor(rect6.xMin + num4 * width);
				rect6.width = UIScaling.AdjustCoordToUIScalingFloor(Mathf.Max(Mathf.Min(num3, 1f) - num4, 0f) * width);
				GUI.color = new Color(1f, 1f, 1f, num2 * 0.7f);
				GenUI.DrawTextureWithMaterial(rect6, EntropyBarTexAdd, null);
				GUI.color = Color.white;
			}
		}
		if (tracker.EntropyValue > tracker.MaxEntropy)
		{
			foreach (KeyValuePair<PsychicEntropySeverity, float> entropyThreshold in Pawn_PsychicEntropyTracker.EntropyThresholds)
			{
				if (entropyThreshold.Value > 1f && entropyThreshold.Value < 2f)
				{
					DrawThreshold(rect5, entropyThreshold.Value - 1f, entropyRelativeValue);
				}
			}
		}
		string label = tracker.EntropyValue.ToString("F0") + " / " + tracker.MaxEntropy.ToString("F0");
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(rect5, label);
		Text.Anchor = TextAnchor.UpperLeft;
		Text.Font = GameFont.Tiny;
		GUI.color = Color.white;
		Rect rect7 = rect2;
		rect7.width = 175f;
		rect7.height = 38f;
		TooltipHandler.TipRegion(rect7, delegate
		{
			float f = tracker.EntropyValue / tracker.RecoveryRate;
			TaggedString taggedString = ("PsychicEntropy".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor) + Mathf.Round(tracker.EntropyValue) + " / " + Mathf.Round(tracker.MaxEntropy);
			taggedString += "\n" + "PawnTooltipPsychicEntropyStats".Translate(tracker.RecoveryRate.ToString("0.#"), Mathf.Round(f));
			return (taggedString + ("\n\n" + "PawnTooltipPsychicEntropyDesc".Translate())).Resolve();
		}, Gen.HashCombineInt(tracker.GetHashCode(), 133858));
		Rect rect8 = rect2;
		rect8.x += 63f;
		rect8.y += 38f;
		rect8.width = 100f;
		rect8.height = 22f;
		lastTargetValue = targetValue;
		if (tracker.Pawn.IsColonistPlayerControlled)
		{
			Widgets.DraggableBar(rect8, PsyfocusBarTex, PsyfocusBarHighlightTex, EmptyBarTex, PsyfocusTargetTex, ref draggingBar, tracker.CurrentPsyfocus, ref targetValue, Pawn_PsychicEntropyTracker.PsyfocusBandPercentages, 16);
			if (lastTargetValue != targetValue)
			{
				tracker.SetPsyfocusTarget(targetValue);
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.MeditationDesiredPsyfocus, KnowledgeAmount.Total);
			}
		}
		else
		{
			Widgets.FillableBar(rect8, tracker.CurrentPsyfocus, PsyfocusBarTex, EmptyBarTex, doBorder: true);
		}
		UIHighlighter.HighlightOpportunity(rect8, "PsyfocusBar");
		if (command_Psycast != null)
		{
			float min = command_Psycast.Ability.def.PsyfocusCostRange.min;
			if (min > float.Epsilon)
			{
				Rect rect9 = rect8.ContractedBy(3f);
				float num5 = Mathf.Max(tracker.CurrentPsyfocus - min, 0f);
				float width2 = rect9.width;
				rect9.xMin = UIScaling.AdjustCoordToUIScalingFloor(rect9.xMin + num5 * width2);
				rect9.width = UIScaling.AdjustCoordToUIScalingCeil((tracker.CurrentPsyfocus - num5) * width2);
				GUI.color = new Color(1f, 1f, 1f, num2);
				GenUI.DrawTextureWithMaterial(rect9, PsyfocusBarTexReduce, null);
				GUI.color = Color.white;
			}
		}
		Rect rect10 = rect2;
		rect10.y += 38f;
		rect10.width = 175f;
		rect10.height = 38f;
		TooltipHandler.TipRegion(rect10, () => tracker.PsyfocusTipString(selectedPsyfocusTarget), Gen.HashCombineInt(tracker.GetHashCode(), 133873));
		if (tracker.Pawn.IsColonistPlayerControlled)
		{
			float num6 = 32f;
			float num7 = 4f;
			float num8 = rect2.height / 2f - num6 + num7;
			float num9 = rect2.width - num6;
			Rect rect11 = new Rect(rect2.x + num9, rect2.y + num8, num6, num6);
			if (Widgets.ButtonImage(rect11, tracker.limitEntropyAmount ? LimitedTex : UnlimitedTex))
			{
				tracker.limitEntropyAmount = !tracker.limitEntropyAmount;
				if (tracker.limitEntropyAmount)
				{
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
			}
			TooltipHandler.TipRegionByKey(rect11, "PawnTooltipPsychicEntropyLimit");
		}
		if (TryGetPainMultiplier(tracker.Pawn, out var painMultiplier))
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			string recoveryBonus = (painMultiplier - 1f).ToStringPercent("F0");
			string text = recoveryBonus;
			float widthCached = text.GetWidthCached();
			Rect rect12 = rect2;
			rect12.x += rect2.width - widthCached / 2f - 16f;
			rect12.y += 38f;
			rect12.width = widthCached;
			rect12.height = Text.LineHeight;
			GUI.color = PainBoostColor;
			Widgets.Label(rect12, text);
			GUI.color = Color.white;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperLeft;
			TooltipHandler.TipRegion(rect12.ContractedBy(-1f), () => "PawnTooltipPsychicEntropyPainFocus".Translate(tracker.Pawn.health.hediffSet.PainTotal.ToStringPercent("F0"), recoveryBonus), Gen.HashCombineInt(tracker.GetHashCode(), 133878));
		}
		return new GizmoResult(GizmoState.Clear);
	}

	private bool TryGetPainMultiplier(Pawn pawn, out float painMultiplier)
	{
		List<StatPart> parts = StatDefOf.PsychicEntropyRecoveryRate.parts;
		for (int i = 0; i < parts.Count; i++)
		{
			if (parts[i] is StatPart_Pain statPart_Pain)
			{
				painMultiplier = statPart_Pain.PainFactor(tracker.Pawn);
				return true;
			}
		}
		painMultiplier = 0f;
		return false;
	}

	public override float GetWidth(float maxWidth)
	{
		return 212f;
	}
}
