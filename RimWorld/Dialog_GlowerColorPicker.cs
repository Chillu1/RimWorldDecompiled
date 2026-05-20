using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_GlowerColorPicker : Dialog_ColorPickerBase
{
	protected const float GlowValue = 1f;

	private static readonly List<Color> colors = new List<Color>
	{
		Color.HSVToRGB(0f, 0f, 1f),
		Color.HSVToRGB(0f, 0.5f, 1f),
		Color.HSVToRGB(0f, 0.33f, 1f),
		Color.HSVToRGB(1f / 18f, 1f, 1f),
		Color.HSVToRGB(1f / 18f, 0.5f, 1f),
		Color.HSVToRGB(1f / 18f, 0.33f, 1f),
		Color.HSVToRGB(1f / 9f, 1f, 1f),
		Color.HSVToRGB(1f / 9f, 0.5f, 1f),
		Color.HSVToRGB(1f / 9f, 0.33f, 1f),
		Color.HSVToRGB(1f / 6f, 1f, 1f),
		Color.HSVToRGB(1f / 6f, 0.5f, 1f),
		Color.HSVToRGB(1f / 6f, 0.33f, 1f),
		Color.HSVToRGB(2f / 9f, 1f, 1f),
		Color.HSVToRGB(2f / 9f, 0.5f, 1f),
		Color.HSVToRGB(2f / 9f, 0.33f, 1f),
		Color.HSVToRGB(5f / 18f, 1f, 1f),
		Color.HSVToRGB(5f / 18f, 0.5f, 1f),
		Color.HSVToRGB(5f / 18f, 0.33f, 1f),
		Color.HSVToRGB(1f / 3f, 1f, 1f),
		Color.HSVToRGB(1f / 3f, 0.5f, 1f),
		Color.HSVToRGB(1f / 3f, 0.33f, 1f),
		Color.HSVToRGB(7f / 18f, 1f, 1f),
		Color.HSVToRGB(7f / 18f, 0.5f, 1f),
		Color.HSVToRGB(7f / 18f, 0.33f, 1f),
		Color.HSVToRGB(4f / 9f, 1f, 1f),
		Color.HSVToRGB(4f / 9f, 0.5f, 1f),
		Color.HSVToRGB(4f / 9f, 0.33f, 1f),
		Color.HSVToRGB(0.5f, 1f, 1f),
		Color.HSVToRGB(0.5f, 0.5f, 1f),
		Color.HSVToRGB(0.5f, 0.33f, 1f),
		Color.HSVToRGB(5f / 9f, 1f, 1f),
		Color.HSVToRGB(5f / 9f, 0.5f, 1f),
		Color.HSVToRGB(5f / 9f, 0.33f, 1f),
		Color.HSVToRGB(11f / 18f, 1f, 1f),
		Color.HSVToRGB(11f / 18f, 0.5f, 1f),
		Color.HSVToRGB(11f / 18f, 0.33f, 1f),
		Color.HSVToRGB(2f / 3f, 1f, 1f),
		Color.HSVToRGB(2f / 3f, 0.5f, 1f),
		Color.HSVToRGB(2f / 3f, 0.33f, 1f),
		Color.HSVToRGB(13f / 18f, 1f, 1f),
		Color.HSVToRGB(13f / 18f, 0.5f, 1f),
		Color.HSVToRGB(13f / 18f, 0.33f, 1f),
		Color.HSVToRGB(7f / 9f, 1f, 1f),
		Color.HSVToRGB(7f / 9f, 0.5f, 1f),
		Color.HSVToRGB(7f / 9f, 0.33f, 1f),
		Color.HSVToRGB(5f / 6f, 1f, 1f),
		Color.HSVToRGB(5f / 6f, 0.5f, 1f),
		Color.HSVToRGB(5f / 6f, 0.33f, 1f),
		Color.HSVToRGB(8f / 9f, 1f, 1f),
		Color.HSVToRGB(8f / 9f, 0.5f, 1f),
		Color.HSVToRGB(8f / 9f, 0.33f, 1f),
		Color.HSVToRGB(17f / 18f, 1f, 1f),
		Color.HSVToRGB(17f / 18f, 0.5f, 1f),
		Color.HSVToRGB(17f / 18f, 0.33f, 1f)
	};

	private CompGlower glower;

	private CompGlower[] extraGlowers;

	protected override Color DefaultColor => glower.Props.glowColor.ToColor;

	protected override bool ShowDarklight { get; } = true;

	protected override List<Color> PickableColors => colors;

	protected override float ForcedColorValue => 1f;

	protected override bool ShowColorTemperatureBar => true;

	public Dialog_GlowerColorPicker(CompGlower glower, IList<CompGlower> extraGlowers, Widgets.ColorComponents visibleTextfields, Widgets.ColorComponents editableTextfields)
		: base(visibleTextfields, editableTextfields)
	{
		this.glower = glower;
		this.extraGlowers = new CompGlower[extraGlowers.Count];
		extraGlowers.CopyTo(this.extraGlowers, 0);
		Color.RGBToHSV(glower.GlowColor.ToColor, out var H, out var S, out var _);
		color = Color.HSVToRGB(H, S, 1f);
		oldColor = color;
	}

	protected override void SaveColor(Color color)
	{
		Color.RGBToHSV(color, out var H, out var S, out var _);
		ColorInt glowColor = glower.GlowColor;
		glowColor.SetHueSaturation(H, S);
		glower.GlowColor = glowColor;
		CompGlower[] array = extraGlowers;
		foreach (CompGlower obj in array)
		{
			glowColor = obj.GlowColor;
			glowColor.SetHueSaturation(H, S);
			obj.GlowColor = glowColor;
		}
	}
}
