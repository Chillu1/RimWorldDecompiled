namespace Verse
{
	public class LanguageWorker_German : LanguageWorker
	{
		public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return str;
			}
			return gender switch
			{
				Gender.Male => "ein " + str, 
				Gender.Female => "eine " + str, 
				Gender.None => "ein " + str, 
				_ => str, 
			};
		}

		public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return str;
			}
			return gender switch
			{
				Gender.Male => "der " + str, 
				Gender.Female => "die " + str, 
				Gender.None => "das " + str, 
				_ => str, 
			};
		}

		public override string OrdinalNumber(int number, Gender gender = Gender.None)
		{
			return number + ".";
		}

		public override string Pluralize(string str, Gender gender, int count = -1)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			char c = str[str.Length - 1];
			char c2 = ((str.Length >= 2) ? str[str.Length - 2] : '\0');
			switch (gender)
			{
			case Gender.Male:
				if (c == 'r' && c2 == 'e')
				{
					return str;
				}
				if (c == 'l' && c2 == 'e')
				{
					return str;
				}
				if (c == 'R' && c2 == 'E')
				{
					return str;
				}
				if (c == 'L' && c2 == 'E')
				{
					return str;
				}
				if (char.IsUpper(c))
				{
					return str + "E";
				}
				return str + "e";
			case Gender.Female:
				switch (c)
				{
				case 'e':
					return str + "n";
				case 'E':
					return str + "N";
				case 'n':
					if (c2 == 'i')
					{
						return str + "nen";
					}
					break;
				}
				if (c == 'N' && c2 == 'I')
				{
					return str + "NEN";
				}
				if (char.IsUpper(c))
				{
					return str + "EN";
				}
				return str + "en";
			case Gender.None:
				if (c == 'r' && c2 == 'e')
				{
					return str;
				}
				if (c == 'l' && c2 == 'e')
				{
					return str;
				}
				if (c == 'n' && c2 == 'e')
				{
					return str;
				}
				if (c == 'R' && c2 == 'E')
				{
					return str;
				}
				if (c == 'L' && c2 == 'E')
				{
					return str;
				}
				if (c == 'N' && c2 == 'E')
				{
					return str;
				}
				if (char.IsUpper(c))
				{
					return str + "EN";
				}
				return str + "en";
			default:
				return str;
			}
		}
	}
}
