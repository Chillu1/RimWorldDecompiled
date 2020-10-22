using System.IO;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public struct SaveFileInfo
	{
		private FileInfo fileInfo;

		private string gameVersion;

		public static readonly Color UnimportantTextColor = new Color(1f, 1f, 1f, 0.5f);

		public bool Valid => gameVersion != null;

		public FileInfo FileInfo => fileInfo;

		public string GameVersion
		{
			get
			{
				if (!Valid)
				{
					return "???";
				}
				return gameVersion;
			}
		}

		public Color VersionColor
		{
			get
			{
				if (!Valid)
				{
					return ColoredText.RedReadable;
				}
				if (VersionControl.MajorFromVersionString(gameVersion) != VersionControl.CurrentMajor || VersionControl.MinorFromVersionString(gameVersion) != VersionControl.CurrentMinor)
				{
					if (BackCompatibility.IsSaveCompatibleWith(gameVersion))
					{
						return Color.yellow;
					}
					return ColoredText.RedReadable;
				}
				return UnimportantTextColor;
			}
		}

		public TipSignal CompatibilityTip
		{
			get
			{
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

		public SaveFileInfo(FileInfo fileInfo)
		{
			this.fileInfo = fileInfo;
			gameVersion = ScribeMetaHeaderUtility.GameVersionOf(fileInfo);
		}
	}
}
