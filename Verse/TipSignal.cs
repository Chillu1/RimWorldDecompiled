using System;

namespace Verse
{
	public struct TipSignal
	{
		public string text;

		public Func<string> textGetter;

		public int uniqueId;

		public TooltipPriority priority;

		public TipSignal(string text, int uniqueId)
		{
			this.text = text;
			textGetter = null;
			this.uniqueId = uniqueId;
			priority = TooltipPriority.Default;
		}

		public TipSignal(string text, int uniqueId, TooltipPriority priority)
		{
			this.text = text;
			textGetter = null;
			this.uniqueId = uniqueId;
			this.priority = priority;
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
		}

		public TipSignal(Func<string> textGetter, int uniqueId)
		{
			text = "";
			this.textGetter = textGetter;
			this.uniqueId = uniqueId;
			priority = TooltipPriority.Default;
		}

		public TipSignal(Func<string> textGetter, int uniqueId, TooltipPriority priority)
		{
			text = "";
			this.textGetter = textGetter;
			this.uniqueId = uniqueId;
			this.priority = priority;
		}

		public TipSignal(TipSignal cloneSource)
		{
			text = cloneSource.text;
			textGetter = null;
			priority = cloneSource.priority;
			uniqueId = cloneSource.uniqueId;
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
}
