using System;
using UnityEngine;
using Verse;

namespace LudeonTK;

public abstract class Window_DevListing : Window_Dev
{
	private Rect container;

	private float y;

	private bool dirty;

	public override bool IsDebug => true;

	protected override float Margin => 4f;

	public override Vector2 InitialSize => new Vector2(275f, 400f);

	public virtual bool AutoUpdate => true;

	protected bool Dirty
	{
		get
		{
			return dirty;
		}
		set
		{
			dirty = value || dirty;
		}
	}

	protected Window_DevListing()
	{
		draggable = true;
		focusWhenOpened = false;
		drawShadow = false;
		closeOnAccept = false;
		closeOnCancel = false;
		preventCameraMotion = false;
		drawInScreenshotMode = false;
		onlyDrawInDevMode = true;
		doCloseX = true;
	}

	public sealed override void DoWindowContents(Rect inRect)
	{
		y = 4f;
		dirty = false;
		container = inRect;
		using (new TextBlock(GameFont.Tiny))
		{
			DoWindowListing();
		}
		windowRect.height = y + 4f;
		if (dirty && AutoUpdate)
		{
			OnChanged();
		}
	}

	protected abstract void DoWindowListing();

	protected virtual void OnChanged()
	{
		dirty = false;
	}

	protected void PrintLabel(string text, GameFont font = GameFont.Tiny)
	{
		using (new TextBlock(font))
		{
			DevGUI.Label(new Rect(container.x, y, container.width, 20f), text);
			y += 20f;
		}
	}

	protected bool SliderFieldInt(string label, ref float sliderValue, ref int value, int min = 0, int max = 1, string tooltip = null)
	{
		Rect rect = TakeRow();
		Rect rect2 = rect.LeftHalf();
		Rect rect3 = rect.RightHalf();
		rect3.yMin += rect3.height / 4f;
		DevGUI.Label(rect2, label);
		if (!string.IsNullOrEmpty(tooltip))
		{
			TooltipHandler.TipRegion(rect2, tooltip);
		}
		sliderValue = DevGUI.HorizontalSlider(rect3, sliderValue, min, max);
		int num = Mathf.RoundToInt(sliderValue);
		if (num != value)
		{
			value = num;
			dirty = true;
			return true;
		}
		return false;
	}

	protected bool SliderFieldFloat(string label, ref float value, float min = 0f, float max = 1f, string tooltip = null)
	{
		Rect rect = TakeRow();
		Rect rect2 = rect.LeftHalf();
		Rect rect3 = rect.RightHalf();
		rect3.yMin += rect3.height / 4f;
		DevGUI.Label(rect2, label);
		if (!string.IsNullOrEmpty(tooltip))
		{
			TooltipHandler.TipRegion(rect2, tooltip);
		}
		float b = value;
		value = DevGUI.HorizontalSlider(rect3, value, min, max);
		if (!Mathf.Approximately(value, b))
		{
			dirty = true;
			return true;
		}
		return false;
	}

	protected bool SliderFieldFloat(string label, ref float value, ref bool enabled, float min = 0f, float max = 1f, string tooltip = null)
	{
		Rect rect = TakeRow();
		Rect rect2 = rect.LeftHalf();
		Rect rect3 = rect.RightHalf();
		Rect rect4 = rect3.RightPartPixels(rect.height);
		rect3.yMin += rect3.height / 4f;
		rect3.xMax = rect4.x;
		DevGUI.Label(rect2, label);
		bool flag = enabled;
		DevGUI.Checkbox(rect4, ref enabled);
		if (enabled != flag)
		{
			dirty = true;
		}
		if (!string.IsNullOrEmpty(tooltip))
		{
			TooltipHandler.TipRegion(rect2, tooltip);
		}
		float b = value;
		value = DevGUI.HorizontalSlider(rect3, value, min, max);
		if (!Mathf.Approximately(value, b))
		{
			dirty = true;
			return true;
		}
		return false;
	}

	protected bool SliderFieldDouble(string label, ref double value, ref bool fine, float min = 0f, float max = 1f, string tooltip = null)
	{
		Rect rect = TakeRow();
		Rect rect2 = rect.LeftHalf();
		Rect rect3 = rect.RightHalf();
		Rect rect4 = rect3.RightPartPixels(rect.height);
		rect3.yMin += rect3.height / 4f;
		rect3.xMax = rect4.x;
		bool flag = fine;
		DevGUI.Checkbox(rect4, ref fine);
		TooltipHandler.TipRegion(rect4, "Toggle fine value adjustment");
		if (fine)
		{
			max /= 10f;
		}
		if (((fine != flag) & fine) && value > (double)max)
		{
			value = max;
			dirty = true;
		}
		DevGUI.Label(rect2, label);
		if (!string.IsNullOrEmpty(tooltip))
		{
			TooltipHandler.TipRegion(rect2, tooltip);
		}
		float num = (float)value;
		float num2 = DevGUI.HorizontalSlider(rect3, num, min, max);
		if (!Mathf.Approximately(num, num2))
		{
			value = num2;
			dirty = true;
			return true;
		}
		return false;
	}

	protected bool TextFieldInt(string label, ref string source, ref int value, string tooltip = null)
	{
		Rect rect = TakeRow();
		Rect rect2 = rect.LeftHalf();
		Rect rect3 = rect.RightHalf();
		DevGUI.Label(rect2, label);
		source = DevGUI.TextField(rect3, source);
		if (!string.IsNullOrEmpty(tooltip))
		{
			TooltipHandler.TipRegion(rect2, tooltip);
		}
		if (int.TryParse(source, out var result) && result != value)
		{
			value = result;
			dirty = true;
			return true;
		}
		return false;
	}

	protected bool TextFieldDouble(string label, ref string source, ref double value, string tooltip = null)
	{
		Rect rect = TakeRow();
		Rect rect2 = rect.LeftHalf();
		Rect rect3 = rect.RightHalf();
		DevGUI.Label(rect2, label);
		source = DevGUI.TextField(rect3, source);
		if (!string.IsNullOrEmpty(tooltip))
		{
			TooltipHandler.TipRegion(rect2, tooltip);
		}
		if (double.TryParse(source, out var result) && Math.Abs(result - value) > 9.999999747378752E-05)
		{
			value = result;
			dirty = true;
			return true;
		}
		return false;
	}

	protected bool Button(string label)
	{
		if (DevGUI.ButtonText(TakeRow(), label))
		{
			dirty = true;
			return true;
		}
		return false;
	}

	protected bool Toggle(string label, ref bool value)
	{
		y += 4f;
		if (DevGUI.ButtonText(TakeRow(), label))
		{
			value = !value;
			dirty = true;
			return true;
		}
		return false;
	}

	protected void Gap(float height)
	{
		y += height;
	}

	protected Rect TakeRow()
	{
		Rect result = new Rect(container.x, y, container.width, 20f);
		y += 24f;
		return result;
	}
}
