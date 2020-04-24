using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_Slider : Window
	{
		public Func<int, string> textGetter;

		public int from;

		public int to;

		private Action<int> confirmAction;

		private int curValue;

		private const float BotAreaHeight = 30f;

		private const float TopPadding = 15f;

		public override Vector2 InitialSize => new Vector2(300f, 130f);

		public Dialog_Slider(Func<int, string> textGetter, int from, int to, Action<int> confirmAction, int startingValue = int.MinValue)
		{
			this.textGetter = textGetter;
			this.from = from;
			this.to = to;
			this.confirmAction = confirmAction;
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

		public Dialog_Slider(string text, int from, int to, Action<int> confirmAction, int startingValue = int.MinValue)
			: this((int val) => string.Format(text, val), from, to, confirmAction, startingValue)
		{
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(inRect.x, inRect.y + 15f, inRect.width, 30f);
			curValue = (int)Widgets.HorizontalSlider(rect, curValue, from, to, middleAlignment: true, textGetter(curValue), null, null, 1f);
			Text.Font = GameFont.Small;
			if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 30f, inRect.width / 2f, 30f), "CancelButton".Translate()))
			{
				Close();
			}
			if (Widgets.ButtonText(new Rect(inRect.x + inRect.width / 2f, inRect.yMax - 30f, inRect.width / 2f, 30f), "OK".Translate()))
			{
				Close();
				confirmAction(curValue);
			}
		}
	}
}
