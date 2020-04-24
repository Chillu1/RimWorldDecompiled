using System.Globalization;
using System.Threading;

namespace Verse
{
	public static class CultureInfoUtility
	{
		private const string EnglishCulture = "en-US";

		public static void EnsureEnglish()
		{
			if (Thread.CurrentThread.CurrentCulture.Name != "en-US")
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
			}
		}
	}
}
