using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Dialog_RechargeSettings : Window
	{
		private FloatRange range;

		private MechanitorControlGroup controlGroup;

		private string title;

		private string text;

		private const float HeaderHeight = 30f;

		private const float SliderHeight = 30f;

		public override Vector2 InitialSize => new Vector2(450f, 300f);

		public Dialog_RechargeSettings(MechanitorControlGroup controlGroup)
		{
			this.controlGroup = controlGroup;
			range = controlGroup.mechRechargeThresholds;
			title = "MechRechargeSettingsTitle".Translate();
			text = "MechRechargeSettingsExplanation".Translate();
			forcePause = true;
			closeOnClickedOutside = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			float y = inRect.y;
			Rect rect = new Rect(inRect.x, y, inRect.width, 30f);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, title);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			y += rect.height + 17f;
			Rect rect2 = new Rect(inRect.x, y, inRect.width, Text.CalcHeight(text, inRect.width));
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.Font = GameFont.Small;
			Widgets.Label(rect2, text);
			Text.Anchor = TextAnchor.UpperLeft;
			y += rect2.height + 17f;
			Widgets.FloatRange(new Rect(inRect.x, y, inRect.width, 30f), GetHashCode(), ref range, 0f, 1f, null, ToStringStyle.PercentZero, 0.05f, GameFont.Small, Color.white);
			range.min = GenMath.RoundTo(range.min, 0.01f);
			range.max = GenMath.RoundTo(range.max, 0.01f);
			if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - Window.CloseButSize.y, Window.CloseButSize.x, Window.CloseButSize.y), "CancelButton".Translate()))
			{
				Close();
			}
			if (Widgets.ButtonText(new Rect(inRect.x + inRect.width / 2f - Window.CloseButSize.x / 2f, inRect.yMax - Window.CloseButSize.y, Window.CloseButSize.x, Window.CloseButSize.y), "Reset".Translate()))
			{
				range = MechanitorControlGroup.DefaultMechRechargeThresholds;
			}
			if (Widgets.ButtonText(new Rect(inRect.xMax - Window.CloseButSize.x, inRect.yMax - Window.CloseButSize.y, Window.CloseButSize.x, Window.CloseButSize.y), "OK".Translate()))
			{
				controlGroup.mechRechargeThresholds = range;
				Close();
			}
		}
	}
}
