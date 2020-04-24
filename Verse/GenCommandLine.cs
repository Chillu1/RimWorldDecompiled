using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Verse
{
	public static class GenCommandLine
	{
		public static bool CommandLineArgPassed(string key)
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (string.Compare(commandLineArgs[i], key, ignoreCase: true) == 0 || string.Compare(commandLineArgs[i], "-" + key, ignoreCase: true) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public static bool TryGetCommandLineArg(string key, out string value)
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (commandLineArgs[i].Contains('='))
				{
					string[] array = commandLineArgs[i].Split('=');
					if (array.Length == 2 && (string.Compare(array[0], key, ignoreCase: true) == 0 || string.Compare(array[0], "-" + key, ignoreCase: true) == 0))
					{
						value = array[1];
						return true;
					}
				}
			}
			value = null;
			return false;
		}

		public static void Restart()
		{
			try
			{
				string[] commandLineArgs = Environment.GetCommandLineArgs();
				string fileName = commandLineArgs[0];
				string text = "";
				for (int i = 1; i < commandLineArgs.Length; i++)
				{
					if (!text.NullOrEmpty())
					{
						text += " ";
					}
					text = text + "\"" + commandLineArgs[i].Replace("\"", "\\\"") + "\"";
				}
				Process process = new Process();
				process.StartInfo = new ProcessStartInfo(fileName, text);
				process.Start();
				Root.Shutdown();
				LongEventHandler.QueueLongEvent(delegate
				{
					Thread.Sleep(10000);
				}, "Restarting", doAsynchronously: true, null);
			}
			catch (Exception arg)
			{
				Log.Error("Error restarting: " + arg);
				Find.WindowStack.Add(new Dialog_MessageBox("FailedToRestart".Translate()));
			}
		}
	}
}
