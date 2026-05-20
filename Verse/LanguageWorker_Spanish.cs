namespace Verse;

public class LanguageWorker_Spanish : LanguageWorker
{
	public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
	{
		if (name)
		{
			return str;
		}
		if (plural)
		{
			return ((gender == Gender.Female) ? "unas " : "unos ") + str;
		}
		return ((gender == Gender.Female) ? "una " : "un ") + str;
	}

	public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
	{
		if (name)
		{
			return str;
		}
		if (plural)
		{
			return ((gender == Gender.Female) ? "las " : "los ") + str;
		}
		return ((gender == Gender.Female) ? "la " : "el ") + str;
	}

	public override string OrdinalNumber(int number, Gender gender = Gender.None)
	{
		return number + ".º";
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
		if (count != -1 && count < 2)
		{
			return str;
		}
		char c = str[str.Length - 1];
		char c2 = ((str.Length >= 2) ? str[str.Length - 2] : '\0');
		if (IsVowel(c))
		{
			if (str == "sí")
			{
				return "síes";
			}
			if (c == 'í' || c == 'ú' || c == 'Í' || c == 'Ú')
			{
				return str + "es";
			}
			return str + "s";
		}
		if ((c == 'y' || c == 'Y') && IsVowel(c2))
		{
			return str + "es";
		}
		if ("lrndjsxLRNDJSX".IndexOf(c) >= 0 || (c == 'h' && c2 == 'c'))
		{
			return str + "es";
		}
		if (c == 'z' || c == 'Z')
		{
			return str.Substring(0, str.Length - 1) + ((c == 'z') ? 'c' : 'C') + "es";
		}
		return str + "s";
	}

	public bool IsVowel(char ch)
	{
		return "aeiouáéíóúAEIOUÁÉÍÓÚ".IndexOf(ch) >= 0;
	}
}
