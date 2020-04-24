namespace Verse
{
	public class LanguageWorker_Hungarian : LanguageWorker
	{
		public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return str;
			}
			return "egy " + str;
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
			char ch = str[0];
			if (IsVowel(ch))
			{
				return "az " + str;
			}
			return "a " + str;
		}

		public bool IsVowel(char ch)
		{
			return "eéöőüűiíaáoóuúEÉÖŐÜŰIÍAÁOÓUÚ".IndexOf(ch) >= 0;
		}
	}
}
