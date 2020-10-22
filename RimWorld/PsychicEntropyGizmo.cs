using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PsychicEntropyGizmo : Gizmo
	{
		private Pawn_PsychicEntropyTracker tracker;

		private float selectedPsyfocusTarget = -1f;

		private bool draggingPsyfocusBar;

		private Texture2D LimitedTex;

		private Texture2D UnlimitedTex;

		private const string LimitedIconPath = "UI/Icons/EntropyLimit/Limited";

		private const string UnlimitedIconPath = "UI/Icons/EntropyLimit/Unlimited";

		private const float CostPreviewFadeIn = 0.1f;

		private const float CostPreviewSolid = 0.15f;

		private const float CostPreviewFadeOut = 0.6f;

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
			order = -100f;
			LimitedTex = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Limited");
			UnlimitedTex = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Unlimited");
		}

		private void DrawThreshold(Rect rect, float percent, float entropyValue)
		{
			Rect rect2 = default(Rect);
			rect2.x = rect.x + 3f + (rect.width - 8f) * percent;
			rect2.y = rect.y + rect.height - 9f;
			rect2.width = 2f;
			rect2.height = 6f;
			Rect position = rect2;
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
			Rect position = default(Rect);
			position.x = rect.x + 3f + num;
			position.y = rect.y;
			position.width = 2f;
			position.height = rect.height;
			GUI.DrawTexture(position, PsyfocusTargetTex);
			float num2 = Widgets.AdjustCoordToUIScalingFloor(rect.x + 2f + num);
			float xMax = Widgets.AdjustCoordToUIScalingCeil(num2 + 4f);
			position = default(Rect);
			position.y = rect.y - 3f;
			position.height = 5f;
			position.xMin = num2;
			position.xMax = xMax;
			Rect rect2 = position;
			GUI.DrawTexture(rect2, PsyfocusTargetTex);
			Rect position2 = rect2;
			position2.y = rect.yMax - 2f;
			GUI.DrawTexture(position2, PsyfocusTargetTex);
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(6f);
			Command_Psycast command_Psycast = ((MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow)?.LastMouseoverGizmo as Command_Psycast;
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
					rect6.xMin = Widgets.AdjustCoordToUIScalingFloor(rect6.xMin + num4 * width);
					rect6.width = Widgets.AdjustCoordToUIScalingFloor(Mathf.Max(Mathf.Min(num3, 1f) - num4, 0f) * width);
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
				return string.Format("PawnTooltipPsychicEntropyStats".Translate(), Mathf.Round(tracker.EntropyValue), Mathf.Round(tracker.MaxEntropy), tracker.RecoveryRate.ToString("0.#"), Mathf.Round(f)) + "\n\n" + "PawnTooltipPsychicEntropyDesc".Translate();
			}, Gen.HashCombineInt(tracker.GetHashCode(), 133858));
			Rect rect8 = rect2;
			rect8.x += 63f;
			rect8.y += 38f;
			rect8.width = 100f;
			rect8.height = 22f;
			bool flag = Mouse.IsOver(rect8);
			Widgets.FillableBar(rect8, Mathf.Min(tracker.CurrentPsyfocus, 1f), flag ? PsyfocusBarHighlightTex : PsyfocusBarTex, EmptyBarTex, doBorder: true);
			if (command_Psycast != null)
			{
				float min = command_Psycast.Ability.def.PsyfocusCostRange.min;
				if (min > float.Epsilon)
				{
					Rect rect9 = rect8.ContractedBy(3f);
					float num5 = Mathf.Max(tracker.CurrentPsyfocus - min, 0f);
					float width2 = rect9.width;
					rect9.xMin = Widgets.AdjustCoordToUIScalingFloor(rect9.xMin + num5 * width2);
					rect9.width = Widgets.AdjustCoordToUIScalingCeil((tracker.CurrentPsyfocus - num5) * width2);
					GUI.color = new Color(1f, 1f, 1f, num2);
					GenUI.DrawTextureWithMaterial(rect9, PsyfocusBarTexReduce, null);
					GUI.color = Color.white;
				}
			}
			for (int i = 1; i < Pawn_PsychicEntropyTracker.PsyfocusBandPercentages.Count - 1; i++)
			{
				DrawThreshold(rect8, Pawn_PsychicEntropyTracker.PsyfocusBandPercentages[i], tracker.CurrentPsyfocus);
			}
			float num6 = Mathf.Clamp(Mathf.Round((Event.current.mousePosition.x - (rect8.x + 3f)) / (rect8.width - 8f) * 16f) / 16f, 0f, 1f);
			Event current2 = Event.current;
			if (current2.type == EventType.MouseDown && current2.button == 0 && flag)
			{
				selectedPsyfocusTarget = num6;
				draggingPsyfocusBar = true;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.MeditationDesiredPsyfocus, KnowledgeAmount.Total);
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
				current2.Use();
			}
			if (current2.type == EventType.MouseDrag && current2.button == 0 && draggingPsyfocusBar && flag)
			{
				if (Math.Abs(num6 - selectedPsyfocusTarget) > float.Epsilon)
				{
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				selectedPsyfocusTarget = num6;
				current2.Use();
			}
			if (current2.type == EventType.MouseUp && current2.button == 0 && draggingPsyfocusBar)
			{
				if (selectedPsyfocusTarget >= 0f)
				{
					tracker.SetPsyfocusTarget(selectedPsyfocusTarget);
				}
				selectedPsyfocusTarget = -1f;
				draggingPsyfocusBar = false;
				current2.Use();
			}
			UIHighlighter.HighlightOpportunity(rect8, "PsyfocusBar");
			DrawPsyfocusTarget(rect8, draggingPsyfocusBar ? selectedPsyfocusTarget : tracker.TargetPsyfocus);
			GUI.color = Color.white;
			Rect rect10 = rect2;
			rect10.y += 38f;
			rect10.width = 175f;
			rect10.height = 38f;
			TooltipHandler.TipRegion(rect10, () => tracker.PsyfocusTipString_NewTemp(selectedPsyfocusTarget), Gen.HashCombineInt(tracker.GetHashCode(), 133873));
			if (tracker.Pawn.IsColonistPlayerControlled)
			{
				float num7 = 32f;
				float num8 = 4f;
				float num9 = rect2.height / 2f - num7 + num8;
				float num10 = rect2.width - num7;
				Rect rect11 = new Rect(rect2.x + num10, rect2.y + num9, num7, num7);
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
			if (tracker.PainMultiplier > 1f)
			{
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;
				string recoveryBonus = (tracker.PainMultiplier - 1f).ToStringPercent("F0");
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

		public override float GetWidth(float maxWidth)
		{
			return 212f;
		}
	}
}
