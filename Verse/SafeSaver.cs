using System;
using System.IO;
using System.Threading;

namespace Verse;

public static class SafeSaver
{
	private static readonly string NewFileSuffix = ".new";

	private static readonly string OldFileSuffix = ".old";

	private static string GetFileFullPath(string path)
	{
		return Path.GetFullPath(path);
	}

	private static string GetNewFileFullPath(string path)
	{
		return Path.GetFullPath(path + NewFileSuffix);
	}

	private static string GetOldFileFullPath(string path)
	{
		return Path.GetFullPath(path + OldFileSuffix);
	}

	public static void Save(string path, string documentElementName, Action saveAction, bool leaveOldFile = false)
	{
		try
		{
			CleanSafeSaverFiles(path);
			if (!File.Exists(GetFileFullPath(path)))
			{
				DoSave(GetFileFullPath(path), documentElementName, saveAction);
				return;
			}
			DoSave(GetNewFileFullPath(path), documentElementName, saveAction);
			try
			{
				SafeMove(GetFileFullPath(path), GetOldFileFullPath(path));
			}
			catch (Exception ex)
			{
				Log.Warning("Could not move file from \"" + GetFileFullPath(path) + "\" to \"" + GetOldFileFullPath(path) + "\": " + ex);
				throw;
			}
			try
			{
				SafeMove(GetNewFileFullPath(path), GetFileFullPath(path));
			}
			catch (Exception ex2)
			{
				Log.Warning("Could not move file from \"" + GetNewFileFullPath(path) + "\" to \"" + GetFileFullPath(path) + "\": " + ex2);
				RemoveFileIfExists(GetFileFullPath(path), rethrow: false);
				RemoveFileIfExists(GetNewFileFullPath(path), rethrow: false);
				try
				{
					SafeMove(GetOldFileFullPath(path), GetFileFullPath(path));
				}
				catch (Exception ex3)
				{
					Log.Warning("Could not move file from \"" + GetOldFileFullPath(path) + "\" back to \"" + GetFileFullPath(path) + "\": " + ex3);
				}
				throw;
			}
			if (!leaveOldFile)
			{
				RemoveFileIfExists(GetOldFileFullPath(path), rethrow: true);
			}
		}
		catch (Exception ex4)
		{
			GenUI.ErrorDialog("ProblemSavingFile".Translate(GetFileFullPath(path), ex4.ToString()));
			throw;
		}
	}

	private static void CleanSafeSaverFiles(string path)
	{
		RemoveFileIfExists(GetOldFileFullPath(path), rethrow: true);
		RemoveFileIfExists(GetNewFileFullPath(path), rethrow: true);
	}

	private static void DoSave(string fullPath, string documentElementName, Action saveAction)
	{
		try
		{
			Scribe.saver.InitSaving(fullPath, documentElementName);
			saveAction();
			Scribe.saver.FinalizeSaving();
		}
		catch (Exception ex)
		{
			Log.Warning("An exception was thrown during saving to \"" + fullPath + "\": " + ex);
			Scribe.saver.ForceStop();
			RemoveFileIfExists(fullPath, rethrow: false);
			throw;
		}
	}

	private static void RemoveFileIfExists(string path, bool rethrow)
	{
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception ex)
		{
			Log.Warning("Could not remove file \"" + path + "\": " + ex);
			if (rethrow)
			{
				throw;
			}
		}
	}

	private static void SafeMove(string from, string to)
	{
		Exception ex = null;
		for (int i = 0; i < 50; i++)
		{
			try
			{
				File.Move(from, to);
				return;
			}
			catch (Exception ex2)
			{
				if (ex == null)
				{
					ex = ex2;
				}
			}
			Thread.Sleep(1);
		}
		throw ex;
	}
}
