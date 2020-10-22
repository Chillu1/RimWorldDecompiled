using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using RimWorld;

namespace Verse
{
	public class ModLoadFolders
	{
		private Dictionary<string, List<LoadFolder>> foldersForVersion = new Dictionary<string, List<LoadFolder>>();

		public const string defaultVersionName = "default";

		public List<LoadFolder> FoldersForVersion(string version)
		{
			if (foldersForVersion.ContainsKey(version))
			{
				return foldersForVersion[version];
			}
			return null;
		}

		public List<string> DefinedVersions()
		{
			return foldersForVersion.Keys.ToList();
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			foreach (XmlNode childNode in xmlRoot.ChildNodes)
			{
				if (childNode is XmlComment)
				{
					continue;
				}
				string text = childNode.Name.ToLower();
				if (text.StartsWith("v"))
				{
					text = text.Substring(1);
				}
				if (!foldersForVersion.ContainsKey(text))
				{
					foldersForVersion.Add(text, new List<LoadFolder>());
				}
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2 is XmlComment)
					{
						continue;
					}
					XmlAttribute xmlAttribute = childNode2.Attributes?["IfModActive"];
					List<string> requiredPackageIds = null;
					if (xmlAttribute != null)
					{
						requiredPackageIds = (from s in xmlAttribute.Value.Split(',')
							select s.Trim()).ToList();
					}
					XmlAttribute xmlAttribute2 = childNode2.Attributes?["IfModNotActive"];
					List<string> disallowedPackageIds = null;
					if (xmlAttribute2 != null)
					{
						disallowedPackageIds = (from s in xmlAttribute2.Value.Split(',')
							select s.Trim()).ToList();
					}
					if (childNode2.InnerText == "/" || childNode2.InnerText == "\\")
					{
						foldersForVersion[text].Add(new LoadFolder("", requiredPackageIds, disallowedPackageIds));
					}
					else
					{
						foldersForVersion[text].Add(new LoadFolder(childNode2.InnerText, requiredPackageIds, disallowedPackageIds));
					}
				}
			}
		}

		public List<string> GetIssueList(ModMetaData mod)
		{
			List<string> list = new List<string>();
			if (foldersForVersion.Count > 0)
			{
				string text = null;
				{
					foreach (string key in foldersForVersion.Keys)
					{
						if (foldersForVersion[key].Count == 0)
						{
							list.Add("ModLoadFolderListEmpty".Translate(key));
						}
						foreach (LoadFolder item in from f in foldersForVersion[key]
							group f by f into g
							where g.Count() > 1
							select g.Key)
						{
							list.Add("ModLoadFolderRepeatingFolder".Translate(key, item.folderName));
						}
						if (!VersionControl.IsWellFormattedVersionString(key) && !key.Equals("default", StringComparison.InvariantCultureIgnoreCase))
						{
							list.Add("ModLoadFolderMalformedVersion".Translate(key));
						}
						if (key.Equals("default"))
						{
							list.Add("ModLoadFolderDefaultDeprecated".Translate());
						}
						if (text != null && VersionControl.TryParseVersionString(key, out var version) && VersionControl.TryParseVersionString(text, out var version2) && version < version2)
						{
							list.Add("ModLoadFolderOutOfOrder".Translate(key, text));
						}
						for (int i = 0; i < foldersForVersion[key].Count; i++)
						{
							LoadFolder loadFolder = foldersForVersion[key][i];
							if (!Directory.Exists(Path.Combine(mod.RootDir.FullName, loadFolder.folderName)))
							{
								list.Add("ModLoadFolderDoesntExist".Translate(loadFolder.folderName, key));
							}
						}
						if (VersionControl.TryParseVersionString(key, out var version3) && !mod.SupportedVersionsReadOnly.Contains(version3))
						{
							list.Add("ModLoadFolderDefinesUnsupportedGameVersion".Translate(key));
						}
						text = key;
					}
					return list;
				}
			}
			return list;
		}
	}
}
