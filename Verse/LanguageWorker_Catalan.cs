namespace Verse
{
	public class LanguageWorker_Catalan : LanguageWorker
	{
		public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return WithElLaArticle(str, gender, name: true);
			}
			if (plural)
			{
				return ((gender == Gender.Female) ? "unes " : "uns ") + str;
			}
			return ((gender == Gender.Female) ? "una " : "un ") + str;
		}

		public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return WithElLaArticle(str, gender, name: true);
			}
			if (plural)
			{
				return ((gender == Gender.Female) ? "les " : "els ") + str;
			}
			return WithElLaArticle(str, gender, name: false);
		}

		private string WithElLaArticle(string str, Gender gender, bool name)
		{
			if (str.Length != 0 && (IsVowel(str[0]) || str[0] == 'h' || str[0] == 'H'))
			{
				if (name)
				{
					return ((gender == Gender.Female) ? "l'" : "n'") + str;
				}
				return "l'" + str;
			}
			return ((gender == Gender.Female) ? "la " : "el ") + str;
		}

		public override string OrdinalNumber(int number, Gender gender = Gender.None)
		{
			if (gender == Gender.Female)
			{
				return number + "a";
			}
			switch (number)
			{
			case 1:
			case 3:
				return number + "r";
			case 2:
				return number + "n";
			case 4:
				return number + "t";
			default:
				return number + "è";
			}
		}

		public bool IsVowel(char ch)
		{
			return "ieɛaoɔuəuàêèéòóüúIEƐAOƆUƏUÀÊÈÉÒÓÜÚ".IndexOf(ch) >= 0;
		}
	}
}
