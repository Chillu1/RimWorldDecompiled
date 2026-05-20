using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RimWorld.IO;

namespace Verse;

public class LanguageWordInfo
{
	private Dictionary<string, Gender> genders = new Dictionary<string, Gender>();

	private Dictionary<string, Dictionary<string, string[]>> lookupTables = new Dictionary<string, Dictionary<string, string[]>>();

	private const string FolderName = "WordInfo";

	private const string GendersFolderName = "Gender";

	private const string MaleFileName = "Male.txt";

	private const string FemaleFileName = "Female.txt";

	private const string NeuterFileName = "Neuter.txt";

	private static StringBuilder tmpLowercase = new StringBuilder();

	public void LoadFrom(Tuple<VirtualDirectory, ModContentPack, string> dir, LoadedLanguage lang)
	{
		VirtualDirectory directory = dir.Item1.GetDirectory("WordInfo").GetDirectory("Gender");
		TryLoadFromFile(directory.GetFile("Male.txt"), Gender.Male, dir, lang);
		TryLoadFromFile(directory.GetFile("Female.txt"), Gender.Female, dir, lang);
		TryLoadFromFile(directory.GetFile("Neuter.txt"), Gender.None, dir, lang);
	}

	public Gender ResolveGender(string str, string fallback = null, Gender defaultGender = Gender.Male)
	{
		if (str == null)
		{
			return defaultGender;
		}
		if (TryResolveGender(str, out var gender))
		{
			return gender;
		}
		if (fallback != null && TryResolveGender(str, out gender))
		{
			return gender;
		}
		return defaultGender;
	}

	private bool TryResolveGender(string str, out Gender gender)
	{
		tmpLowercase.Length = 0;
		for (int i = 0; i < str.Length; i++)
		{
			tmpLowercase.Append(char.ToLower(str[i]));
		}
		string key = tmpLowercase.ToString();
		if (genders.TryGetValue(key, out gender))
		{
			return true;
		}
		gender = Gender.Male;
		return false;
	}

	private void TryLoadFromFile(VirtualFile file, Gender gender, Tuple<VirtualDirectory, ModContentPack, string> dir, LoadedLanguage lang)
	{
		string[] array;
		try
		{
			array = file.ReadAllLines();
		}
		catch (DirectoryNotFoundException)
		{
			return;
		}
		catch (FileNotFoundException)
		{
			return;
		}
		if (!lang.TryRegisterFileIfNew(dir, file.FullPath))
		{
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].NullOrEmpty() && !genders.ContainsKey(array[i]))
			{
				genders.Add(array[i], gender);
			}
		}
	}

	public void RegisterLut(string name)
	{
		if (lookupTables.ContainsKey(name.ToLower()))
		{
			Log.Error("Tried registering language look up table named " + name + " twice.");
			return;
		}
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
		LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
		try
		{
			foreach (Tuple<VirtualDirectory, ModContentPack, string> allDirectory in activeLanguage.AllDirectories)
			{
				VirtualFile file = allDirectory.Item1.GetDirectory("WordInfo").GetFile(name + ".txt");
				if (!file.Exists)
				{
					continue;
				}
				foreach (string item in GenText.LinesFromString(file.ReadAllText()))
				{
					if (GenText.TryGetSeparatedValues(item, ';', out var output))
					{
						string key = output[0].ToLower();
						if (!dictionary.ContainsKey(key))
						{
							dictionary.Add(key, output);
						}
					}
					else
					{
						Log.ErrorOnce("Failed parsing lookup items from line " + item + " in " + file.FullPath + ". Line: " + item, name.GetHashCode() ^ 0x6EB2F393);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Exception parsing a language lookup table: " + ex);
		}
		lookupTables.Add(name.ToLower(), dictionary);
	}

	public Dictionary<string, string[]> GetLookupTable(string name)
	{
		string text = name.ToLower();
		if (lookupTables.ContainsKey(text))
		{
			return lookupTables[text];
		}
		RegisterLut(text);
		if (lookupTables.ContainsKey(text))
		{
			return lookupTables[text];
		}
		return null;
	}
}
