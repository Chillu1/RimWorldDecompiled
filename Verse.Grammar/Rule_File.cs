using System.Collections.Generic;
using System.Linq;

namespace Verse.Grammar;

public class Rule_File : Rule
{
	[MayTranslate]
	public string path;

	[MayTranslate]
	[TranslationCanChangeCount]
	public List<string> pathList = new List<string>();

	[Unsaved(false)]
	private List<string> cachedStrings = new List<string>();

	public override float BaseSelectionWeight => cachedStrings.Count;

	public override Rule DeepCopy()
	{
		Rule_File rule_File = (Rule_File)base.DeepCopy();
		rule_File.path = path;
		if (pathList != null)
		{
			rule_File.pathList = pathList.ToList();
		}
		if (cachedStrings != null)
		{
			rule_File.cachedStrings = cachedStrings.ToList();
		}
		return rule_File;
	}

	public override string Generate()
	{
		if (cachedStrings.NullOrEmpty())
		{
			return "Filestring";
		}
		return cachedStrings.RandomElement();
	}

	public override void Init()
	{
		if (!path.NullOrEmpty())
		{
			LoadStringsFromFile(path);
		}
		foreach (string path in pathList)
		{
			LoadStringsFromFile(path);
		}
	}

	private void LoadStringsFromFile(string filePath)
	{
		if (!Translator.TryGetTranslatedStringsForFile(filePath, out var stringList))
		{
			return;
		}
		foreach (string item in stringList)
		{
			cachedStrings.Add(item);
		}
	}

	public override string ToString()
	{
		if (!path.NullOrEmpty())
		{
			return keyword + "->(" + cachedStrings.Count + " strings from file: " + path + ")";
		}
		if (pathList.Count > 0)
		{
			return keyword + "->(" + cachedStrings.Count + " strings from " + pathList.Count + " files)";
		}
		return keyword + "->(Rule_File with no configuration)";
	}
}
