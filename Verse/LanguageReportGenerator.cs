using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RimWorld;

namespace Verse
{
	public static class LanguageReportGenerator
	{
		private const string FileName = "TranslationReport.txt";

		private static List<string> tmpStr1Symbols = new List<string>();

		private static List<string> tmpStr2Symbols = new List<string>();

		private static StringBuilder tmpSymbol = new StringBuilder();

		public static void SaveTranslationReport()
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage && !defaultLanguage.anyError)
			{
				Messages.Message("Please activate a non-English language to scan.", MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			activeLanguage.LoadData();
			defaultLanguage.LoadData();
			LongEventHandler.QueueLongEvent(DoSaveTranslationReport, "GeneratingTranslationReport", doAsynchronously: true, null);
		}

		private static void DoSaveTranslationReport()
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Translation report for " + activeLanguage);
			if (activeLanguage.defInjections.Any((DefInjectionPackage x) => x.usedOldRepSyntax))
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Consider using <Something.Field.Example.Etc>translation</Something.Field.Example.Etc> def-injection syntax instead of <rep>.");
			}
			try
			{
				AppendGeneralLoadErrors(stringBuilder);
			}
			catch (Exception arg)
			{
				Log.Error("Error while generating translation report (general load errors): " + arg);
			}
			try
			{
				AppendDefInjectionsLoadErros(stringBuilder);
			}
			catch (Exception arg2)
			{
				Log.Error("Error while generating translation report (def-injections load errors): " + arg2);
			}
			try
			{
				AppendBackstoriesLoadErrors(stringBuilder);
			}
			catch (Exception arg3)
			{
				Log.Error("Error while generating translation report (backstories load errors): " + arg3);
			}
			try
			{
				AppendMissingKeyedTranslations(stringBuilder);
			}
			catch (Exception arg4)
			{
				Log.Error("Error while generating translation report (missing keyed translations): " + arg4);
			}
			List<string> list = new List<string>();
			try
			{
				AppendMissingDefInjections(stringBuilder, list);
			}
			catch (Exception arg5)
			{
				Log.Error("Error while generating translation report (missing def-injections): " + arg5);
			}
			try
			{
				AppendMissingBackstories(stringBuilder);
			}
			catch (Exception arg6)
			{
				Log.Error("Error while generating translation report (missing backstories): " + arg6);
			}
			try
			{
				AppendUnnecessaryDefInjections(stringBuilder, list);
			}
			catch (Exception arg7)
			{
				Log.Error("Error while generating translation report (unnecessary def-injections): " + arg7);
			}
			try
			{
				AppendRenamedDefInjections(stringBuilder);
			}
			catch (Exception arg8)
			{
				Log.Error("Error while generating translation report (renamed def-injections): " + arg8);
			}
			try
			{
				AppendArgumentCountMismatches(stringBuilder);
			}
			catch (Exception arg9)
			{
				Log.Error("Error while generating translation report (argument count mismatches): " + arg9);
			}
			try
			{
				AppendUnnecessaryKeyedTranslations(stringBuilder);
			}
			catch (Exception arg10)
			{
				Log.Error("Error while generating translation report (unnecessary keyed translations): " + arg10);
			}
			try
			{
				AppendKeyedTranslationsMatchingEnglish(stringBuilder);
			}
			catch (Exception arg11)
			{
				Log.Error("Error while generating translation report (keyed translations matching English): " + arg11);
			}
			try
			{
				AppendBackstoriesMatchingEnglish(stringBuilder);
			}
			catch (Exception arg12)
			{
				Log.Error("Error while generating translation report (backstories matching English): " + arg12);
			}
			try
			{
				AppendDefInjectionsSyntaxSuggestions(stringBuilder);
			}
			catch (Exception arg13)
			{
				Log.Error("Error while generating translation report (def-injections syntax suggestions): " + arg13);
			}
			try
			{
				AppendTKeySystemErrors(stringBuilder);
			}
			catch (Exception arg14)
			{
				Log.Error("Error while generating translation report (TKeySystem errors): " + arg14);
			}
			string text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			if (text.NullOrEmpty())
			{
				text = GenFilePaths.SaveDataFolderPath;
			}
			text = Path.Combine(text, "TranslationReport.txt");
			File.WriteAllText(text, stringBuilder.ToString());
			Messages.Message("MessageTranslationReportSaved".Translate(Path.GetFullPath(text)), MessageTypeDefOf.TaskCompletion, historical: false);
		}

		private static void AppendGeneralLoadErrors(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string loadError in activeLanguage.loadErrors)
			{
				num++;
				stringBuilder.AppendLine(loadError);
			}
			sb.AppendLine();
			sb.AppendLine("========== General load errors (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendDefInjectionsLoadErros(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (DefInjectionPackage defInjection in activeLanguage.defInjections)
			{
				foreach (string loadError in defInjection.loadErrors)
				{
					num++;
					stringBuilder.AppendLine(loadError);
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Def-injected translations load errors (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendBackstoriesLoadErrors(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string backstoriesLoadError in activeLanguage.backstoriesLoadErrors)
			{
				num++;
				stringBuilder.AppendLine(backstoriesLoadError);
			}
			sb.AppendLine();
			sb.AppendLine("========== Backstories load errors (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendMissingKeyedTranslations(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (KeyValuePair<string, LoadedLanguage.KeyedReplacement> keyedReplacement in defaultLanguage.keyedReplacements)
			{
				if (!activeLanguage.HaveTextForKey(keyedReplacement.Key))
				{
					string text = keyedReplacement.Key + " '" + keyedReplacement.Value.value.Replace("\n", "\\n") + "' (English file: " + defaultLanguage.GetKeySourceFileAndLine(keyedReplacement.Key) + ")";
					if (activeLanguage.HaveTextForKey(keyedReplacement.Key, allowPlaceholders: true))
					{
						text = text + " (placeholder exists in " + activeLanguage.GetKeySourceFileAndLine(keyedReplacement.Key) + ")";
					}
					num++;
					stringBuilder.AppendLine(text);
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Missing keyed translations (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendMissingDefInjections(StringBuilder sb, List<string> outUnnecessaryDefInjections)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (DefInjectionPackage defInjection in activeLanguage.defInjections)
			{
				foreach (string item in defInjection.MissingInjections(outUnnecessaryDefInjections))
				{
					num++;
					stringBuilder.AppendLine(defInjection.defType.Name + ": " + item);
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Def-injected translations missing (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendMissingBackstories(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string item in BackstoryTranslationUtility.MissingBackstoryTranslations(activeLanguage))
			{
				num++;
				stringBuilder.AppendLine(item);
			}
			sb.AppendLine();
			sb.AppendLine("========== Backstory translations missing (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendUnnecessaryDefInjections(StringBuilder sb, List<string> unnecessaryDefInjections)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string unnecessaryDefInjection in unnecessaryDefInjections)
			{
				num++;
				stringBuilder.AppendLine(unnecessaryDefInjection);
			}
			sb.AppendLine();
			sb.AppendLine("========== Unnecessary def-injected translations (marked as NoTranslate) (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendRenamedDefInjections(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (DefInjectionPackage defInjection in activeLanguage.defInjections)
			{
				foreach (KeyValuePair<string, DefInjectionPackage.DefInjection> injection in defInjection.injections)
				{
					if (!(injection.Value.path == injection.Value.nonBackCompatiblePath))
					{
						string text = injection.Value.nonBackCompatiblePath.Split('.')[0];
						string text2 = injection.Value.path.Split('.')[0];
						if (text != text2)
						{
							stringBuilder.AppendLine("Def has been renamed: " + text + " -> " + text2 + ", translation " + injection.Value.nonBackCompatiblePath + " should be renamed as well.");
						}
						else
						{
							stringBuilder.AppendLine("Translation " + injection.Value.nonBackCompatiblePath + " should be renamed to " + injection.Value.path);
						}
						num++;
					}
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Def-injected translations using old, renamed defs (fixed automatically but can break in the next RimWorld version) (" + num + ") =========");
			sb.Append(stringBuilder);
		}

		private static void AppendArgumentCountMismatches(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string item in defaultLanguage.keyedReplacements.Keys.Intersect(activeLanguage.keyedReplacements.Keys))
			{
				if (!activeLanguage.keyedReplacements[item].isPlaceholder && !SameSimpleGrammarResolverSymbols(defaultLanguage.keyedReplacements[item].value, activeLanguage.keyedReplacements[item].value))
				{
					num++;
					stringBuilder.AppendLine(string.Format("{0} ({1})\n  - '{2}'\n  - '{3}'", item, activeLanguage.GetKeySourceFileAndLine(item), defaultLanguage.keyedReplacements[item].value.Replace("\n", "\\n"), activeLanguage.keyedReplacements[item].value.Replace("\n", "\\n")));
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Argument count mismatches (may or may not be incorrect) (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendUnnecessaryKeyedTranslations(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (KeyValuePair<string, LoadedLanguage.KeyedReplacement> keyedReplacement in activeLanguage.keyedReplacements)
			{
				if (!defaultLanguage.HaveTextForKey(keyedReplacement.Key))
				{
					num++;
					stringBuilder.AppendLine(keyedReplacement.Key + " '" + keyedReplacement.Value.value.Replace("\n", "\\n") + "' (" + activeLanguage.GetKeySourceFileAndLine(keyedReplacement.Key) + ")");
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Unnecessary keyed translations (will never be used) (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendKeyedTranslationsMatchingEnglish(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (KeyValuePair<string, LoadedLanguage.KeyedReplacement> keyedReplacement in activeLanguage.keyedReplacements)
			{
				if (!keyedReplacement.Value.isPlaceholder && defaultLanguage.TryGetTextFromKey(keyedReplacement.Key, out var translated) && keyedReplacement.Value.value == (string)translated)
				{
					num++;
					stringBuilder.AppendLine(keyedReplacement.Key + " '" + keyedReplacement.Value.value.Replace("\n", "\\n") + "' (" + activeLanguage.GetKeySourceFileAndLine(keyedReplacement.Key) + ")");
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Keyed translations matching English (maybe ok) (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendBackstoriesMatchingEnglish(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string item in BackstoryTranslationUtility.BackstoryTranslationsMatchingEnglish(activeLanguage))
			{
				num++;
				stringBuilder.AppendLine(item);
			}
			sb.AppendLine();
			sb.AppendLine("========== Backstory translations matching English (maybe ok) (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendDefInjectionsSyntaxSuggestions(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (DefInjectionPackage defInjection in activeLanguage.defInjections)
			{
				foreach (string loadSyntaxSuggestion in defInjection.loadSyntaxSuggestions)
				{
					num++;
					stringBuilder.AppendLine(loadSyntaxSuggestion);
				}
			}
			if (num != 0)
			{
				sb.AppendLine();
				sb.AppendLine("========== Def-injected translations syntax suggestions (" + num + ") ==========");
				sb.Append(stringBuilder);
			}
		}

		private static void AppendTKeySystemErrors(StringBuilder sb)
		{
			if (TKeySystem.loadErrors.Count != 0)
			{
				sb.AppendLine();
				sb.AppendLine("========== TKey system errors (" + TKeySystem.loadErrors.Count + ") ==========");
				sb.Append(string.Join("\r\n", TKeySystem.loadErrors));
			}
		}

		public static bool SameSimpleGrammarResolverSymbols(string str1, string str2)
		{
			tmpStr1Symbols.Clear();
			tmpStr2Symbols.Clear();
			CalculateSimpleGrammarResolverSymbols(str1, tmpStr1Symbols);
			CalculateSimpleGrammarResolverSymbols(str2, tmpStr2Symbols);
			for (int i = 0; i < tmpStr1Symbols.Count; i++)
			{
				if (!tmpStr2Symbols.Contains(tmpStr1Symbols[i]))
				{
					return false;
				}
			}
			return true;
		}

		private static void CalculateSimpleGrammarResolverSymbols(string str, List<string> outSymbols)
		{
			outSymbols.Clear();
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] != '{')
				{
					continue;
				}
				tmpSymbol.Length = 0;
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				for (i++; i < str.Length; i++)
				{
					char c = str[i];
					switch (c)
					{
					case '}':
						break;
					case '_':
						flag2 = true;
						continue;
					case '?':
						flag3 = true;
						continue;
					default:
						if (!flag2 && !flag3)
						{
							tmpSymbol.Append(c);
						}
						continue;
					}
					flag = true;
					break;
				}
				if (flag)
				{
					outSymbols.Add(tmpSymbol.ToString().Trim());
				}
			}
		}
	}
}
