namespace Verse
{
	public class LanguageWorker_Turkish : LanguageWorker
	{
		public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return str;
			}
			return str + " bir";
		}

		public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			return str;
		}
	}
}
