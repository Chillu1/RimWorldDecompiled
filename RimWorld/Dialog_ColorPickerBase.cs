using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class Dialog_ColorPickerBase : Window
{
	private const int ContextHash = 195906069;

	private static readonly List<string> focusableControlNames = new List<string> { "title", "colorTextfields_0", "colorTextfields_1", "colorTextfields_2", "colorTextfields_3", "colorTextfields_4" };

	private const int ColorWheelSize = 128;

	private const int ColorTextfieldsWidth = 125;

	private const int PaletteColumns = 9;

	private const int ColorIconSize = 22;

	private const int ColorIconPadding = 2;

	private const int CurrentColorLabelWidth = 100;

	private const int PaletteWidth = 250;

	private const int ColorTemperatureBarHeight = 34;

	protected Color color;

	protected Color oldColor;

	private bool hsvColorWheelDragging;

	private bool colorTemperatureDragging;

	private string[] textfieldBuffers = new string[6];

	private Color textfieldColorBuffer;

	private string previousFocusedControlName;

	private Widgets.ColorComponents visibleTextfields;

	private Widgets.ColorComponents editableTextfields;

	protected static readonly Vector2 ButSize = new Vector2(150f, 38f);

	public override Vector2 InitialSize => new Vector2(600f, 450f);

	protected abstract bool ShowDarklight { get; }

	protected abstract Color DefaultColor { get; }

	protected abstract List<Color> PickableColors { get; }

	protected abstract float ForcedColorValue { get; }

	protected abstract bool ShowColorTemperatureBar { get; }

	protected abstract void SaveColor(Color color);

	public Dialog_ColorPickerBase(Widgets.ColorComponents visibleTextfields, Widgets.ColorComponents editableTextfields)
	{
		this.visibleTextfields = visibleTextfields;
		this.editableTextfields = editableTextfields;
		forcePause = true;
		absorbInputAroundWindow = true;
		closeOnClickedOutside = true;
		closeOnAccept = false;
	}

	private static void HeaderRow(ref RectDivider layout)
	{
		using (new TextBlock(GameFont.Medium))
		{
			TaggedString taggedString = "ChooseAColor".Translate().CapitalizeFirst();
			RectDivider rectDivider = layout.NewRow(Text.CalcHeight(taggedString, layout.Rect.width));
			GUI.SetNextControlName(focusableControlNames[0]);
			Rect rect = rectDivider.Rect;
			rect.y -= 5f;
			Widgets.Label(rect, taggedString);
		}
	}

	private void BottomButtons(ref RectDivider layout)
	{
		RectDivider rectDivider = layout.NewRow(ButSize.y, VerticalJustification.Bottom);
		if (Widgets.ButtonText(rectDivider.NewCol(ButSize.x), "Cancel".Translate()))
		{
			Close();
		}
		if (Widgets.ButtonText(rectDivider.NewCol(ButSize.x, HorizontalJustification.Right), "Accept".Translate()))
		{
			SaveColor(color);
			Close();
		}
	}

	private void ColorPalette(ref RectDivider layout, ref Color color, Color defaultColor, bool showDarklight, out float paletteHeight)
	{
		using (new TextBlock(TextAnchor.MiddleLeft))
		{
			RectDivider rectDivider = layout;
			RectDivider rectDivider2 = rectDivider.NewCol(250f, HorizontalJustification.Right);
			int num = 26;
			RectDivider rectDivider3 = rectDivider2.NewRow(num);
			int num2 = 4;
			rectDivider3.Rect.SplitVertically(num2 * (num + 2), out var left, out var right);
			RectDivider rectDivider4 = new RectDivider(left, 195906069, new Vector2(10f, 2f));
			Widgets.ColorBox(rectDivider4.NewCol(num), ref color, defaultColor);
			Widgets.Label(rectDivider4, "Default".Translate().CapitalizeFirst());
			RectDivider rectDivider5 = new RectDivider(right, 195906069, new Vector2(10f, 2f));
			Color defaultDarklight = DarklightUtility.DefaultDarklight;
			Rect rect = rectDivider5.NewCol(num);
			if (showDarklight)
			{
				Widgets.ColorBox(rect, ref color, defaultDarklight);
				Widgets.Label(rectDivider5, "Darklight".Translate().CapitalizeFirst());
			}
			Widgets.ColorSelector(rectDivider2, ref color, PickableColors, out paletteHeight);
			paletteHeight += num + 2;
		}
	}

	private void ColorTextfields(ref RectDivider layout, out Vector2 size)
	{
		RectAggregator aggregator = new RectAggregator(new Rect(layout.Rect.position, new Vector2(125f, 0f)), 195906069);
		bool num = Widgets.ColorTextfields(ref aggregator, ref color, ref textfieldBuffers, ref textfieldColorBuffer, previousFocusedControlName, "colorTextfields", editableTextfields, visibleTextfields);
		size = aggregator.Rect.size;
		if (num)
		{
			Color.RGBToHSV(color, out var H, out var S, out var _);
			color = Color.HSVToRGB(H, S, ForcedColorValue);
		}
	}

	private static void ColorReadback(Rect rect, Color color, Color oldColor, bool showDarklight)
	{
		rect.SplitVertically((rect.width - 26f) / 2f, out var left, out var right);
		RectDivider rectDivider = new RectDivider(left, 195906069);
		TaggedString label = "CurrentColor".Translate().CapitalizeFirst();
		TaggedString label2 = "OldColor".Translate().CapitalizeFirst();
		float width = Mathf.Max(100f, label.GetWidthCached(), label2.GetWidthCached());
		RectDivider rectDivider2 = rectDivider.NewRow(Text.LineHeight);
		Widgets.Label(rectDivider2.NewCol(width), label);
		Widgets.DrawBoxSolid(rectDivider2, color);
		RectDivider rectDivider3 = rectDivider.NewRow(Text.LineHeight);
		Widgets.Label(rectDivider3.NewCol(width), label2);
		Widgets.DrawBoxSolid(rectDivider3, oldColor);
		RectDivider rectDivider4 = new RectDivider(right, 195906069);
		rectDivider4.NewCol(26f);
		if (showDarklight)
		{
			if (DarklightUtility.IsDarklight(color))
			{
				Widgets.Label(rectDivider4, "Darklight".Translate().CapitalizeFirst());
			}
			else
			{
				Widgets.Label(rectDivider4, "NotDarklight".Translate().CapitalizeFirst());
			}
		}
	}

	private static void TabControl()
	{
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
		{
			bool num = !Event.current.shift;
			Event.current.Use();
			string text = GUI.GetNameOfFocusedControl();
			if (text.NullOrEmpty())
			{
				text = focusableControlNames[0];
			}
			int num2 = focusableControlNames.IndexOf(text);
			if (num2 < 0)
			{
				num2 = focusableControlNames.Count;
			}
			num2 = ((!num) ? (num2 - 1) : (num2 + 1));
			if (num2 >= focusableControlNames.Count)
			{
				num2 = 0;
			}
			else if (num2 < 0)
			{
				num2 = focusableControlNames.Count - 1;
			}
			GUI.FocusControl(focusableControlNames[num2]);
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		using (TextBlock.Default())
		{
			RectDivider layout = new RectDivider(inRect, 195906069);
			HeaderRow(ref layout);
			layout.NewRow(0f);
			BottomButtons(ref layout);
			layout.NewRow(0f, VerticalJustification.Bottom);
			Color.RGBToHSV(DefaultColor, out var H, out var S, out var _);
			Color defaultColor = Color.HSVToRGB(H, S, ForcedColorValue);
			defaultColor.a = 1f;
			ColorPalette(ref layout, ref color, defaultColor, ShowDarklight, out var paletteHeight);
			ColorTextfields(ref layout, out var size);
			float height = Mathf.Max(paletteHeight, 128f, size.y);
			RectDivider rectDivider = layout.NewRow(height);
			rectDivider.NewCol(size.x);
			rectDivider.NewCol(250f, HorizontalJustification.Right);
			Widgets.HSVColorWheel(rectDivider.Rect.ContractedBy((rectDivider.Rect.width - 128f) / 2f, (rectDivider.Rect.height - 128f) / 2f), ref color, ref hsvColorWheelDragging, ForcedColorValue);
			layout.NewRow(10f);
			Rect rect = layout.NewRow(34f);
			if (ShowColorTemperatureBar)
			{
				Widgets.ColorTemperatureBar(rect, ref color, ref colorTemperatureDragging, ForcedColorValue);
			}
			layout.NewRow(26f);
			ColorReadback(layout, color, oldColor, ShowDarklight);
			TabControl();
			if (Event.current.type == EventType.Layout)
			{
				previousFocusedControlName = GUI.GetNameOfFocusedControl();
			}
		}
	}
}
