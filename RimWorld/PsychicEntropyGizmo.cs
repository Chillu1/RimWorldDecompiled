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

		private Texture2D LimitedTex;

		private Texture2D UnlimitedTex;

		private const string LimitedIconPath = "UI/Icons/EntropyLimit/Limited";

		private const string UnlimitedIconPath = "UI/Icons/EntropyLimit/Unlimited";

		private static readonly Color PainBoostColor = new Color(0.2f, 0.65f, 0.35f);

		private static readonly Texture2D EntropyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.46f, 0.34f, 0.35f));

		private static readonly Texture2D OverLimitBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.75f, 0.2f, 0.15f));

		private static readonly Texture2D PsyfocusBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

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
			Widgets.DrawWindowBackground(rect);
			Text.Font = GameFont.Small;
			Rect rect3 = rect2;
			rect3.y += 6f;
			rect3.height = Text.LineHeight;
			Widgets.Label(rect3, "PsychicEntropyShort".Translate());
			Rect rect4 = rect2;
			rect4.y += 38f;
			rect4.height = Text.LineHeight;
			Widgets.Label(rect4, "Psyfocus".Translate());
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
			Rect rect6 = rect2;
			rect6.width = 175f;
			rect6.height = 38f;
			TooltipHandler.TipRegion(rect6, delegate
			{
				float f = tracker.EntropyValue / tracker.RecoveryRate;
				return string.Format("PawnTooltipPsychicEntropyStats".Translate(), Mathf.Round(tracker.EntropyValue), Mathf.Round(tracker.MaxEntropy), tracker.RecoveryRate.ToString("0.#"), Mathf.Round(f)) + "\n\n" + "PawnTooltipPsychicEntropyDesc".Translate();
			}, Gen.HashCombineInt(tracker.GetHashCode(), 133858));
			Rect rect7 = rect2;
			rect7.x += 63f;
			rect7.y += 38f;
			rect7.width = 100f;
			rect7.height = 22f;
			bool flag = Mouse.IsOver(rect7);
			Widgets.FillableBar(rect7, Mathf.Min(tracker.CurrentPsyfocus, 1f), flag ? PsyfocusBarHighlightTex : PsyfocusBarTex, EmptyBarTex, doBorder: true);
			for (int i = 1; i < Pawn_PsychicEntropyTracker.PsyfocusBandPercentages.Count - 1; i++)
			{
				DrawThreshold(rect7, Pawn_PsychicEntropyTracker.PsyfocusBandPercentages[i], tracker.CurrentPsyfocus);
			}
			DrawPsyfocusTarget(rect7, tracker.TargetPsyfocus);
			float targetPsyfocus = tracker.TargetPsyfocus;
			Vector2 mousePosition = Event.current.mousePosition;
			if (flag && Input.GetMouseButton(0))
			{
				tracker.SetPsyfocusTarget(Mathf.Round((mousePosition.x - (rect7.x + 3f)) / (rect7.width - 8f) * 16f) / 16f);
				if (Math.Abs(targetPsyfocus - tracker.TargetPsyfocus) > float.Epsilon)
				{
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.MeditationDesiredPsyfocus, KnowledgeAmount.Total);
			}
			UIHighlighter.HighlightOpportunity(rect7, "PsyfocusBar");
			GUI.color = Color.white;
			Rect rect8 = rect2;
			rect8.y += 38f;
			rect8.width = 175f;
			rect8.height = 38f;
			TooltipHandler.TipRegion(rect8, () => tracker.PsyfocusTipString(), Gen.HashCombineInt(tracker.GetHashCode(), 133873));
			if (tracker.Pawn.IsColonistPlayerControlled)
			{
				float num = 32f;
				float num2 = 4f;
				float num3 = rect2.height / 2f - num + num2;
				float num4 = rect2.width - num;
				Rect rect9 = new Rect(rect2.x + num4, rect2.y + num3, num, num);
				if (Widgets.ButtonImage(rect9, tracker.limitEntropyAmount ? LimitedTex : UnlimitedTex))
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
				TooltipHandler.TipRegionByKey(rect9, "PawnTooltipPsychicEntropyLimit");
			}
			if (tracker.PainMultiplier > 1f)
			{
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;
				string recoveryBonus = (tracker.PainMultiplier - 1f).ToStringPercent("F0");
				string text = recoveryBonus;
				float widthCached = text.GetWidthCached();
				Rect rect10 = rect2;
				rect10.x += rect2.width - widthCached / 2f - 16f;
				rect10.y += 38f;
				rect10.width = widthCached;
				rect10.height = Text.LineHeight;
				GUI.color = PainBoostColor;
				Widgets.Label(rect10, text);
				GUI.color = Color.white;
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.UpperLeft;
				TooltipHandler.TipRegion(rect10.ContractedBy(-1f), () => "PawnTooltipPsychicEntropyPainFocus".Translate(tracker.Pawn.health.hediffSet.PainTotal.ToStringPercent("F0"), recoveryBonus), Gen.HashCombineInt(tracker.GetHashCode(), 133878));
			}
			return new GizmoResult(GizmoState.Clear);
		}

		public override float GetWidth(float maxWidth)
		{
			return 212f;
		}
	}
}
