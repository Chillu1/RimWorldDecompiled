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

		private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.55f, 0.84f));

		private static readonly Texture2D OverLimitBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.75f, 0.2f, 0.15f));

		private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

		public PsychicEntropyGizmo(Pawn_PsychicEntropyTracker tracker)
		{
			this.tracker = tracker;
			order = -100f;
			LimitedTex = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Limited");
			UnlimitedTex = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Unlimited");
		}

		private void DrawThreshold(Rect rect, float percent)
		{
			Rect rect2 = default(Rect);
			rect2.x = rect.x + rect.width * percent - 1f;
			rect2.y = rect.y;
			rect2.width = 2f;
			rect2.height = 6f;
			Rect position = rect2;
			if (tracker.EntropyRelativeValue < percent)
			{
				GUI.DrawTexture(position, BaseContent.GreyTex);
			}
			else
			{
				GUI.DrawTexture(position, BaseContent.BlackTex);
			}
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(6f);
			Widgets.DrawWindowBackground(rect);
			Text.Font = GameFont.Tiny;
			Rect rect3 = rect2;
			rect3.height = Text.LineHeight;
			Widgets.Label(rect3, "PsychicEntropy".Translate());
			if (tracker.Pawn.IsColonistPlayerControlled)
			{
				float num = 32f;
				float num2 = 6f;
				float num3 = rect2.height / 2f - num + num2;
				float num4 = rect2.width - num;
				Rect rect4 = new Rect(rect2.x + num4, rect2.y + num3, num, num);
				if (Widgets.ButtonImage(rect4, tracker.limitEntropyAmount ? LimitedTex : UnlimitedTex))
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
				TooltipHandler.TipRegionByKey(rect4, "PawnTooltipPsychicEntropyLimit");
			}
			if (tracker.PainMultiplier > 1f)
			{
				string recoveryBonus = (tracker.PainMultiplier - 1f).ToStringPercent();
				TaggedString label = "PsychicEntropyPainFocus".Translate(recoveryBonus);
				float widthCached = label.GetWidthCached();
				Rect rect5 = rect2;
				rect5.y += rect2.height / 2f - Text.LineHeight + 4f;
				rect5.width = widthCached;
				rect5.height = Text.LineHeight;
				Widgets.Label(rect5, label);
				TooltipHandler.TipRegion(rect5.ContractedBy(-1f), () => "PawnTooltipPsychicEntropyPainFocus".Translate(tracker.Pawn.health.hediffSet.PainTotal.ToStringPercent(), recoveryBonus), Gen.HashCombineInt(tracker.GetHashCode(), 133878));
			}
			Rect rect6 = rect2;
			rect6.yMin = rect2.y + rect2.height / 2f + 4f;
			float entropyRelativeValue = tracker.EntropyRelativeValue;
			Widgets.FillableBar(rect6, Mathf.Min(entropyRelativeValue, 1f), FullBarTex, EmptyBarTex, doBorder: false);
			if (tracker.EntropyValue > tracker.MaxEntropy)
			{
				Widgets.FillableBar(rect6, Mathf.Min(entropyRelativeValue - 1f, 1f), OverLimitBarTex, FullBarTex, doBorder: false);
				foreach (KeyValuePair<PsychicEntropySeverity, float> entropyThreshold in Pawn_PsychicEntropyTracker.EntropyThresholds)
				{
					if (entropyThreshold.Value > 1f && entropyThreshold.Value < 2f)
					{
						DrawThreshold(rect6, entropyThreshold.Value - 1f);
					}
				}
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect6, tracker.EntropyValue.ToString("F0") + " / " + tracker.MaxEntropy.ToString("F0"));
			Text.Anchor = TextAnchor.UpperLeft;
			if (Mouse.IsOver(rect2))
			{
				TooltipHandler.TipRegion(tip: new TipSignal(delegate
				{
					float f = tracker.EntropyValue / tracker.RecoveryRatePerSecond;
					return string.Format("PawnTooltipPsychicEntropy".Translate(), Mathf.Round(tracker.EntropyValue), Mathf.Round(tracker.MaxEntropy), tracker.RecoveryRate.ToString("0.0"), 30f, Mathf.Round(f));
				}, Gen.HashCombineInt(tracker.GetHashCode(), 133877), TooltipPriority.Pawn), rect: rect2);
			}
			return new GizmoResult(GizmoState.Clear);
		}

		public override float GetWidth(float maxWidth)
		{
			return 170f;
		}
	}
}
