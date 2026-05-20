using System;

namespace Verse;

public class LanguageWorker_English : LanguageWorker
{
	public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
	{
		if (str.NullOrEmpty())
		{
			return "";
		}
		if (name)
		{
			return str;
		}
		if (plural)
		{
			return str;
		}
		return "a " + str;
	}

	public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
	{
		if (str.NullOrEmpty())
		{
			return "";
		}
		if (name)
		{
			return str;
		}
		return "the " + str;
	}

	public override string PostProcessed(string str)
	{
		str = base.PostProcessed(str);
		if (str.StartsWith("a ", StringComparison.OrdinalIgnoreCase) && str.Length >= 3)
		{
			bool num = str.Substring(2) == "hour";
			if (num || str[2] == 'a' || str[2] == 'e' || str[2] == 'i' || str[2] == 'o' || str[2] == 'u')
			{
				str = str.Insert(1, "n");
			}
			if (num || str[2] == 'A' || str[2] == 'E' || str[2] == 'I' || str[2] == 'O' || str[2] == 'U')
			{
				str = str.Insert(1, "n");
			}
		}
		if (str.StartsWith("an unique ", StringComparison.OrdinalIgnoreCase))
		{
			str = str.Remove(1, 1);
		}
		str = str.Replace(" a a", " an a");
		str = str.Replace(" a e", " an e");
		str = str.Replace(" a i", " an i");
		str = str.Replace(" a o", " an o");
		str = str.Replace(" a u", " an u");
		str = str.Replace(" a hour", " an hour");
		str = str.Replace(" an unique", " a unique");
		str = str.Replace(" A a", " An a");
		str = str.Replace(" A e", " An e");
		str = str.Replace(" A i", " An i");
		str = str.Replace(" A o", " An o");
		str = str.Replace(" A u", " An u");
		str = str.Replace(" A hour", " An hour");
		str = str.Replace(" An unique", " A unique");
		str = str.Replace("\na a", "\nan a");
		str = str.Replace("\na e", "\nan e");
		str = str.Replace("\na i", "\nan i");
		str = str.Replace("\na o", "\nan o");
		str = str.Replace("\na u", "\nan u");
		str = str.Replace("\na hour", "\nan hour");
		str = str.Replace("\nan unique", "\na unique");
		str = str.Replace("\nA a", "\nAn a");
		str = str.Replace("\nA e", "\nAn e");
		str = str.Replace("\nA i", "\nAn i");
		str = str.Replace("\nA o", "\nAn o");
		str = str.Replace("\nA u", "\nAn u");
		str = str.Replace("\nA hour", "\nAn hour");
		str = str.Replace("\nAn unique", "\nA unique");
		str = str.Replace(" a A", " an A");
		str = str.Replace(" a E", " an E");
		str = str.Replace(" a I", " an I");
		str = str.Replace(" a O", " an O");
		str = str.Replace(" a U", " an U");
		str = str.Replace(" A A", " An A");
		str = str.Replace(" A E", " An E");
		str = str.Replace(" A I", " An I");
		str = str.Replace(" A O", " An O");
		str = str.Replace(" A U", " An U");
		str = str.Replace("\na A", "\nan A");
		str = str.Replace("\na E", "\nan E");
		str = str.Replace("\na I", "\nan I");
		str = str.Replace("\na O", "\nan O");
		str = str.Replace("\na U", "\nan U");
		str = str.Replace("\nA A", "\nAn A");
		str = str.Replace("\nA E", "\nAn E");
		str = str.Replace("\nA I", "\nAn I");
		str = str.Replace("\nA O", "\nAn O");
		str = str.Replace("\nA U", "\nAn U");
		return str;
	}

	public override string ToTitleCase(string str)
	{
		return GenText.ToTitleCaseSmart(str);
	}

	public override string OrdinalNumber(int number, Gender gender = Gender.None)
	{
		int num = number % 10;
		if (number / 10 % 10 != 1)
		{
			switch (num)
			{
			case 1:
				return number + "st";
			case 2:
				return number + "nd";
			case 3:
				return number + "rd";
			}
		}
		return number + "th";
	}

	public override string Pluralize(string str, Gender gender, int count = -1)
	{
		if (str.NullOrEmpty())
		{
			return str;
		}
		if (TryLookupPluralForm(str, gender, out var plural, count))
		{
			return plural;
		}
		if (str[str.Length - 1] == 's' || (count != -1 && count < 2))
		{
			return str;
		}
		char num = str[str.Length - 1];
		char c = ((str.Length != 1) ? str[str.Length - 2] : '\0');
		bool flag = char.IsLetter(c) && "oaieuyOAIEUY".IndexOf(c) >= 0;
		bool flag2 = char.IsLetter(c) && !flag;
		if (num == 'y' && flag2)
		{
			return str.Substring(0, str.Length - 1) + "ies";
		}
		return str + "s";
	}

	public override string PostProcessThingLabelForRelic(string thingLabel)
	{
		int num = thingLabel.LastIndexOf(' ');
		if (num != -1)
		{
			return thingLabel.Substring(num + 1);
		}
		return thingLabel;
	}
}
