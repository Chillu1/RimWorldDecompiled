using System.Text.RegularExpressions;

namespace Verse;

public class LanguageWorker_French : LanguageWorker
{
	private Regex ElisionE = new Regex("\\b([cdjlmnst]|qu|quoiqu|lorsqu)e ([aàâäeéèêëiîïoôöuùüûh])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private Regex ElisionLa = new Regex("\\b(l)a ([aàâäeéèêëiîïoôöuùüûh])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private Regex ElisionSi = new Regex("\\b(s)i (ils?)\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private Regex DeLe = new Regex("\\b(d)e l(es?)\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private Regex ALe = new Regex("\\bà les?\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
		char c = str[str.Length - 1];
		if (c == 's' || c == 'x' || c == 'z')
		{
			return str;
		}
		switch (str)
		{
		case "travail":
			return str.Substring(0, str.Length - 3) + "aux";
		case "bleu":
		case "émeu":
		case "lieu":
		case "banal":
		case "fatal":
		case "final":
			return str + "s";
		case "bijou":
		case "caillou":
		case "genou":
			return str + "x";
		default:
			if (str.EndsWith("al"))
			{
				return str.Substring(0, str.Length - 2) + "aux";
			}
			if (str.EndsWith("au") | str.EndsWith("eu"))
			{
				return str + "x";
			}
			return str + "s";
		}
	}

	public override string PostProcessed(string str)
	{
		return PostProcessedInt(base.PostProcessed(str));
	}

	public bool IsVowel(char ch)
	{
		return "aàâäæeéèêëiîïoôöœuùüûhAÀÂÄÆEÉÈÊËIÎÏOÔÖŒUÙÜÛH".IndexOf(ch) >= 0;
	}

	private string PostProcessedInt(string str)
	{
		str = ElisionE.Replace(str, "$1'$2");
		str = ElisionLa.Replace(str, "$1'$2");
		str = ElisionSi.Replace(str, "$1'$2");
		str = DeLe.Replace(str, "$1$2");
		str = ALe.Replace(str, ReplaceALe);
		return str;
	}

	private string ReplaceALe(Match match)
	{
		return match.ToString() switch
		{
			"à le" => "au", 
			"à les" => "aux", 
			"À le" => "Au", 
			"À les" => "Aux", 
			_ => match.ToString(), 
		};
	}
}
