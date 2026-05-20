using System;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_SelectXenotypeIcon : Window
{
	private Vector2 scrollPosition;

	private float scrollHeight;

	private XenotypeIconDef selected;

	private Action<XenotypeIconDef> iconSelector;

	private const float HeaderHeight = 35f;

	private const float IconSize = 35f;

	private const float IconGap = 6f;

	private const int IconsPerRow = 8;

	private static readonly Color OutlineColorSelected = new Color(1f, 1f, 0.7f, 1f);

	private static readonly Color OutlineColorUnselected = new Color(1f, 1f, 1f, 0.1f);

	public override Vector2 InitialSize => new Vector2(334f + Margin * 2f + 16f, 400f);

	public Dialog_SelectXenotypeIcon(XenotypeIconDef selected, Action<XenotypeIconDef> iconSelector)
	{
		this.selected = selected;
		this.iconSelector = iconSelector;
		closeOnClickedOutside = true;
	}

	public override void PostOpen()
	{
		if (!ModLister.CheckBiotech("xenotype icon"))
		{
			Close(doCloseSound: false);
		}
		else
		{
			base.PostOpen();
		}
	}

	public override void DoWindowContents(Rect rect)
	{
		Rect rect2 = rect;
		Text.Font = GameFont.Medium;
		Widgets.Label(rect2, "SelectIcon".Translate());
		Text.Font = GameFont.Small;
		rect2.yMin += 39f;
		rect2.yMax -= Window.CloseButSize.y + 4f;
		Rect outRect = rect2;
		outRect.yMax -= 4f;
		Rect rect3 = new Rect(outRect.x, outRect.y, outRect.width - 16f, scrollHeight);
		Widgets.DrawLightHighlight(rect3);
		Widgets.BeginScrollView(outRect, ref scrollPosition, rect3);
		float num = outRect.x + 6f;
		float num2 = outRect.y + 6f;
		foreach (XenotypeIconDef allDef in DefDatabase<XenotypeIconDef>.AllDefs)
		{
			if (num + 35f + 6f > rect3.width)
			{
				num = outRect.x + 6f;
				num2 += 41f;
				scrollHeight = Mathf.Max(scrollHeight, num2);
			}
			Rect rect4 = new Rect(num, num2, 35f, 35f);
			Widgets.DrawHighlight(rect4);
			if (selected == allDef)
			{
				GUI.color = OutlineColorSelected;
				Widgets.DrawHighlight(rect4);
				Widgets.DrawBox(rect4.ExpandedBy(2f), 2);
			}
			else
			{
				GUI.color = OutlineColorUnselected;
				Widgets.DrawBox(rect4);
			}
			GUI.color = Color.white;
			if (Widgets.ButtonImage(rect4, allDef.Icon, XenotypeDef.IconColor))
			{
				selected = allDef;
			}
			num += 41f;
		}
		Widgets.EndScrollView();
		if (Widgets.ButtonText(new Rect((rect.width - Window.CloseButSize.x) / 2f, rect2.yMax, Window.CloseButSize.x, Window.CloseButSize.y), "Accept".Translate()))
		{
			Close();
		}
	}

	public override void PreClose()
	{
		iconSelector(selected);
	}
}
