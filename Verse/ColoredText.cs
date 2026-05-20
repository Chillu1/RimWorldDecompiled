using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;

namespace Verse;

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

	private static Regex ColonistCountRegex;

	private static List<Regex> DateTimeRegexes;

	public static readonly Color NameColor = GenColor.FromHex("d09b61");

	public static readonly Color CurrencyColor = GenColor.FromHex("dbb40c");

	public static readonly Color TipSectionTitleColor = new Color(0.9f, 0.9f, 0.3f);

	public static readonly Color DateTimeColor = GenColor.FromHex("87f6f6");

	public static readonly Color FactionColor_Ally = GenColor.FromHex("00ff00");

	public static readonly Color FactionColor_Hostile = ColorLibrary.RedReadable;

	public static readonly Color ThreatColor = GenColor.FromHex("d46f68");

	public static readonly Color FactionColor_Neutral = GenColor.FromHex("00bfff");

	public static readonly Color WarningColor = GenColor.FromHex("ff0000");

	public static readonly Color ColonistCountColor = GenColor.FromHex("dcffaf");

	public static readonly Color SubtleGrayColor = GenColor.FromHex("999999");

	public static readonly Color ExpectationsColor = new Color(0.57f, 0.9f, 0.69f);

	public static readonly Color ImpactColor = GenColor.FromHex("c79fef");

	public static readonly Color GeneColor = ColorLibrary.LightBlue;

	private static readonly Regex CurrencyRegex = new Regex("\\$\\d+\\.?\\d*");

	private static readonly Regex TagRegex = new Regex("\\([\\*\\/][^\\)]*\\)");

	private static readonly Regex XMLRegex = new Regex("<[^>]*>");

	private const string Digits = "\\d+\\.?\\d*";

	private const string Replacement = "$&";

	private const string TagStartString = "(*";

	private const char TagStartChar = '(';

	private const char TagEndChar = ')';

	public static void ResetStaticData()
	{
		DateTimeRegexes = new List<Regex>();
		AddRegexPatternsForDateString("PeriodYears".Translate(), "Period1Year".Translate());
		AddRegexPatternsForDateString("PeriodQuadrums".Translate(), "Period1Quadrum".Translate());
		AddRegexPatternsForDateString("PeriodDays".Translate(), "Period1Day".Translate());
		AddRegexPatternsForDateString("PeriodHours".Translate(), "Period1Hour".Translate());
		AddRegexPatternsForDateString("PeriodSeconds".Translate(), "Period1Second".Translate());
		string text = "(" + FactionDefOf.PlayerColony.pawnsPlural + "|" + FactionDefOf.PlayerColony.pawnSingular + ")";
		ColonistCountRegex = new Regex("\\d+\\.?\\d* " + text);
	}

	private static void AddRegexPatternsForDateString(string dateMany, string dateOne)
	{
		if (dateMany.Contains("{0}"))
		{
			DateTimeRegexes.Add(new Regex("(" + string.Format(dateMany, "\\d+\\.?\\d*") + "|" + dateOne + ")"));
			return;
		}
		List<string> list = GrammarResolverSimple.TryParseNumCase(dateMany);
		if (list.NullOrEmpty())
		{
			return;
		}
		foreach (string item in list)
		{
			DateTimeRegexes.Add(new Regex("(\\d+\\.?\\d* " + item + "|" + dateOne + ")"));
		}
	}

	public static void ClearCache()
	{
		cache.Clear();
	}

	public static TaggedString ApplyTag(this string s, TagType tagType, string arg = null)
	{
		if (arg == null)
		{
			return string.Format("(*{0}){1}(/{0})", tagType.ToString(), s);
		}
		return string.Format("(*{0}={1}){2}(/{0})", tagType.ToString(), arg, s);
	}

	public static TaggedString ApplyTag(this string s, Faction faction)
	{
		if (faction == null)
		{
			return s;
		}
		return s.ApplyTag(TagType.Faction, faction.GetUniqueLoadID());
	}

	public static TaggedString ApplyTag(this string s, Ideo ideo)
	{
		if (ideo == null)
		{
			return s;
		}
		return s.ApplyTag(TagType.Ideo, ideo.GetUniqueLoadID());
	}

	public static string StripTags(this string s)
	{
		if (s.NullOrEmpty() || (s.IndexOf("(*") < 0 && s.IndexOf('<') < 0))
		{
			return s;
		}
		s = XMLRegex.Replace(s, string.Empty);
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
		if (rawText.NullOrEmpty())
		{
			return rawText;
		}
		if (cache.TryGetValue(rawText, out var value))
		{
			return value;
		}
		resultBuffer.Length = 0;
		if (rawText.IndexOf("(*") < 0)
		{
			resultBuffer.Append(rawText);
		}
		else
		{
			for (int i = 0; i < rawText.Length; i++)
			{
				char c = rawText[i];
				if (c == '(' && i < rawText.Length - 1 && rawText[i + 1] == '*' && rawText.IndexOf(')', i) > i + 1)
				{
					bool flag = false;
					int num = i;
					tagBuffer.Length = 0;
					argBuffer.Length = 0;
					capStage = CaptureStage.Tag;
					for (i += 2; i < rawText.Length; i++)
					{
						char c2 = rawText[i];
						if (c2 == ')')
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
		for (int j = 0; j < DateTimeRegexes.Count; j++)
		{
			input = DateTimeRegexes[j].Replace(input, "$&".Colorize(DateTimeColor));
		}
		input = CurrencyRegex.Replace(input, "$&".Colorize(CurrencyColor));
		input = ColonistCountRegex.Replace(input, "$&".Colorize(ColonistCountColor));
		cache.Add(rawText, input);
		return input;
	}

	public static string Colorize(this TaggedString ts, Color color)
	{
		return ts.Resolve().Colorize(color);
	}

	public static string Colorize(this string s, Color color)
	{
		return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{s}</color>";
	}

	private static string SwapTagWithColor(this string str, string tag, string arg)
	{
		TagType tagType = ParseEnum<TagType>(tag.CapitalizeFirst());
		string text = str.StripTags();
		switch (tagType)
		{
		case TagType.Undefined:
			return str;
		case TagType.Name:
			return text.Colorize(NameColor);
		case TagType.SectionTitle:
			return text.Colorize(TipSectionTitleColor);
		case TagType.Faction:
		{
			if (arg.NullOrEmpty())
			{
				return text;
			}
			Faction faction2 = Find.FactionManager.AllFactions.ToList().Find((Faction x) => x.GetUniqueLoadID() == arg);
			if (faction2 == null)
			{
				return text.Colorize(SubtleGrayColor);
			}
			return text.Colorize(GetFactionRelationColor(faction2));
		}
		case TagType.Ideo:
		{
			if (arg.NullOrEmpty())
			{
				return text;
			}
			Ideo ideo = Find.IdeoManager.IdeosListForReading.Find((Ideo x) => x.GetUniqueLoadID() == arg);
			if (ideo == null)
			{
				return text;
			}
			return text.Colorize(ideo.TextColor);
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
				return text.Colorize(SubtleGrayColor);
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
		case TagType.Threat:
			return text.Colorize(ThreatColor);
		case TagType.Red:
			return text.Colorize(ColorLibrary.RedReadable);
		case TagType.Reward:
			return text.Colorize(CurrencyColor);
		case TagType.Gray:
			return text.Colorize(SubtleGrayColor);
		default:
			Log.ErrorOnce("Invalid tag '" + tag + "'", tag.GetHashCode());
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
		return faction.RelationKindWith(Faction.OfPlayer) switch
		{
			FactionRelationKind.Ally => FactionColor_Ally, 
			FactionRelationKind.Hostile => FactionColor_Hostile, 
			FactionRelationKind.Neutral => FactionColor_Neutral, 
			_ => faction.Color, 
		};
	}

	private static T ParseEnum<T>(string value, bool ignoreCase = true)
	{
		if (Enum.IsDefined(typeof(T), value))
		{
			return (T)Enum.Parse(typeof(T), value, ignoreCase);
		}
		return default(T);
	}

	public static StringBuilder AppendTagged(this StringBuilder sb, TaggedString taggedString)
	{
		return sb.Append(taggedString.Resolve());
	}

	public static StringBuilder AppendLineTagged(this StringBuilder sb, TaggedString taggedString)
	{
		return sb.AppendLine(taggedString.Resolve());
	}

	public static TaggedString ToTaggedString(this StringBuilder sb)
	{
		return new TaggedString(sb.ToString());
	}

	public static string AsTipTitle(this TaggedString ts)
	{
		return ts.Colorize(TipSectionTitleColor);
	}

	public static string AsTipTitle(this string s)
	{
		return s.Colorize(TipSectionTitleColor);
	}
}
