namespace Verse
{
	public class NameSingle : Name
	{
		private string nameInt;

		private bool numerical;

		public string Name => nameInt;

		public override string ToStringFull => nameInt;

		public override string ToStringShort => nameInt;

		public override bool IsValid => !nameInt.NullOrEmpty();

		public override bool Numerical => numerical;

		private int FirstDigitPosition
		{
			get
			{
				if (!numerical)
				{
					return -1;
				}
				if (nameInt.NullOrEmpty() || !char.IsDigit(nameInt[nameInt.Length - 1]))
				{
					return -1;
				}
				for (int num = nameInt.Length - 2; num >= 0; num--)
				{
					if (!char.IsDigit(nameInt[num]))
					{
						return num + 1;
					}
				}
				return 0;
			}
		}

		public string NameWithoutNumber
		{
			get
			{
				if (!numerical)
				{
					return nameInt;
				}
				int firstDigitPosition = FirstDigitPosition;
				if (firstDigitPosition < 0)
				{
					return nameInt;
				}
				int num = firstDigitPosition;
				if (num - 1 >= 0 && nameInt[num - 1] == ' ')
				{
					num--;
				}
				if (num <= 0)
				{
					return "";
				}
				return nameInt.Substring(0, num);
			}
		}

		public int Number
		{
			get
			{
				if (!numerical)
				{
					return 0;
				}
				int firstDigitPosition = FirstDigitPosition;
				if (firstDigitPosition < 0)
				{
					return 0;
				}
				return int.Parse(nameInt.Substring(firstDigitPosition));
			}
		}

		public NameSingle()
		{
		}

		public NameSingle(string name, bool numerical = false)
		{
			nameInt = name;
			this.numerical = numerical;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref nameInt, "name");
			Scribe_Values.Look(ref numerical, "numerical", defaultValue: false);
		}

		public override bool ConfusinglySimilarTo(Name other)
		{
			NameSingle nameSingle = other as NameSingle;
			if (nameSingle != null && nameSingle.nameInt == nameInt)
			{
				return true;
			}
			NameTriple nameTriple = other as NameTriple;
			if (nameTriple != null && nameTriple.Nick == nameInt)
			{
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return nameInt;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (!(obj is NameSingle))
			{
				return false;
			}
			NameSingle nameSingle = (NameSingle)obj;
			return nameInt == nameSingle.nameInt;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineInt(nameInt.GetHashCode(), 1384661390);
		}
	}
}
