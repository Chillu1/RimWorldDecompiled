namespace Verse
{
	public abstract class LanguageWorker
	{
		public virtual string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (str.NullOrEmpty())
			{
				return "";
			}
			if (name)
			{
				return str;
			}
			if ("IndefiniteForm".CanTranslate())
			{
				return "IndefiniteForm".Translate(str);
			}
			return "IndefiniteArticle".Translate() + " " + str;
		}

		public string WithIndefiniteArticle(string str, bool plural = false, bool name = false)
		{
			return WithIndefiniteArticle(str, LanguageDatabase.activeLanguage.ResolveGender(str), plural, name);
		}

		public string WithIndefiniteArticlePostProcessed(string str, Gender gender, bool plural = false, bool name = false)
		{
			return PostProcessed(WithIndefiniteArticle(str, gender, plural, name));
		}

		public string WithIndefiniteArticlePostProcessed(string str, bool plural = false, bool name = false)
		{
			return PostProcessed(WithIndefiniteArticle(str, plural, name));
		}

		public virtual string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (str.NullOrEmpty())
			{
				return "";
			}
			if (name)
			{
				return str;
			}
			if ("DefiniteForm".CanTranslate())
			{
				return "DefiniteForm".Translate(str);
			}
			return "DefiniteArticle".Translate() + " " + str;
		}

		public string WithDefiniteArticle(string str, bool plural = false, bool name = false)
		{
			return WithDefiniteArticle(str, LanguageDatabase.activeLanguage.ResolveGender(str), plural, name);
		}

		public string WithDefiniteArticlePostProcessed(string str, Gender gender, bool plural = false, bool name = false)
		{
			return PostProcessed(WithDefiniteArticle(str, gender, plural, name));
		}

		public string WithDefiniteArticlePostProcessed(string str, bool plural = false, bool name = false)
		{
			return PostProcessed(WithDefiniteArticle(str, plural, name));
		}

		public virtual string OrdinalNumber(int number, Gender gender = Gender.None)
		{
			return number.ToString();
		}

		public virtual string PostProcessed(string str)
		{
			str = str.MergeMultipleSpaces();
			return str;
		}

		public virtual string ToTitleCase(string str)
		{
			return str.CapitalizeFirst();
		}

		public virtual string Pluralize(string str, Gender gender, int count = -1)
		{
			return str;
		}

		public string Pluralize(string str, int count = -1)
		{
			return Pluralize(str, LanguageDatabase.activeLanguage.ResolveGender(str), count);
		}

		public virtual string PostProcessedKeyedTranslation(string translation)
		{
			return translation;
		}
	}
}
