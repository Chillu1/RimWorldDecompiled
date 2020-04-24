using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Verse
{
	public static class ColoredText
	{
		private enum CaptureStage
		{
			Tag,
			Arg,
			Result
		}

		private static StringBuilder resultBuffer = new StringBuilder();

		private static StringBuilder tagBuffer = new StringBuilder();

		private static StringBuilder argBuffer = new StringBuilder();

		private static Dictionary<string, string> cache = new Dictionary<string, string>();

		private static CaptureStage capStage = CaptureStage.Result;

		private static Regex DaysRegex;

		private static Regex HoursRegex;

		private static Regex SecondsRegex;

		private static Regex ColonistCountRegex;

		public static readonly Color RedReadable = new Color(1f, 0.2f, 0.2f);

		public static readonly Color NameColor = GenColor.FromHex("d09b61");

		public static readonly Color CurrencyColor = GenColor.FromHex("dbb40c");

		public static readonly Color DateTimeColor = GenColor.FromHex("87f6f6");

		public static readonly Color FactionColor_Ally = GenColor.FromHex("00ff00");

		public static readonly Color FactionColor_Hostile = RedReadable;

		public static readonly Color FactionColor_Neutral = GenColor.FromHex("00bfff");

		public static readonly Color WarningColor = GenColor.FromHex("ff0000");

		public static readonly Color ColonistCountColor = GenColor.FromHex("dcffaf");

		private static readonly Regex CurrencyRegex = new Regex("[^>]\\$\\d+\\.?\\d*[^<)]");

		private static readonly Regex TagRegex = new Regex("<[^>]*>");

		private const string Digits = "\\d+\\.?\\d*";

		private const string Replacement = "$&";

		public static void ResetStaticData()
		{
			DaysRegex = new Regex(string.Format("[^>]" + "PeriodDays".Translate() + "[^<]", "\\d+\\.?\\d*"));
			HoursRegex = new Regex(string.Format("[^>]" + "PeriodHours".Translate() + "[^<]", "\\d+\\.?\\d*"));
			SecondsRegex = new Regex(string.Format("[^>]" + "PeriodSeconds".Translate() + "[^<]", "\\d+\\.?\\d*"));
			string str = "(" + FactionDefOf.PlayerColony.pawnSingular + "|" + FactionDefOf.PlayerColony.pawnsPlural + ")";
			ColonistCountRegex = new Regex("[^>]\\d+\\.?\\d* " + str + "[^<]");
		}

		public static void ClearCache()
		{
			cache.Clear();
		}

		public static TaggedString ApplyTag(this string s, TagType tagType, string arg = null)
		{
			if (arg == null)
			{
				return string.Format("<{0}>{1}</{0}>", tagType.ToString(), s);
			}
			return string.Format("<{0}={1}>{2}</{0}>", tagType.ToString(), arg, s);
		}

		public static TaggedString ApplyTag(this string s, Faction faction)
		{
			if (faction == null)
			{
				return s;
			}
			return s.ApplyTag(TagType.Faction, faction.GetUniqueLoadID());
		}

		public static string StripTags(this string s)
		{
			if (s.NullOrEmpty() || s.IndexOf('<') < 0)
			{
				return s;
			}
			return TagRegex.Replace(s, string.Empty);
		}

		public static string ResolveTags(this string str)
		{
			return Resolve(str);
		}

		public static string Resolve(TaggedString taggedStr)
		{
			if ((string)taggedStr == null)
			{
				return null;
			}
			string rawText = taggedStr.RawText;
			if (rawText == null)
			{
				return rawText;
			}
			if (cache.TryGetValue(rawText, out string _))
			{
				return cache[rawText];
			}
			resultBuffer.Length = 0;
			if (rawText.IndexOf('<') < 0)
			{
				resultBuffer.Append(rawText);
			}
			else
			{
				for (int i = 0; i < rawText.Length; i++)
				{
					char c = rawText[i];
					if (c == '<' && rawText.IndexOf('>', i) > i)
					{
						bool flag = false;
						int num = i;
						tagBuffer.Length = 0;
						argBuffer.Length = 0;
						capStage = CaptureStage.Tag;
						for (i++; i < rawText.Length; i++)
						{
							char c2 = rawText[i];
							if (c2 == '>')
							{
								capStage = CaptureStage.Result;
								if (flag)
								{
									string value2 = rawText.Substring(num, i - num + 1).SwapTagWithColor(tagBuffer.ToString(), argBuffer.ToString());
									resultBuffer.Append(value2);
									break;
								}
							}
							else if (c2 == '/')
							{
								flag = true;
							}
							if (capStage == CaptureStage.Arg)
							{
								argBuffer.Append(c2);
							}
							if (!flag && c2 == '=')
							{
								capStage = CaptureStage.Arg;
							}
							if (capStage == CaptureStage.Tag)
							{
								tagBuffer.Append(c2);
							}
						}
						if (!flag)
						{
							resultBuffer.Append(c);
							i = num + 1;
						}
					}
					else
					{
						resultBuffer.Append(c);
					}
				}
			}
			string input = resultBuffer.ToString();
			input = CurrencyRegex.Replace(input, "$&".Colorize(CurrencyColor));
			input = DaysRegex.Replace(input, "$&".Colorize(DateTimeColor));
			input = HoursRegex.Replace(input, "$&".Colorize(DateTimeColor));
			input = SecondsRegex.Replace(input, "$&".Colorize(DateTimeColor));
			input = ColonistCountRegex.Replace(input, "$&".Colorize(ColonistCountColor));
			cache.Add(rawText, input);
			return input;
		}

		public static string Colorize(this string s, Color color)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{s}</color>";
		}

		private static string SwapTagWithColor(this string str, string tag, string arg)
		{
			TagType tagType = ParseEnum<TagType>(tag.CapitalizeFirst());
			if (tagType == TagType.Color)
			{
				return str;
			}
			string text = str.StripTags();
			switch (tagType)
			{
			case TagType.Undefined:
				return str;
			case TagType.Name:
				return text.Colorize(NameColor);
			case TagType.Faction:
			{
				if (arg.NullOrEmpty())
				{
					return text;
				}
				Faction faction2 = Find.FactionManager.AllFactions.ToList().Find((Faction x) => x.GetUniqueLoadID() == arg);
				if (faction2 == null)
				{
					Log.Error("No faction found with UniqueLoadID '" + arg + "'");
				}
				return text.Colorize(GetFactionRelationColor(faction2));
			}
			case TagType.Settlement:
			{
				if (arg.NullOrEmpty())
				{
					return text;
				}
				Faction faction = Find.FactionManager.AllFactionsVisible.ToList().Find((Faction x) => x.GetUniqueLoadID() == arg);
				if (faction == null)
				{
					Log.Error("No faction found with UniqueLoadID '" + arg + "'");
				}
				if (faction == null)
				{
					return text;
				}
				return text.Colorize(faction.Color);
			}
			case TagType.DateTime:
				return text.Colorize(DateTimeColor);
			case TagType.ColonistCount:
				return text.Colorize(ColonistCountColor);
			case TagType.Color:
				return str;
			default:
				Log.Error("Invalid tag '" + tag + "'");
				return text;
			}
		}

		private static Color GetFactionRelationColor(Faction faction)
		{
			if (faction == null)
			{
				return Color.white;
			}
			if (faction.IsPlayer)
			{
				return faction.Color;
			}
			switch (faction.RelationKindWith(Faction.OfPlayer))
			{
			case FactionRelationKind.Ally:
				return FactionColor_Ally;
			case FactionRelationKind.Hostile:
				return FactionColor_Hostile;
			case FactionRelationKind.Neutral:
				return FactionColor_Neutral;
			default:
				return faction.Color;
			}
		}

		private static T ParseEnum<T>(string value, bool ignoreCase = true)
		{
			if (Enum.IsDefined(typeof(T), value))
			{
				return (T)Enum.Parse(typeof(T), value, ignoreCase);
			}
			return default(T);
		}

		public static void AppendTagged(this StringBuilder sb, TaggedString taggedString)
		{
			sb.Append(taggedString.Resolve());
		}

		public static void AppendLineTagged(this StringBuilder sb, TaggedString taggedString)
		{
			sb.AppendLine(taggedString.Resolve());
		}

		public static TaggedString ToTaggedString(this StringBuilder sb)
		{
			return new TaggedString(sb.ToString());
		}
	}
}
