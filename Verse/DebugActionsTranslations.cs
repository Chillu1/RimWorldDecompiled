using LudeonTK;

namespace Verse;

public static class DebugActionsTranslations
{
	[DebugAction("Translation", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Entry)]
	private static void WriteBackstoryTranslationFile()
	{
		LanguageDataWriter.WriteBackstoryFile();
	}

	[DebugAction("Translation", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Entry)]
	private static void SaveTranslationReport()
	{
		LanguageReportGenerator.SaveTranslationReport();
	}
}
