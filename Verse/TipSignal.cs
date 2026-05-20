using System;

namespace Verse;

public struct TipSignal
{
	public const float DefaultDelay = 0.45f;

	public string text;

	public Func<string> textGetter;

	public int uniqueId;

	public TooltipPriority priority;

	public float delay;

	public TipSignal(string text, int uniqueId)
	{
		this.text = text;
		textGetter = null;
		this.uniqueId = uniqueId;
		priority = TooltipPriority.Default;
		delay = 0.45f;
	}

	public TipSignal(string text, int uniqueId, TooltipPriority priority)
	{
		this.text = text;
		textGetter = null;
		this.uniqueId = uniqueId;
		this.priority = priority;
		delay = 0.45f;
	}

	public TipSignal(string text)
	{
		if (text == null)
		{
			text = "";
		}
		this.text = text;
		textGetter = null;
		uniqueId = text.GetHashCode();
		priority = TooltipPriority.Default;
		delay = 0.45f;
	}

	public TipSignal(string text, float delay)
	{
		if (text == null)
		{
			text = "";
		}
		this.text = text;
		textGetter = null;
		uniqueId = text.GetHashCode();
		priority = TooltipPriority.Default;
		this.delay = delay;
	}

	public TipSignal(TaggedString text)
	{
		if ((string)text == null)
		{
			text = "";
		}
		this.text = text.Resolve();
		textGetter = null;
		uniqueId = text.GetHashCode();
		priority = TooltipPriority.Default;
		delay = 0.45f;
	}

	public TipSignal(Func<string> textGetter, int uniqueId)
	{
		text = "";
		this.textGetter = textGetter;
		this.uniqueId = uniqueId;
		priority = TooltipPriority.Default;
		delay = 0.45f;
	}

	public TipSignal(Func<string> textGetter, int uniqueId, TooltipPriority priority)
	{
		text = "";
		this.textGetter = textGetter;
		this.uniqueId = uniqueId;
		this.priority = priority;
		delay = 0.45f;
	}

	public TipSignal(TipSignal cloneSource)
	{
		text = cloneSource.text;
		textGetter = null;
		priority = cloneSource.priority;
		uniqueId = cloneSource.uniqueId;
		delay = 0.45f;
	}

	public static implicit operator TipSignal(string str)
	{
		return new TipSignal(str);
	}

	public static implicit operator TipSignal(TaggedString str)
	{
		return new TipSignal(str);
	}

	public override string ToString()
	{
		return "Tip(" + text + ", " + uniqueId + ")";
	}
}
