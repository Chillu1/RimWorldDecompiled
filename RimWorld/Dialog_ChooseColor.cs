using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_ChooseColor : Window
	{
		private string header;

		private Color selectedColor;

		private Action<Color> onApply;

		private List<Color> colors;

		private Vector2 scrollPosition;

		private const float HeaderHeight = 35f;

		private const int ColorSize = 22;

		private const int ColorPadding = 2;

		public override Vector2 InitialSize => new Vector2(500f, 410f);

		public Dialog_ChooseColor(string header, Color selectedColor, List<Color> colors, Action<Color> onApply)
		{
			this.header = header;
			this.selectedColor = selectedColor;
			this.onApply = onApply;
			this.colors = colors;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 35f), header);
			Text.Font = GameFont.Small;
			Widgets.ColorSelectorIcon(new Rect(inRect.x, inRect.y + 35f + 10f, 88f, 88f).ContractedBy(2f), null, selectedColor, drawColor: true);
			Rect rect = inRect;
			rect.xMin += 105f;
			rect.yMin += 45f;
			rect.height -= Window.CloseButSize.y + 10f;
			float num = (float)colors.Count / (rect.width / 24f);
			Rect viewRect = rect;
			viewRect.width -= 16f;
			viewRect.height = num * 24f;
			Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
			Widgets.ColorSelector(rect, ref selectedColor, colors, out var _);
			Widgets.EndScrollView();
			Text.Font = GameFont.Small;
			if (Widgets.ButtonText(new Rect(inRect.x, inRect.height - Window.CloseButSize.y, Window.CloseButSize.x, Window.CloseButSize.y), "CloseButton".Translate()))
			{
				Close();
			}
			if (Widgets.ButtonText(new Rect(inRect.width - Window.CloseButSize.x, inRect.height - Window.CloseButSize.y, Window.CloseButSize.x, Window.CloseButSize.y), "OK".Translate()))
			{
				onApply?.Invoke(selectedColor);
				Close();
			}
		}
	}
}
