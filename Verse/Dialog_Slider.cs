using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_Slider : Window
	{
		public Func<int, string> textGetter;

		public int from;

		public int to;

		public float roundTo = 1f;

		public float extraBottomSpace;

		private Action<int> confirmAction;

		private int curValue;

		private const float BotAreaHeight = 30f;

		private const float NumberYOffset = 10f;

		public override Vector2 InitialSize => new Vector2(300f, 130f + extraBottomSpace);

		protected override float Margin => 10f;

		public Dialog_Slider(Func<int, string> textGetter, int from, int to, Action<int> confirmAction, int startingValue = int.MinValue, float roundTo = 1f)
		{
			this.textGetter = textGetter;
			this.from = from;
			this.to = to;
			this.confirmAction = confirmAction;
			this.roundTo = roundTo;
			forcePause = true;
			closeOnClickedOutside = true;
			if (startingValue == int.MinValue)
			{
				curValue = from;
			}
			else
			{
				curValue = startingValue;
			}
		}

		public Dialog_Slider(string text, int from, int to, Action<int> confirmAction, int startingValue = int.MinValue, float roundTo = 1f)
			: this((int val) => string.Format(text, val), from, to, confirmAction, startingValue, roundTo)
		{
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			string text = textGetter(curValue);
			float height = Text.CalcHeight(text, inRect.width);
			Rect rect = new Rect(inRect.x, inRect.y, inRect.width, height);
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(rect, text);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect2 = new Rect(inRect.x, inRect.y + rect.height + 10f, inRect.width, 30f);
			curValue = (int)Widgets.HorizontalSlider(rect2, curValue, from, to, middleAlignment: true, null, null, null, roundTo);
			GUI.color = ColoredText.SubtleGrayColor;
			Text.Font = GameFont.Tiny;
			Widgets.Label(new Rect(inRect.x, rect2.yMax - 10f, inRect.width / 2f, Text.LineHeight), from.ToString());
			Text.Anchor = TextAnchor.UpperRight;
			Widgets.Label(new Rect(inRect.x + inRect.width / 2f, rect2.yMax - 10f, inRect.width / 2f, Text.LineHeight), to.ToString());
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			float num = (inRect.width - 10f) / 2f;
			if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 30f, num, 30f), "CancelButton".Translate()))
			{
				Close();
			}
			if (Widgets.ButtonText(new Rect(inRect.x + num + 10f, inRect.yMax - 30f, num, 30f), "OK".Translate()))
			{
				Close();
				confirmAction(curValue);
			}
		}
	}
}
