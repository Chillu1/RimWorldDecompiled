using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public class LanguageWorker_French : LanguageWorker
	{
		private static readonly List<string> Exceptions1 = new List<string>
		{
			"bail",
			"corail",
			"émail",
			"gemmail",
			"soupirail",
			"travail",
			"vantail",
			"vitrail"
		};

		private static readonly List<string> Exceptions2 = new List<string>
		{
			"bleu",
			"émeu",
			"landau",
			"lieu",
			"pneu",
			"sarrau",
			"bal",
			"banal",
			"fatal",
			"final",
			"festival"
		};

		private static readonly List<string> Exceptions3 = new List<string>
		{
			"bijou",
			"caillou",
			"chou",
			"genou",
			"hibou",
			"joujou",
			"pou"
		};

		private static StringBuilder tmpStr = new StringBuilder();

		public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return str;
			}
			if (plural)
			{
				return "des " + str;
			}
			return ((gender == Gender.Female) ? "une " : "un ") + str;
		}

		public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			if (name)
			{
				return str;
			}
			if (plural)
			{
				return "les " + str;
			}
			char ch = str[0];
			if (IsVowel(ch))
			{
				return "l'" + str;
			}
			return ((gender == Gender.Female) ? "la " : "le ") + str;
		}

		public override string OrdinalNumber(int number, Gender gender = Gender.None)
		{
			if (number != 1)
			{
				return number + "e";
			}
			return number + "er";
		}

		public override string Pluralize(string str, Gender gender, int count = -1)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			string item = str.ToLower();
			if (Exceptions1.Contains(item))
			{
				return str.Substring(0, str.Length - 3) + "aux";
			}
			if (Exceptions2.Contains(item))
			{
				return str + "s";
			}
			if (Exceptions3.Contains(item))
			{
				return str + "x";
			}
			if (str[str.Length - 1] == 's' || str[str.Length - 1] == 'x' || str[str.Length - 1] == 'z')
			{
				return str;
			}
			if (str.Length >= 2 && str[str.Length - 2] == 'a' && str[str.Length - 1] == 'l')
			{
				return str.Substring(0, str.Length - 2) + "aux";
			}
			if (str.Length >= 2 && str[str.Length - 2] == 'a' && str[str.Length - 1] == 'u')
			{
				return str.Substring(0, str.Length - 2) + "x";
			}
			if (str.Length >= 2 && str[str.Length - 2] == 'e' && str[str.Length - 1] == 'u')
			{
				return str.Substring(0, str.Length - 2) + "x";
			}
			return str + "s";
		}

		public override string PostProcessed(string str)
		{
			return PostProcessedInt(base.PostProcessed(str));
		}

		public override string PostProcessedKeyedTranslation(string translation)
		{
			return PostProcessedInt(base.PostProcessedKeyedTranslation(translation));
		}

		public bool IsVowel(char ch)
		{
			return "hiueøoɛœəɔaãɛ\u0303œ\u0303ɔ\u0303IHUEØOƐŒƏƆAÃƐ\u0303Œ\u0303Ɔ\u0303".IndexOf(ch) >= 0;
		}

		private string PostProcessedInt(string str)
		{
			str = str.Replace(" de le ", " du ").Replace("De le ", "Du ").Replace(" de les ", " des ")
				.Replace("De les ", "Des ")
				.Replace(" de des ", " des ")
				.Replace("De des ", "Des ")
				.Replace(" à le ", " au ")
				.Replace("À le ", "Au ")
				.Replace(" à les ", " aux ")
				.Replace("À les ", "Aux ")
				.Replace(" si il ", " s'il ")
				.Replace("Si il ", "S'il ")
				.Replace(" si ils ", " s'ils ")
				.Replace("Si ils ", "S'ils ")
				.Replace(" que il ", " qu'il ")
				.Replace("Que il ", "Qu'il ")
				.Replace(" que ils ", " qu'ils ")
				.Replace("Que ils ", "Qu'ils ")
				.Replace(" lorsque il ", " lorsqu'il ")
				.Replace("Lorsque il ", "Lorsqu'il ")
				.Replace(" lorsque ils ", " lorsqu'ils ")
				.Replace("Lorsque ils ", "Lorsqu'ils ")
				.Replace(" que elle ", " qu'elle ")
				.Replace("Que elle ", "Qu'elle ")
				.Replace(" que elles ", " qu'elles ")
				.Replace("Que elles ", "Qu'elles ")
				.Replace(" lorsque elle ", " lorsqu'elle ")
				.Replace("Lorsque elle ", "Lorsqu'elle ")
				.Replace(" lorsque elles ", " lorsqu'elles ")
				.Replace("Lorsque elles ", "Lorsqu'elles ");
			tmpStr.Clear();
			tmpStr.Append(str);
			for (int i = 0; i < tmpStr.Length; i++)
			{
				if (i + 3 < tmpStr.Length && tmpStr[i] == 'D' && tmpStr[i + 1] == 'e' && tmpStr[i + 2] == ' ' && IsVowel(tmpStr[i + 3]))
				{
					tmpStr[i] = '\0';
					tmpStr[i + 1] = 'D';
					tmpStr[i + 2] = '\'';
				}
				else if (i + 3 < tmpStr.Length && tmpStr[i] == 'L' && tmpStr[i + 1] == 'e' && tmpStr[i + 2] == ' ' && IsVowel(tmpStr[i + 3]))
				{
					tmpStr[i] = '\0';
					tmpStr[i + 1] = 'L';
					tmpStr[i + 2] = '\'';
				}
				else if (i + 3 < tmpStr.Length && tmpStr[i] == 'L' && tmpStr[i + 1] == 'a' && tmpStr[i + 2] == ' ' && IsVowel(tmpStr[i + 3]))
				{
					tmpStr[i] = '\0';
					tmpStr[i + 1] = 'L';
					tmpStr[i + 2] = '\'';
				}
				else if (i + 4 < tmpStr.Length && tmpStr[i] == ' ' && tmpStr[i + 1] == 'd' && tmpStr[i + 2] == 'e' && tmpStr[i + 3] == ' ' && IsVowel(tmpStr[i + 4]))
				{
					tmpStr[i + 1] = '\0';
					tmpStr[i + 2] = 'd';
					tmpStr[i + 3] = '\'';
				}
				else if (i + 4 < tmpStr.Length && tmpStr[i] == ' ' && tmpStr[i + 1] == 'l' && tmpStr[i + 2] == 'e' && tmpStr[i + 3] == ' ' && IsVowel(tmpStr[i + 4]))
				{
					tmpStr[i + 1] = '\0';
					tmpStr[i + 2] = 'l';
					tmpStr[i + 3] = '\'';
				}
				else if (i + 4 < tmpStr.Length && tmpStr[i] == ' ' && tmpStr[i + 1] == 'l' && tmpStr[i + 2] == 'a' && tmpStr[i + 3] == ' ' && IsVowel(tmpStr[i + 4]))
				{
					tmpStr[i + 1] = '\0';
					tmpStr[i + 2] = 'l';
					tmpStr[i + 3] = '\'';
				}
			}
			str = tmpStr.ToString();
			str = str.Replace("\0", "");
			return str;
		}
	}
}
