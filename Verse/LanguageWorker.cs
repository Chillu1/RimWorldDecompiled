using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Verse
{
	public abstract class LanguageWorker
	{
		private static Regex ParenthesisRegex = new Regex("\\((.*?)\\)");

		private static readonly Regex replaceArgRegex = new Regex("(?<old>[^\"]*?)\"-\"(?<new>[^\"]*?)\"", RegexOptions.Compiled);

		public virtual int TotalNumCaseCount => 0;

		public virtual string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (str.NullOrEmpty())
			{
				return "";
			}
			if (name)
			{
				return str;
			}
			if ("IndefiniteForm".CanTranslate())
			{
				return "IndefiniteForm".Translate(str);
			}
			return "IndefiniteArticle".Translate() + " " + str;
		}

		public string WithIndefiniteArticle(string str, bool plural = false, bool name = false)
		{
			return WithIndefiniteArticle(str, LanguageDatabase.activeLanguage.ResolveGender(str), plural, name);
		}

		public string WithIndefiniteArticlePostProcessed(string str, Gender gender, bool plural = false, bool name = false)
		{
			return PostProcessed(WithIndefiniteArticle(str, gender, plural, name));
		}

		public string WithIndefiniteArticlePostProcessed(string str, bool plural = false, bool name = false)
		{
			return PostProcessed(WithIndefiniteArticle(str, plural, name));
		}

		public virtual string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (str.NullOrEmpty())
			{
				return "";
			}
			if (name)
			{
				return str;
			}
			if ("DefiniteForm".CanTranslate())
			{
				return "DefiniteForm".Translate(str);
			}
			return "DefiniteArticle".Translate() + " " + str;
		}

		public string WithDefiniteArticle(string str, bool plural = false, bool name = false)
		{
			return WithDefiniteArticle(str, LanguageDatabase.activeLanguage.ResolveGender(str), plural, name);
		}

		public string WithDefiniteArticlePostProcessed(string str, Gender gender, bool plural = false, bool name = false)
		{
			return PostProcessed(WithDefiniteArticle(str, gender, plural, name));
		}

		public string WithDefiniteArticlePostProcessed(string str, bool plural = false, bool name = false)
		{
			return PostProcessed(WithDefiniteArticle(str, plural, name));
		}

		public virtual string OrdinalNumber(int number, Gender gender = Gender.None)
		{
			return number.ToString();
		}

		public virtual string PostProcessed(string str)
		{
			return str.MergeMultipleSpaces();
		}

		public virtual string ToTitleCase(string str)
		{
			return str.CapitalizeFirst();
		}

		public virtual string Pluralize(string str, Gender gender, int count = -1)
		{
			if (TryLookupPluralForm(str, gender, out var plural, count))
			{
				return plural;
			}
			return str;
		}

		public string Pluralize(string str, int count = -1)
		{
			return Pluralize(str, LanguageDatabase.activeLanguage.ResolveGender(str), count);
		}

		public virtual bool TryLookupPluralForm(string str, Gender gender, out string plural, int count = -1)
		{
			plural = null;
			if (str.NullOrEmpty() || (count != -1 && count < 2))
			{
				return false;
			}
			Dictionary<string, string[]> lookupTable = LanguageDatabase.activeLanguage.WordInfo.GetLookupTable("plural");
			if (lookupTable == null)
			{
				return false;
			}
			string key = str.ToLower();
			if (!lookupTable.ContainsKey(key))
			{
				return false;
			}
			string[] array = lookupTable[key];
			if (array.Length < 2)
			{
				return false;
			}
			plural = array[1];
			if (str.Length != 0 && char.IsUpper(str[0]))
			{
				plural = plural.CapitalizeFirst();
			}
			return true;
		}

		public virtual bool TryLookUp(string tableName, string keyName, int index, out string result, string fullStringForReference = null)
		{
			result = null;
			Dictionary<string, string[]> lookupTable = LanguageDatabase.activeLanguage.WordInfo.GetLookupTable(tableName);
			if (lookupTable == null)
			{
				return false;
			}
			if (keyName.NullOrEmpty())
			{
				if (DebugSettings.logTranslationLookupErrors)
				{
					Log.Warning("Tried to lookup an empty key in table '" + tableName + "'.");
				}
				result = keyName;
				return true;
			}
			string text = keyName.ToLower();
			if (!lookupTable.ContainsKey(text))
			{
				ParenthesisRegex.Replace(text, "");
				text = text.Trim();
				if (!lookupTable.ContainsKey(text))
				{
					if (DebugSettings.logTranslationLookupErrors)
					{
						Log.Warning("Tried a lookup for key '" + keyName + "' in table '" + tableName + "', which doesn't exist.");
					}
					result = keyName;
					return true;
				}
			}
			string[] array = lookupTable[text];
			if (array.Length < index + 1)
			{
				if (DebugSettings.logTranslationLookupErrors)
				{
					Log.Warning($"Tried a lookup an out-of-bounds index '{index}' for key '{keyName}' in table '{tableName}'.");
				}
				result = keyName;
				return true;
			}
			result = array[index];
			return true;
		}

		public virtual string PostProcessThingLabelForRelic(string thingLabel)
		{
			if (thingLabel.IndexOf(' ') != -1)
			{
				return null;
			}
			return thingLabel;
		}

		public virtual string ResolveNumCase(float number, List<string> args)
		{
			string formOne = args[0].Trim('\'');
			string text = args[1].Trim('\'');
			string formMany = args[2].Trim('\'');
			if (number - Mathf.Floor(number) > float.Epsilon)
			{
				return number + " " + text;
			}
			return number + " " + GetFormForNumber((int)number, formOne, text, formMany);
		}

		protected virtual string GetFormForNumber(int num, string formOne, string formSeveral, string formMany)
		{
			int num2 = num % 10;
			if (num / 10 % 10 == 1)
			{
				return formMany;
			}
			switch (num2)
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

		public virtual string ResolveReplace(List<string> args)
		{
			if (args.Count == 0)
			{
				return null;
			}
			string text = args[0];
			if (args.Count == 1)
			{
				return text;
			}
			for (int i = 1; i < args.Count; i++)
			{
				string input = args[i];
				Match match = replaceArgRegex.Match(input);
				if (!match.Success)
				{
					return null;
				}
				string value = match.Groups["old"].Value;
				string value2 = match.Groups["new"].Value;
				if (text.Contains(value))
				{
					return text.Replace(value, value2);
				}
			}
			return text;
		}

		public virtual string ResolveFunction(string functionName, List<string> args, string fullStringForReference)
		{
			if (functionName == "lookup")
			{
				return ResolveLookup(args, fullStringForReference);
			}
			if (functionName == "replace")
			{
				return ResolveReplace(args);
			}
			return "";
		}

		protected string ResolveLookup(List<string> args, string fullStringForReference)
		{
			if (args.Count != 2 && args.Count != 3)
			{
				Log.Error("Invalid argument number for 'lookup' function, expected 2 or 3. A key to lookup, table name and optional index if there's more than 1 entry per key. Full string: " + fullStringForReference);
				return "";
			}
			string text = args[1];
			int result = 1;
			if (args.Count == 3 && !int.TryParse(args[2], out result))
			{
				Log.Error("Invalid lookup index value: '" + args[2] + "' Full string: " + fullStringForReference);
				return "";
			}
			if (TryLookUp(text.ToLower(), args[0], result, out var result2, fullStringForReference))
			{
				return result2;
			}
			return "";
		}
	}
}
