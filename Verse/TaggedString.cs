namespace Verse
{
	public struct TaggedString
	{
		private string rawText;

		private static TaggedString empty;

		public string RawText => rawText;

		public char this[int i] => RawText[i];

		public int Length => RawText.Length;

		public int StrippedLength => RawText.StripTags().Length;

		public static TaggedString Empty
		{
			get
			{
				if ((string)empty == null)
				{
					empty = new TaggedString("");
				}
				return empty;
			}
		}

		public TaggedString(string dat)
		{
			rawText = dat;
		}

		public string Resolve()
		{
			return ColoredText.Resolve(this);
		}

		public TaggedString CapitalizeFirst()
		{
			return RawText.CapitalizeFirst();
		}

		public bool NullOrEmpty()
		{
			return RawText.NullOrEmpty();
		}

		public TaggedString AdjustedFor(Pawn p, string pawnSymbol = "PAWN", bool addRelationInfoSymbol = true)
		{
			return RawText.AdjustedFor(p, pawnSymbol, addRelationInfoSymbol);
		}

		public float GetWidthCached()
		{
			return RawText.StripTags().GetWidthCached();
		}

		public TaggedString Trim()
		{
			return new TaggedString(RawText.Trim());
		}

		public TaggedString Shorten()
		{
			rawText = rawText.Shorten();
			return this;
		}

		public TaggedString ToLower()
		{
			return new TaggedString(RawText.ToLower());
		}

		public TaggedString Replace(string oldValue, string newValue)
		{
			return new TaggedString(RawText.Replace(oldValue, newValue));
		}

		public static implicit operator string(TaggedString taggedString)
		{
			return taggedString.RawText.StripTags();
		}

		public static implicit operator TaggedString(string str)
		{
			return new TaggedString(str);
		}

		public static TaggedString operator +(TaggedString t1, TaggedString t2)
		{
			return new TaggedString(t1.RawText + t2.RawText);
		}

		public static TaggedString operator +(string t1, TaggedString t2)
		{
			return new TaggedString(t1 + t2.RawText);
		}

		public static TaggedString operator +(TaggedString t1, string t2)
		{
			return new TaggedString(t1.RawText + t2);
		}

		public override string ToString()
		{
			return RawText;
		}
	}
}
