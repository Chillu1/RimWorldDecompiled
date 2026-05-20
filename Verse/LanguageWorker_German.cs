using System;
using System.Text.RegularExpressions;

namespace Verse;

public class LanguageWorker_German : LanguageWorker
{
	private static readonly LanguageWorker_English englishWorker = new LanguageWorker_English();

	public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
	{
		if (name)
		{
			return str;
		}
		return gender switch
		{
			Gender.None => "ein " + str, 
			Gender.Male => "ein " + str, 
			Gender.Female => "eine " + str, 
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
			Gender.None => "das " + str, 
			Gender.Male => "der " + str, 
			Gender.Female => "die " + str, 
			_ => str, 
		};
	}

	public override string PostProcessed(string str)
	{
		str = base.PostProcessed(str);
		return Regex.Replace(str, "(.)(.)(\\(/Name\\)|</color>)?'s(?=\\s|$)", delegate(Match m)
		{
			string value = m.Groups[1].Value;
			string value2 = m.Groups[2].Value;
			string value3 = m.Groups[3].Value;
			switch (value2)
			{
			default:
				if (!(value == "c") || !(value2 == "e"))
				{
					break;
				}
				goto case "s";
			case "s":
			case "ß":
			case "z":
			case "x":
				return value + value2 + value3 + "'";
			}
			return value + value2 + value3 + "s";
		}, RegexOptions.Compiled);
	}

	public override string OrdinalNumber(int number, Gender gender = Gender.None)
	{
		return number + ".";
	}

	public override string Pluralize(string str, Gender gender, int count = -1)
	{
		return englishWorker.Pluralize(str, gender, count);
	}

	public override string PostProcessThingLabelForRelic(string thingLabel)
	{
		char[] anyOf = new char[2] { ' ', '-' };
		int num = thingLabel.LastIndexOfAny(anyOf);
		if (num != -1)
		{
			thingLabel = thingLabel.Substring(num + 1);
		}
		string[] array = new string[26]
		{
			"Horn", "Lanze", "Pulser", "Werfer", "Axt", "Flinte", "Bogen", "Revolver", "Gewehr", "Stoßzahn",
			"Stab", "Hammer", "Schwert", "Pistole", "Dolch", "Büchse", "Kanone", "Granaten", "Granate", "Keule",
			"Säbel", "Messer", "Rapier", "Klinge", "Sense", "Speer"
		};
		foreach (string text in array)
		{
			if (thingLabel.EndsWith(text, StringComparison.OrdinalIgnoreCase))
			{
				thingLabel = text;
				break;
			}
		}
		return thingLabel;
	}
}
