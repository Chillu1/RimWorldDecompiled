using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Verse;

public static class GenFile
{
	public static string TextFromRawFile(string filePath)
	{
		return File.ReadAllText(filePath);
	}

	public static string TextFromResourceFile(string filePath)
	{
		TextAsset textAsset = Resources.Load("Text/" + filePath) as TextAsset;
		if (textAsset == null)
		{
			Log.Message("Found no text asset in resources at " + filePath);
			return null;
		}
		return GetTextWithoutBOM(textAsset);
	}

	public static string GetTextWithoutBOM(TextAsset textAsset)
	{
		string text = null;
		using MemoryStream stream = new MemoryStream(textAsset.bytes);
		using StreamReader streamReader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
		return streamReader.ReadToEnd();
	}

	public static IEnumerable<string> LinesFromFile(string filePath)
	{
		string text = TextFromResourceFile(filePath);
		foreach (string item in GenText.LinesFromString(text))
		{
			yield return item;
		}
	}

	public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool useLinuxLineEndings = false, Func<string, string> destFileNameGetter = null)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}
		if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}
		FileInfo[] files = directoryInfo.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			string text = fileInfo.Name;
			if (destFileNameGetter != null)
			{
				text = destFileNameGetter(text);
			}
			string text2 = Path.Combine(destDirName, text);
			if (useLinuxLineEndings && (fileInfo.Extension == ".sh" || fileInfo.Extension == ".txt"))
			{
				if (!File.Exists(text2))
				{
					File.WriteAllText(text2, File.ReadAllText(fileInfo.FullName).Replace("\r\n", "\n").Replace("\r", "\n"));
				}
			}
			else
			{
				fileInfo.CopyTo(text2, overwrite: false);
			}
		}
		if (copySubDirs)
		{
			DirectoryInfo[] array = directories;
			foreach (DirectoryInfo directoryInfo2 in array)
			{
				string destDirName2 = Path.Combine(destDirName, directoryInfo2.Name);
				DirectoryCopy(directoryInfo2.FullName, destDirName2, copySubDirs, useLinuxLineEndings);
			}
		}
	}

	public static string SanitizedFileName(string fileName)
	{
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		string text = "";
		for (int i = 0; i < fileName.Length; i++)
		{
			if (!invalidFileNameChars.Contains(fileName[i]))
			{
				text += fileName[i];
			}
		}
		if (text.Length == 0)
		{
			text = "unnamed";
		}
		return text;
	}

	public static string ResolveCaseInsensitiveFilePath(string dir, string targetFileName)
	{
		string text = Path.Combine(dir, targetFileName);
		if (File.Exists(text))
		{
			return text;
		}
		char directorySeparatorChar;
		if (Directory.Exists(dir))
		{
			string[] files = Directory.GetFiles(dir);
			foreach (string path in files)
			{
				if (string.Compare(Path.GetFileName(path), targetFileName, StringComparison.CurrentCultureIgnoreCase) == 0)
				{
					directorySeparatorChar = Path.DirectorySeparatorChar;
					return dir + directorySeparatorChar + Path.GetFileName(path);
				}
			}
		}
		directorySeparatorChar = Path.DirectorySeparatorChar;
		return dir + directorySeparatorChar + targetFileName;
	}
}
