using System.Text.RegularExpressions;

namespace Verse
{
	public class LanguageWorker_Russian : LanguageWorker
	{
		private interface IResolver
		{
			string Resolve(string[] arguments);
		}

		private class ReplaceResolver : IResolver
		{
			private static readonly Regex _argumentRegex = new Regex("'(?<old>[^']*?)'-'(?<new>[^']*?)'", RegexOptions.Compiled);

			public string Resolve(string[] arguments)
			{
				if (arguments.Length == 0)
				{
					return null;
				}
				string text = arguments[0];
				if (arguments.Length == 1)
				{
					return text;
				}
				for (int i = 1; i < arguments.Length; i++)
				{
					string input = arguments[i];
					Match match = _argumentRegex.Match(input);
					if (!match.Success)
					{
						return null;
					}
					string value = match.Groups["old"].Value;
					string value2 = match.Groups["new"].Value;
					if (value == text)
					{
						return value2;
					}
				}
				return text;
			}
		}

		private class NumberCaseResolver : IResolver
		{
			private static readonly Regex _numberRegex = new Regex("(?<floor>[0-9]+)(\\.(?<frac>[0-9]+))?", RegexOptions.Compiled);

			public string Resolve(string[] arguments)
			{
				if (arguments.Length != 4)
				{
					return null;
				}
				string text = arguments[0];
				Match match = _numberRegex.Match(text);
				if (!match.Success)
				{
					return null;
				}
				bool success = match.Groups["frac"].Success;
				string value = match.Groups["floor"].Value;
				string formOne = arguments[1].Trim('\'');
				string text2 = arguments[2].Trim('\'');
				string formMany = arguments[3].Trim('\'');
				if (success)
				{
					return text2.Replace("#", text);
				}
				return GetFormForNumber(int.Parse(value), formOne, text2, formMany).Replace("#", text);
			}

			private static string GetFormForNumber(int number, string formOne, string formSeveral, string formMany)
			{
				int num = number % 10;
				if (number / 10 % 10 == 1)
				{
					return formMany;
				}
				switch (num)
				{
				case 1:
					return formOne;
				case 2:
				case 3:
				case 4:
					return formSeveral;
				default:
					return formMany;
				}
			}
		}

		private static readonly ReplaceResolver replaceResolver = new ReplaceResolver();

		private static readonly NumberCaseResolver numberCaseResolver = new NumberCaseResolver();

		private static readonly Regex _languageWorkerResolverRegex = new Regex("\\^(?<resolverName>\\w+)\\(\\s*(?<argument>[^|]+?)\\s*(\\|\\s*(?<argument>[^|]+?)\\s*)*\\)\\^", RegexOptions.Compiled);

		public override string PostProcessedKeyedTranslation(string translation)
		{
			translation = base.PostProcessedKeyedTranslation(translation);
			return PostProcess(translation);
		}

		public override string PostProcessed(string str)
		{
			str = base.PostProcessed(str);
			return PostProcess(str);
		}

		private static string PostProcess(string translation)
		{
			return _languageWorkerResolverRegex.Replace(translation, EvaluateResolver);
		}

		public override string ToTitleCase(string str)
		{
			return GenText.ToTitleCaseSmart(str);
		}

		private static string EvaluateResolver(Match match)
		{
			string value = match.Groups["resolverName"].Value;
			Group group = match.Groups["argument"];
			string[] array = new string[group.Captures.Count];
			for (int i = 0; i < group.Captures.Count; i++)
			{
				array[i] = group.Captures[i].Value.Trim();
			}
			IResolver resolverByKeyword = GetResolverByKeyword(value);
			if (resolverByKeyword == null)
			{
				return match.Value;
			}
			string text = resolverByKeyword.Resolve(array);
			if (text == null)
			{
				Log.ErrorOnce($"Error happened while resolving LW instruction: \"{match.Value}\"", match.Value.GetHashCode() ^ 0x1654CDB0);
				return match.Value;
			}
			return text;
		}

		private static IResolver GetResolverByKeyword(string keyword)
		{
			if (!(keyword == "Replace"))
			{
				if (keyword == "Number")
				{
					return numberCaseResolver;
				}
				return null;
			}
			return replaceResolver;
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
			case Gender.None:
				switch (c)
				{
				case 'o':
					return str.Substring(0, str.Length - 1) + "a";
				case 'O':
					return str.Substring(0, str.Length - 1) + "A";
				case 'E':
				case 'e':
				{
					char value2 = char.ToUpper(c2);
					if ("ГКХЖЧШЩЦ".IndexOf(value2) >= 0)
					{
						switch (c)
						{
						case 'e':
							return str.Substring(0, str.Length - 1) + "a";
						case 'E':
							return str.Substring(0, str.Length - 1) + "A";
						}
					}
					else
					{
						switch (c)
						{
						case 'e':
							return str.Substring(0, str.Length - 1) + "я";
						case 'E':
							return str.Substring(0, str.Length - 1) + "Я";
						}
					}
					break;
				}
				}
				break;
			case Gender.Female:
				switch (c)
				{
				case 'я':
					return str.Substring(0, str.Length - 1) + "и";
				case 'ь':
					return str.Substring(0, str.Length - 1) + "и";
				case 'Я':
					return str.Substring(0, str.Length - 1) + "И";
				case 'Ь':
					return str.Substring(0, str.Length - 1) + "И";
				case 'A':
				case 'a':
				{
					char value = char.ToUpper(c2);
					if ("ГКХЖЧШЩ".IndexOf(value) >= 0)
					{
						if (c == 'a')
						{
							return str.Substring(0, str.Length - 1) + "и";
						}
						return str.Substring(0, str.Length - 1) + "И";
					}
					if (c == 'a')
					{
						return str.Substring(0, str.Length - 1) + "ы";
					}
					return str.Substring(0, str.Length - 1) + "Ы";
				}
				}
				break;
			case Gender.Male:
				if (IsConsonant(c))
				{
					return str + "ы";
				}
				switch (c)
				{
				case 'й':
					return str.Substring(0, str.Length - 1) + "и";
				case 'ь':
					return str.Substring(0, str.Length - 1) + "и";
				case 'Й':
					return str.Substring(0, str.Length - 1) + "И";
				case 'Ь':
					return str.Substring(0, str.Length - 1) + "И";
				}
				break;
			}
			return str;
		}

		private static bool IsConsonant(char ch)
		{
			return "бвгджзклмнпрстфхцчшщБВГДЖЗКЛМНПРСТФХЦЧШЩ".IndexOf(ch) >= 0;
		}
	}
}
