using System;
using System.IO;
using System.Threading;
using RimWorld;
using UnityEngine;

namespace Verse;

public class SaveFileInfo
{
	private FileInfo fileInfo;

	private string gameVersion;

	private DateTime lastWriteTime;

	private string fileName;

	private bool loaded;

	private object lockObject = new object();

	public static readonly Color UnimportantTextColor = new Color(1f, 1f, 1f, 0.5f);

	private bool Valid
	{
		get
		{
			lock (lockObject)
			{
				return gameVersion != null;
			}
		}
	}

	public FileInfo FileInfo => fileInfo;

	public string FileName => fileName;

	public DateTime LastWriteTime => lastWriteTime;

	public string GameVersion
	{
		get
		{
			bool flag = false;
			try
			{
				if (flag = TryLock(0))
				{
					if (!loaded)
					{
						return "LoadingVersionInfo".Translate();
					}
					if (!Valid)
					{
						return "???";
					}
					return gameVersion;
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(lockObject);
				}
			}
			return "LoadingVersionInfo".Translate();
		}
	}

	public Color VersionColor
	{
		get
		{
			bool flag = false;
			try
			{
				if (flag = TryLock(0))
				{
					if (!loaded)
					{
						return Color.gray;
					}
					if (!Valid)
					{
						return ColorLibrary.RedReadable;
					}
					if (VersionControl.MajorFromVersionString(gameVersion) != VersionControl.CurrentMajor || VersionControl.MinorFromVersionString(gameVersion) != VersionControl.CurrentMinor)
					{
						if (BackCompatibility.IsSaveCompatibleWith(gameVersion))
						{
							return Color.yellow;
						}
						return ColorLibrary.RedReadable;
					}
					return UnimportantTextColor;
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(lockObject);
				}
			}
			return Color.gray;
		}
	}

	public TipSignal CompatibilityTip
	{
		get
		{
			bool flag = false;
			try
			{
				if (flag = TryLock(0))
				{
					if (!loaded)
					{
						return "LoadingVersionInfo".Translate();
					}
					if (!Valid)
					{
						return "SaveIsUnknownFormat".Translate();
					}
					if ((VersionControl.MajorFromVersionString(gameVersion) != VersionControl.CurrentMajor || VersionControl.MinorFromVersionString(gameVersion) != VersionControl.CurrentMinor) && !BackCompatibility.IsSaveCompatibleWith(gameVersion))
					{
						return "SaveIsFromDifferentGameVersion".Translate(VersionControl.CurrentVersionString, gameVersion);
					}
					if (VersionControl.BuildFromVersionString(gameVersion) != VersionControl.CurrentBuild)
					{
						return "SaveIsFromDifferentGameBuild".Translate(VersionControl.CurrentVersionString, gameVersion);
					}
					return "SaveIsFromThisGameBuild".Translate();
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(lockObject);
				}
			}
			return "LoadingVersionInfo".Translate();
		}
	}

	private bool TryLock(int timeoutMilliseconds)
	{
		return Monitor.TryEnter(lockObject, timeoutMilliseconds);
	}

	public SaveFileInfo(FileInfo fileInfo)
	{
		this.fileInfo = fileInfo;
		fileName = fileInfo.Name;
		lastWriteTime = fileInfo.LastWriteTime;
	}

	public void LoadData()
	{
		lock (lockObject)
		{
			gameVersion = ScribeMetaHeaderUtility.GameVersionOf(fileInfo);
			loaded = true;
		}
	}
}
