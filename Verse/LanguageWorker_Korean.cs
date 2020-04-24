using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Verse
{
	public class LanguageWorker_Korean : LanguageWorker
	{
		private struct JosaPair
		{
			public readonly string josa1;

			public readonly string josa2;

			public JosaPair(string josa1, string josa2)
			{
				this.josa1 = josa1;
				this.josa2 = josa2;
			}
		}

		private static StringBuilder tmpStringBuilder = new StringBuilder();

		private static readonly Regex JosaPattern = new Regex("\\(이\\)가|\\(와\\)과|\\(을\\)를|\\(은\\)는|\\(아\\)야|\\(이\\)여|\\(으\\)로|\\(이\\)라");

		private static readonly Dictionary<string, JosaPair> JosaPatternPaired = new Dictionary<string, JosaPair>
		{
			{
				"(이)가",
				new JosaPair("이", "가")
			},
			{
				"(와)과",
				new JosaPair("과", "와")
			},
			{
				"(을)를",
				new JosaPair("을", "를")
			},
			{
				"(은)는",
				new JosaPair("은", "는")
			},
			{
				"(아)야",
				new JosaPair("아", "야")
			},
			{
				"(이)여",
				new JosaPair("이여", "여")
			},
			{
				"(으)로",
				new JosaPair("으로", "로")
			},
			{
				"(이)라",
				new JosaPair("이라", "라")
			}
		};

		private static readonly List<char> AlphabetEndPattern = new List<char>
		{
			'b',
			'c',
			'k',
			'l',
			'm',
			'n',
			'p',
			'q',
			't'
		};

		public override string PostProcessed(string str)
		{
			str = base.PostProcessed(str);
			str = ReplaceJosa(str);
			return str;
		}

		public override string PostProcessedKeyedTranslation(string translation)
		{
			translation = base.PostProcessedKeyedTranslation(translation);
			translation = ReplaceJosa(translation);
			return translation;
		}

		public string ReplaceJosa(string src)
		{
			tmpStringBuilder.Length = 0;
			MatchCollection matchCollection = JosaPattern.Matches(src);
			int num = 0;
			for (int i = 0; i < matchCollection.Count; i++)
			{
				Match match = matchCollection[i];
				JosaPair josaPair = JosaPatternPaired[match.Value];
				tmpStringBuilder.Append(src, num, match.Index - num);
				if (match.Index > 0)
				{
					char inChar = src[match.Index - 1];
					if ((match.Value != "(으)로" && HasJong(inChar)) || (match.Value == "(으)로" && HasJongExceptRieul(inChar)))
					{
						tmpStringBuilder.Append(josaPair.josa1);
					}
					else
					{
						tmpStringBuilder.Append(josaPair.josa2);
					}
				}
				else
				{
					tmpStringBuilder.Append(josaPair.josa1);
				}
				num = match.Index + match.Length;
			}
			tmpStringBuilder.Append(src, num, src.Length - num);
			return tmpStringBuilder.ToString();
		}

		private bool HasJong(char inChar)
		{
			if (!IsKorean(inChar))
			{
				return AlphabetEndPattern.Contains(inChar);
			}
			return (inChar - 44032) % 28 > 0;
		}

		private bool HasJongExceptRieul(char inChar)
		{
			if (!IsKorean(inChar))
			{
				return false;
			}
			int num = (inChar - 44032) % 28;
			if (num != 8)
			{
				return num != 0;
			}
			return false;
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
}
