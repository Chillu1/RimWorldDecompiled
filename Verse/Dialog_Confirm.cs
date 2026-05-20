using System;
using UnityEngine;

namespace Verse;

public class Dialog_Confirm : Window
{
	private string title;

	private string confirm;

	private Action onConfirm;

	private static readonly Vector2 ButtonSize = new Vector2(120f, 32f);

	public override Vector2 InitialSize => new Vector2(280f, 150f);

	public Dialog_Confirm(string title, Action onConfirm)
		: this(title, "Confirm".Translate(), onConfirm)
	{
	}

	public Dialog_Confirm(string title, string confirm, Action onConfirm)
	{
		this.title = title;
		this.confirm = confirm;
		this.onConfirm = onConfirm;
		forcePause = true;
		closeOnAccept = false;
		closeOnClickedOutside = true;
		absorbInputAroundWindow = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		bool flag = false;
		if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
		{
			flag = true;
			Event.current.Use();
		}
		Rect rect = inRect;
		rect.width = inRect.width / 2f - 5f;
		rect.yMin = inRect.yMax - ButtonSize.y - 10f;
		Rect rect2 = inRect;
		rect2.xMin = rect.xMax + 10f;
		rect2.yMin = inRect.yMax - ButtonSize.y - 10f;
		Rect rect3 = inRect;
		rect3.y += 4f;
		rect3.yMax = rect2.y - 10f;
		using (new TextBlock(TextAnchor.UpperCenter))
		{
			Widgets.Label(rect3, title);
		}
		if (Widgets.ButtonText(rect, "Cancel".Translate()))
		{
			Find.WindowStack.TryRemove(this);
		}
		if (Widgets.ButtonText(rect2, confirm) || flag)
		{
			onConfirm?.Invoke();
			Find.WindowStack.TryRemove(this);
		}
	}
}
