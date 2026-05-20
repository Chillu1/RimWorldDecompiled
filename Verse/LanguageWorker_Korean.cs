using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Verse;

public class LanguageWorker_Korean : LanguageWorker
{
	private static StringBuilder tmpStringBuilder = new StringBuilder();

	private static readonly Regex JosaPattern = new Regex("\\(이\\)가|\\(와\\)과|\\(을\\)를|\\(은\\)는|\\(아\\)야|\\(이\\)어|\\(으\\)로|\\(이\\)", RegexOptions.Compiled);

	private static readonly Dictionary<string, (string, string)> JosaPatternPaired = new Dictionary<string, (string, string)>
	{
		{
			"(이)가",
			("이", "가")
		},
		{
			"(와)과",
			("과", "와")
		},
		{
			"(을)를",
			("을", "를")
		},
		{
			"(은)는",
			("은", "는")
		},
		{
			"(아)야",
			("아", "야")
		},
		{
			"(이)어",
			("이어", "여")
		},
		{
			"(으)로",
			("으로", "로")
		},
		{
			"(이)",
			("이", "")
		}
	};

	private static readonly Regex TagOrNodeOpeningPattern = new Regex("\\(\\*|<", RegexOptions.Compiled);

	private static readonly Regex TagOrNodeClosingPattern = new Regex("(\\(|<)\\/\\w+(\\)|>)", RegexOptions.Compiled);

	private static readonly List<char> AlphabetEndPattern = new List<char> { 'b', 'c', 'k', 'l', 'm', 'n', 'p', 'q', 't' };

	public override string PostProcessed(string str)
	{
		return ReplaceJosa(base.PostProcessed(str));
	}

	public string ReplaceJosa(string src)
	{
		tmpStringBuilder.Length = 0;
		string text = StripTags(src);
		int num = 0;
		MatchCollection matchCollection = JosaPattern.Matches(src);
		MatchCollection matchCollection2 = JosaPattern.Matches(text);
		if (matchCollection2.Count < matchCollection.Count)
		{
			return src;
		}
		for (int i = 0; i < matchCollection.Count; i++)
		{
			Match match = matchCollection[i];
			Match match2 = matchCollection2[i];
			tmpStringBuilder.Append(src, num, match.Index - num);
			char? c = FindLastChar(text, match2.Index);
			if (c.HasValue)
			{
				tmpStringBuilder.Append(ResolveJosa(match.Value, c.Value));
			}
			else
			{
				tmpStringBuilder.Append(match.Value);
			}
			num = match.Index + match.Length;
		}
		tmpStringBuilder.Append(src, num, src.Length - num);
		return tmpStringBuilder.ToString();
	}

	private string StripTags(string inString)
	{
		if (TagOrNodeOpeningPattern.Match(inString).Success)
		{
			return TagOrNodeClosingPattern.Replace(inString, "");
		}
		return inString;
	}

	private string ResolveJosa(string josaToken, char lastChar)
	{
		if (char.IsLetterOrDigit(lastChar))
		{
			if (!((josaToken == "(으)로") ? HasJongExceptRieul(lastChar) : HasJong(lastChar)))
			{
				return JosaPatternPaired[josaToken].Item2;
			}
			return JosaPatternPaired[josaToken].Item1;
		}
		return josaToken;
	}

	private char? FindLastChar(string stripped, int strippedMatchIndex)
	{
		if (strippedMatchIndex == 0)
		{
			return null;
		}
		char c = stripped[strippedMatchIndex - 1];
		switch (c)
		{
		case '"':
		case '\'':
			if (strippedMatchIndex == 1)
			{
				return null;
			}
			return stripped[strippedMatchIndex - 2];
		default:
			return c;
		case ')':
		{
			int num = stripped.LastIndexOf('(', strippedMatchIndex - 1, strippedMatchIndex - 1);
			if (num == -1)
			{
				return null;
			}
			for (int num2 = num; num2 >= 0; num2--)
			{
				if (stripped[num2] != ' ')
				{
					return stripped[num2];
				}
			}
			return null;
		}
		}
	}

	private bool HasJong(char inChar)
	{
		if (!IsKorean(inChar))
		{
			return AlphabetEndPattern.Contains(inChar);
		}
		return ExtractJongCode(inChar) > 0;
	}

	private bool HasJongExceptRieul(char inChar)
	{
		if (!IsKorean(inChar))
		{
			if (inChar != 'l')
			{
				return AlphabetEndPattern.Contains(inChar);
			}
			return false;
		}
		int num = ExtractJongCode(inChar);
		if (num != 8)
		{
			return num != 0;
		}
		return false;
	}

	private int ExtractJongCode(char inChar)
	{
		return (inChar - 44032) % 28;
	}

	private bool IsKorean(char inChar)
	{
		if (inChar >= '가')
		{
			return inChar <= '힣';
		}
		return false;
	}
}
