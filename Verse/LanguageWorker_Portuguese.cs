namespace Verse
{
	public class LanguageWorker_Portuguese : LanguageWorker
	{
		public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return str;
			}
			if (plural)
			{
				return ((gender == Gender.Female) ? "umas " : "uns ") + str;
			}
			return ((gender == Gender.Female) ? "uma " : "um ") + str;
		}

		public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return str;
			}
			if (plural)
			{
				return ((gender == Gender.Female) ? "as " : "os ") + str;
			}
			return ((gender == Gender.Female) ? "a " : "o ") + str;
		}
	}
}
