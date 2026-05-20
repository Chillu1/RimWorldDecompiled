using System;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace RimWorld;

public static class VersionControl
{
	private static Version version;

	private static string versionStringWithoutBuild;

	private static string versionString;

	private static string versionStringWithRev;

	private static DateTime buildDate;

	public static Version CurrentVersion => version;

	public static string CurrentVersionString => versionString;

	public static string CurrentVersionStringWithoutBuild => versionStringWithoutBuild;

	public static string CurrentVersionStringWithRev => versionStringWithRev;

	public static int CurrentMajor => version.Major;

	public static int CurrentMinor => version.Minor;

	public static int CurrentBuild => version.Build;

	public static int CurrentRevision => version.Revision;

	public static DateTime CurrentBuildDate => buildDate;

	static VersionControl()
	{
		Version version = Assembly.GetExecutingAssembly().GetName().Version;
		buildDate = new DateTime(2000, 1, 1).AddDays(version.Build);
		int build = version.Build - 4805;
		int revision = version.Revision * 2 / 60;
		VersionControl.version = new Version(version.Major, version.Minor, build, revision);
		versionStringWithRev = VersionControl.version.Major + "." + VersionControl.version.Minor + "." + VersionControl.version.Build + " rev" + VersionControl.version.Revision;
		versionString = VersionControl.version.Major + "." + VersionControl.version.Minor + "." + VersionControl.version.Build;
		versionStringWithoutBuild = VersionControl.version.Major + "." + VersionControl.version.Minor;
	}

	public static void DrawInfoInCorner()
	{
		Text.Font = GameFont.Small;
		GUI.color = new Color(1f, 1f, 1f, 0.5f);
		string text = "VersionIndicator".Translate(versionString);
		string versionExtraInfo = GetVersionExtraInfo();
		if (!versionExtraInfo.NullOrEmpty())
		{
			text = text + " (" + versionExtraInfo + ")";
		}
		text += "\n" + "CompiledOn".Translate(buildDate.ToString("MMM d yyyy"));
		if (SteamManager.Initialized)
		{
			text += "\n" + "LoggedIntoSteamAs".Translate(SteamUtility.SteamPersonaName);
		}
		Rect rect = new Rect(10f, 10f, 330f, Text.CalcHeight(text, 330f));
		Widgets.Label(rect, text);
		GUI.color = Color.white;
		LatestVersionGetter component = Current.Root.gameObject.GetComponent<LatestVersionGetter>();
		Rect rect2 = new Rect(10f, rect.yMax - 5f, 330f, 999f);
		component.DrawAt(rect2);
	}

	private static string GetVersionExtraInfo()
	{
		string text = "";
		if (UnityData.Is32BitBuild)
		{
			text += "32-bit";
		}
		else if (UnityData.Is64BitBuild)
		{
			text += "64-bit";
		}
		return text;
	}

	public static void LogVersionNumber()
	{
		Log.Message("RimWorld " + versionStringWithRev);
	}

	public static bool IsCompatible(Version v)
	{
		if (v.Major == CurrentMajor)
		{
			return v.Minor == CurrentMinor;
		}
		return false;
	}

	public static bool TryParseVersionString(string str, out Version version)
	{
		version = null;
		if (str == null)
		{
			return false;
		}
		string[] array = str.Split('.');
		if (array.Length < 2)
		{
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			if (!int.TryParse(array[i], out var result))
			{
				return false;
			}
			if (result < 0)
			{
				return false;
			}
		}
		version = new Version(int.Parse(array[0]), int.Parse(array[1]));
		return true;
	}

	public static int BuildFromVersionString(string str)
	{
		str = VersionStringWithoutRev(str);
		int result = 0;
		string[] array = str.Split('.');
		if (array.Length < 3 || !int.TryParse(array[2], out result))
		{
			Log.Warning("Could not get build from version string " + str);
		}
		return result;
	}

	public static int MinorFromVersionString(string str)
	{
		str = VersionStringWithoutRev(str);
		int result = 0;
		string[] array = str.Split('.');
		if (array.Length < 2 || !int.TryParse(array[1], out result))
		{
			Log.Warning("Could not get minor version from version string " + str);
		}
		return result;
	}

	public static int MajorFromVersionString(string str)
	{
		str = VersionStringWithoutRev(str);
		int result = 0;
		if (!int.TryParse(str.Split('.')[0], out result))
		{
			Log.Warning("Could not get major version from version string " + str);
		}
		return result;
	}

	public static string VersionStringWithoutRev(string str)
	{
		return str.Split(' ')[0];
	}

	public static Version VersionFromString(string str)
	{
		if (str.NullOrEmpty())
		{
			throw new ArgumentException("str");
		}
		string[] array = str.Split('.');
		if (array.Length > 3)
		{
			throw new ArgumentException("str");
		}
		int major = 0;
		int minor = 0;
		int build = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (!int.TryParse(array[i], out var result))
			{
				throw new ArgumentException("str");
			}
			if (result < 0)
			{
				throw new ArgumentException("str");
			}
			switch (i)
			{
			case 0:
				major = result;
				break;
			case 1:
				minor = result;
				break;
			case 2:
				build = result;
				break;
			}
		}
		return new Version(major, minor, build);
	}

	public static bool IsWellFormattedVersionString(string str)
	{
		string[] array = str.Split('.');
		if (array.Length != 2)
		{
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			if (!int.TryParse(array[i], out var result))
			{
				return false;
			}
			if (result < 0)
			{
				return false;
			}
		}
		return true;
	}
}
