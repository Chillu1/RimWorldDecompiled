using System;
using System.Collections.Generic;

namespace Verse
{
	public class LanguageInfo
	{
		public string friendlyNameNative;

		public string friendlyNameEnglish;

		public bool canBeTiny = true;

		public List<CreditsEntry> credits = new List<CreditsEntry>();

		public Type languageWorkerClass = typeof(LanguageWorker_Default);
	}
}
