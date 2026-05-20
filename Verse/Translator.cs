using System;
using System.Collections.Generic;
using System.Text;

namespace Verse;

public static class Translator
{
	public static bool CanTranslate(this string key)
	{
		return LanguageDatabase.activeLanguage.HaveTextForKey(key);
	}

	public static TaggedString TranslateWithBackup(this string key, TaggedString backupKey)
	{
		if (key.TryTranslate(out var result))
		{
			return result;
		}
		if (TryTranslate(backupKey, out result))
		{
			return result;
		}
		return key.Translate();
	}

	public static bool TryTranslate(this string key, out TaggedString result)
	{
		if (key.NullOrEmpty())
		{
			result = key;
			return false;
		}
		if (LanguageDatabase.activeLanguage == null)
		{
			Log.Error("No active language! Cannot translate from key " + key + ".");
			result = key;
			return true;
		}
		if (LanguageDatabase.activeLanguage.TryGetTextFromKey(key, out result))
		{
			return true;
		}
		result = key;
		return false;
	}

	public static string TranslateSimple(this string key)
	{
		return key.Translate();
	}

	public static TaggedString Translate(this string key)
	{
		if (key.TryTranslate(out var result))
		{
			return result;
		}
		LanguageDatabase.defaultLanguage.TryGetTextFromKey(key, out result);
		if (Prefs.DevMode)
		{
			return PseudoTranslated(result);
		}
		return result;
	}

	[Obsolete("Use TranslatorFormattedStringExtensions")]
	public static string Translate(this string key, params object[] args)
	{
		if (key.NullOrEmpty())
		{
			return key;
		}
		if (LanguageDatabase.activeLanguage == null)
		{
			Log.Error("No active language! Cannot translate from key " + key + ".");
			return key;
		}
		if (!LanguageDatabase.activeLanguage.TryGetTextFromKey(key, out var translated))
		{
			LanguageDatabase.defaultLanguage.TryGetTextFromKey(key, out translated);
			if (Prefs.DevMode)
			{
				translated = PseudoTranslated(translated);
			}
		}
		string result = translated;
		try
		{
			result = string.Format(translated, args);
		}
		catch (Exception ex)
		{
			Log.ErrorOnce(string.Concat("Exception translating '" + translated + "': ", ex?.ToString()), Gen.HashCombineInt(key.GetHashCode(), 394878901));
		}
		return result;
	}

	public static bool TryGetTranslatedStringsForFile(string fileName, out List<string> stringList)
	{
		if (!LanguageDatabase.activeLanguage.TryGetStringsFromFile(fileName, out stringList) && !LanguageDatabase.defaultLanguage.TryGetStringsFromFile(fileName, out stringList))
		{
			Log.Error("No string files for " + fileName + ".");
			return false;
		}
		return true;
	}

	private static string PseudoTranslated(string original)
	{
		if (original == null)
		{
			return null;
		}
		if (!Prefs.DevMode)
		{
			return original;
		}
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < original.Length; i++)
		{
			char c = original[i];
			switch (c)
			{
			case '{':
				flag = true;
				stringBuilder.Append(c);
				continue;
			case '}':
				flag = false;
				stringBuilder.Append(c);
				continue;
			}
			if (!flag)
			{
				string text = null;
				stringBuilder.Append(c switch
				{
					'a' => "à", 
					'b' => "þ", 
					'c' => "ç", 
					'd' => "ð", 
					'e' => "è", 
					'f' => "Ƒ", 
					'g' => "ğ", 
					'h' => "ĥ", 
					'i' => "ì", 
					'j' => "ĵ", 
					'k' => "к", 
					'l' => "ſ", 
					'm' => "ṁ", 
					'n' => "ƞ", 
					'o' => "ò", 
					'p' => "ṗ", 
					'q' => "q", 
					'r' => "ṟ", 
					's' => "ș", 
					't' => "ṭ", 
					'u' => "ù", 
					'v' => "ṽ", 
					'w' => "ẅ", 
					'x' => "ẋ", 
					'y' => "ý", 
					'z' => "ž", 
					_ => c.ToString() ?? "", 
				});
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}
}
