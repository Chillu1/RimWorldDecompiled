using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Verse;

public static class LogSimple
{
	private static List<string> messages = new List<string>();

	private static int tabDepth = 0;

	public static void Message(string text)
	{
		for (int i = 0; i < tabDepth; i++)
		{
			text = "  " + text;
		}
		messages.Add(text);
	}

	public static void BeginTabMessage(string text)
	{
		Message(text);
		tabDepth++;
	}

	public static void EndTab()
	{
		tabDepth--;
	}

	public static void FlushToFileAndOpen()
	{
		if (messages.Count != 0)
		{
			string value = CompiledLog();
			string saveDataFolderPath = GenFilePaths.SaveDataFolderPath;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			string path = saveDataFolderPath + directorySeparatorChar + "LogSimple.txt";
			using (StreamWriter streamWriter = new StreamWriter(path, append: false))
			{
				streamWriter.Write(value);
			}
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				Application.OpenURL(path);
			});
			messages.Clear();
		}
	}

	public static void FlushToStandardLog()
	{
		if (messages.Count != 0)
		{
			Log.Message(CompiledLog());
			messages.Clear();
		}
	}

	private static string CompiledLog()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string message in messages)
		{
			stringBuilder.AppendLine(message);
		}
		return stringBuilder.ToString().TrimEnd();
	}
}
