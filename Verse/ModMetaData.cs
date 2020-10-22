using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using RimWorld;
using Steamworks;
using UnityEngine;
using Verse.Steam;

namespace Verse
{
	public class ModMetaData : WorkshopUploadable
	{
		private class ModMetaDataInternal
		{
			public string packageId = "";

			public string name = "";

			public string author = "Anonymous";

			public string url = "";

			public string description = "No description provided.";

			public int steamAppId;

			public List<string> supportedVersions;

			[Unsaved(true)]
			private string targetVersion;

			public List<ModDependency> modDependencies = new List<ModDependency>();

			public List<string> loadBefore = new List<string>();

			public List<string> loadAfter = new List<string>();

			public List<string> incompatibleWith = new List<string>();

			private VersionedData<string> descriptionsByVersion;

			private VersionedData<List<ModDependency>> modDependenciesByVersion;

			private VersionedData<List<string>> loadBeforeByVersion;

			private VersionedData<List<string>> loadAfterByVersion;

			private VersionedData<List<string>> incompatibleWithByVersion;

			public static readonly Regex PackageIdFormatRegex = new Regex("(?=.{1,60}$)^(?!\\.)(?=.*?[.])(?!.*([.])\\1+)[a-zA-Z0-9.]{1,}[a-zA-Z0-9]{1}$");

			public List<System.Version> SupportedVersions
			{
				get;
				private set;
			}

			private bool TryParseVersion(string str, bool logIssues = true)
			{
				if (!VersionControl.TryParseVersionString(str, out var version))
				{
					if (logIssues)
					{
						Log.Error("Unable to parse version string on mod " + name + " from " + author + " \"" + str + "\"");
					}
					return false;
				}
				SupportedVersions.Add(version);
				if (!VersionControl.IsWellFormattedVersionString(str))
				{
					if (logIssues)
					{
						Log.Warning("Malformed (correct format is Major.Minor) version string on mod " + name + " from " + author + " \"" + str + "\" - parsed as \"" + version.Major + "." + version.Minor + "\"");
					}
					return false;
				}
				return true;
			}

			public bool TryParseSupportedVersions(bool logIssues = true)
			{
				if (targetVersion != null && logIssues)
				{
					Log.Warning("Mod " + name + ": targetVersion field is obsolete, use supportedVersions instead.");
				}
				bool flag = false;
				SupportedVersions = new List<System.Version>();
				if (packageId.ToLower() == ModContentPack.CoreModPackageId)
				{
					SupportedVersions.Add(VersionControl.CurrentVersion);
				}
				else if (supportedVersions == null)
				{
					if (logIssues)
					{
						Log.Warning("Mod " + name + " is missing supported versions list in About.xml! (example: <supportedVersions><li>1.0</li></supportedVersions>)");
					}
					flag = true;
				}
				else if (supportedVersions.Count == 0)
				{
					if (logIssues)
					{
						Log.Error("Mod " + name + ": <supportedVersions> in mod About.xml must specify at least one version.");
					}
					flag = true;
				}
				else
				{
					for (int i = 0; i < supportedVersions.Count; i++)
					{
						flag |= !TryParseVersion(supportedVersions[i], logIssues);
					}
				}
				SupportedVersions = (from v in SupportedVersions
					orderby (!VersionControl.IsCompatible(v)) ? 100 : (-100), v.Major descending, v.Minor descending
					select v).Distinct().ToList();
				return !flag;
			}

			public bool TryParsePackageId(bool isOfficial, bool logIssues = true)
			{
				bool flag = false;
				if (packageId.NullOrEmpty())
				{
					string str = "none";
					if (!description.NullOrEmpty())
					{
						str = GenText.StableStringHash(description).ToString().Replace("-", "");
						str = str.Substring(0, Math.Min(3, str.Length));
					}
					packageId = ConvertToASCII(author + str) + "." + ConvertToASCII(name);
					if (logIssues)
					{
						Log.Warning("Mod " + name + " is missing packageId in About.xml! (example: <packageId>AuthorName.ModName.Specific</packageId>)");
					}
					flag = true;
				}
				if (!PackageIdFormatRegex.IsMatch(packageId))
				{
					if (logIssues)
					{
						Log.Warning("Mod " + name + " <packageId> (" + packageId + ") is not in valid format.");
					}
					flag = true;
				}
				if (!isOfficial && packageId.ToLower().Contains(ModContentPack.LudeonPackageIdAuthor))
				{
					if (logIssues)
					{
						Log.Warning("Mod " + name + " <packageId> contains word \"Ludeon\", which is reserved for official content.");
					}
					flag = true;
				}
				return !flag;
			}

			private string ConvertToASCII(string part)
			{
				StringBuilder stringBuilder = new StringBuilder("");
				for (int i = 0; i < part.Length; i++)
				{
					char c = part[i];
					if (!char.IsLetterOrDigit(c) || c >= '\u0080')
					{
						c = (char)((int)c % 25 + 65);
					}
					stringBuilder.Append(c);
				}
				return stringBuilder.ToString();
			}

			[Obsolete("Only need this overload to not break mod compatibility.")]
			public void ValidateDependencies()
			{
				ValidateDependencies_NewTmp();
			}

			public void ValidateDependencies_NewTmp(bool logIssues = true)
			{
				for (int num = modDependencies.Count - 1; num >= 0; num--)
				{
					bool flag = false;
					ModDependency modDependency = modDependencies[num];
					if (modDependency.packageId.NullOrEmpty())
					{
						if (logIssues)
						{
							Log.Warning("Mod " + name + " has a dependency with no <packageId> specified.");
						}
						flag = true;
					}
					else if (!PackageIdFormatRegex.IsMatch(modDependency.packageId))
					{
						if (logIssues)
						{
							Log.Warning("Mod " + name + " has a dependency with invalid <packageId>: " + modDependency.packageId);
						}
						flag = true;
					}
					if (modDependency.displayName.NullOrEmpty())
					{
						if (logIssues)
						{
							Log.Warning("Mod " + name + " has a dependency (" + modDependency.packageId + ") with empty display name.");
						}
						flag = true;
					}
					if (modDependency.downloadUrl.NullOrEmpty() && modDependency.steamWorkshopUrl.NullOrEmpty() && !modDependency.packageId.ToLower().Contains(ModContentPack.LudeonPackageIdAuthor))
					{
						if (logIssues)
						{
							Log.Warning("Mod " + name + " dependency (" + modDependency.packageId + ") needs to have <downloadUrl> and/or <steamWorkshopUrl> specified.");
						}
						flag = true;
					}
					if (flag)
					{
						modDependencies.Remove(modDependency);
					}
				}
			}

			public void InitVersionedData()
			{
				string currentVersionStringWithoutBuild = VersionControl.CurrentVersionStringWithoutBuild;
				string text = descriptionsByVersion?.GetItemForVersion(currentVersionStringWithoutBuild);
				if (text != null)
				{
					description = text;
				}
				List<ModDependency> list = modDependenciesByVersion?.GetItemForVersion(currentVersionStringWithoutBuild);
				if (list != null)
				{
					modDependencies = list;
				}
				List<string> list2 = loadBeforeByVersion?.GetItemForVersion(currentVersionStringWithoutBuild);
				if (list2 != null)
				{
					loadBefore = list2;
				}
				List<string> list3 = loadAfterByVersion?.GetItemForVersion(currentVersionStringWithoutBuild);
				if (list3 != null)
				{
					loadAfter = list3;
				}
				List<string> list4 = incompatibleWithByVersion?.GetItemForVersion(currentVersionStringWithoutBuild);
				if (list4 != null)
				{
					incompatibleWith = list4;
				}
			}
		}

		private class VersionedData<T> where T : class
		{
			private Dictionary<string, T> itemForVersion = new Dictionary<string, T>();

			public void LoadDataFromXmlCustom(XmlNode xmlRoot)
			{
				foreach (XmlNode childNode in xmlRoot.ChildNodes)
				{
					if (!(childNode is XmlComment))
					{
						string text = childNode.Name.ToLower();
						if (text.StartsWith("v"))
						{
							text = text.Substring(1);
						}
						if (!itemForVersion.ContainsKey(text))
						{
							itemForVersion[text] = ((typeof(T) == typeof(string)) ? ((T)(object)childNode.FirstChild.Value) : DirectXmlToObject.ObjectFromXml<T>(childNode, doPostLoad: false));
						}
						else
						{
							Log.Warning("More than one value for a same version of " + typeof(T).Name + " named " + xmlRoot.Name);
						}
					}
				}
			}

			public T GetItemForVersion(string ver)
			{
				if (itemForVersion.ContainsKey(ver))
				{
					return itemForVersion[ver];
				}
				return null;
			}
		}

		private DirectoryInfo rootDirInt;

		private ContentSource source;

		private Texture2D previewImage;

		private bool previewImageWasLoaded;

		public bool enabled = true;

		private ModMetaDataInternal meta = new ModMetaDataInternal();

		public ModLoadFolders loadFolders;

		private WorkshopItemHook workshopHookInt;

		private PublishedFileId_t publishedFileIdInt = PublishedFileId_t.Invalid;

		public bool appendPackageIdSteamPostfix;

		public bool translationMod;

		private string packageIdLowerCase;

		private string descriptionCached;

		private const string AboutFolderName = "About";

		public static readonly string SteamModPostfix = "_steam";

		private List<string> unsatisfiedDepsList = new List<string>();

		public Texture2D PreviewImage
		{
			get
			{
				if (previewImageWasLoaded)
				{
					return previewImage;
				}
				if (File.Exists(PreviewImagePath))
				{
					previewImage = new Texture2D(0, 0);
					previewImage.LoadImage(File.ReadAllBytes(PreviewImagePath));
				}
				previewImageWasLoaded = true;
				return previewImage;
			}
		}

		public string FolderName => RootDir.Name;

		public DirectoryInfo RootDir => rootDirInt;

		public bool IsCoreMod => SamePackageId(ModContentPack.CoreModPackageId);

		public bool Active
		{
			get
			{
				return ModsConfig.IsActive(this);
			}
			set
			{
				ModsConfig.SetActive(this, value);
			}
		}

		public bool VersionCompatible
		{
			get
			{
				if (IsCoreMod)
				{
					return true;
				}
				return meta.SupportedVersions.Any((System.Version v) => VersionControl.IsCompatible(v));
			}
		}

		public bool MadeForNewerVersion
		{
			get
			{
				if (VersionCompatible)
				{
					return false;
				}
				return meta.SupportedVersions.Any((System.Version v) => v.Major > VersionControl.CurrentMajor || (v.Major == VersionControl.CurrentMajor && v.Minor > VersionControl.CurrentMinor));
			}
		}

		public ExpansionDef Expansion => ModLister.GetExpansionWithIdentifier(PackageId);

		public string Name
		{
			get
			{
				ExpansionDef expansion = Expansion;
				if (expansion == null)
				{
					return meta.name;
				}
				return expansion.label;
			}
		}

		public string Description
		{
			get
			{
				if (descriptionCached == null)
				{
					ExpansionDef expansionWithIdentifier = ModLister.GetExpansionWithIdentifier(PackageId);
					descriptionCached = ((expansionWithIdentifier != null) ? expansionWithIdentifier.description : meta.description);
				}
				return descriptionCached;
			}
		}

		public string Author => meta.author;

		public string Url => meta.url;

		public int SteamAppId => meta.steamAppId;

		[Obsolete("Deprecated, will be removed in the future. Use SupportedVersions instead")]
		public string TargetVersion
		{
			get
			{
				if (SupportedVersionsReadOnly.Count == 0)
				{
					return "Unknown";
				}
				System.Version version = meta.SupportedVersions[0];
				return version.Major + "." + version.Minor;
			}
		}

		public List<System.Version> SupportedVersionsReadOnly => meta.SupportedVersions;

		IEnumerable<System.Version> WorkshopUploadable.SupportedVersions => SupportedVersionsReadOnly;

		public string PreviewImagePath => rootDirInt.FullName + Path.DirectorySeparatorChar + "About" + Path.DirectorySeparatorChar + "Preview.png";

		public bool Official
		{
			get
			{
				if (!IsCoreMod)
				{
					return Source == ContentSource.OfficialModsFolder;
				}
				return true;
			}
		}

		public ContentSource Source => source;

		public string PackageId
		{
			get
			{
				if (!appendPackageIdSteamPostfix)
				{
					return packageIdLowerCase;
				}
				return packageIdLowerCase + SteamModPostfix;
			}
		}

		public string PackageIdNonUnique => packageIdLowerCase;

		public string PackageIdPlayerFacing => meta.packageId;

		public List<ModDependency> Dependencies => meta.modDependencies;

		public List<string> LoadBefore => meta.loadBefore;

		public List<string> LoadAfter => meta.loadAfter;

		public List<string> IncompatibleWith => meta.incompatibleWith;

		public bool HadIncorrectlyFormattedVersionInMetadata
		{
			get;
			private set;
		}

		public bool HadIncorrectlyFormattedPackageId
		{
			get;
			private set;
		}

		public bool OnSteamWorkshop => source == ContentSource.SteamWorkshop;

		private string PublishedFileIdPath => rootDirInt.FullName + Path.DirectorySeparatorChar + "About" + Path.DirectorySeparatorChar + "PublishedFileId.txt";

		public List<string> UnsatisfiedDependencies()
		{
			unsatisfiedDepsList.Clear();
			for (int i = 0; i < Dependencies.Count; i++)
			{
				ModDependency modDependency = Dependencies[i];
				if (!modDependency.IsSatisfied)
				{
					unsatisfiedDepsList.Add(modDependency.displayName);
				}
			}
			return unsatisfiedDepsList;
		}

		public ModMetaData(string localAbsPath, bool official = false)
		{
			rootDirInt = new DirectoryInfo(localAbsPath);
			source = (official ? ContentSource.OfficialModsFolder : ContentSource.ModsFolder);
			Init();
		}

		public ModMetaData(WorkshopItem workshopItem)
		{
			rootDirInt = workshopItem.Directory;
			source = ContentSource.SteamWorkshop;
			Init();
		}

		public void UnsetPreviewImage()
		{
			previewImage = null;
		}

		public bool SamePackageId(string otherPackageId, bool ignorePostfix = false)
		{
			if (PackageId == null)
			{
				return false;
			}
			if (ignorePostfix)
			{
				return packageIdLowerCase.Equals(otherPackageId, StringComparison.CurrentCultureIgnoreCase);
			}
			return PackageId.Equals(otherPackageId, StringComparison.CurrentCultureIgnoreCase);
		}

		public List<LoadFolder> LoadFoldersForVersion(string version)
		{
			return loadFolders?.FoldersForVersion(version);
		}

		private void Init()
		{
			meta = DirectXmlLoader.ItemFromXmlFile<ModMetaDataInternal>(RootDir.FullName + Path.DirectorySeparatorChar + "About" + Path.DirectorySeparatorChar + "About.xml");
			loadFolders = DirectXmlLoader.ItemFromXmlFile<ModLoadFolders>(RootDir.FullName + Path.DirectorySeparatorChar + "LoadFolders.xml");
			bool shouldLogIssues = ModLister.ShouldLogIssues;
			HadIncorrectlyFormattedVersionInMetadata = !meta.TryParseSupportedVersions(!OnSteamWorkshop && shouldLogIssues);
			if (meta.name.NullOrEmpty())
			{
				if (OnSteamWorkshop)
				{
					meta.name = "Workshop mod " + FolderName;
				}
				else
				{
					meta.name = FolderName;
				}
			}
			HadIncorrectlyFormattedPackageId = !meta.TryParsePackageId(Official, !OnSteamWorkshop && shouldLogIssues);
			packageIdLowerCase = meta.packageId.ToLower();
			meta.InitVersionedData();
			meta.ValidateDependencies_NewTmp(shouldLogIssues);
			string publishedFileIdPath = PublishedFileIdPath;
			if (File.Exists(PublishedFileIdPath) && ulong.TryParse(File.ReadAllText(publishedFileIdPath), out var result))
			{
				publishedFileIdInt = new PublishedFileId_t(result);
			}
		}

		internal void DeleteContent()
		{
			rootDirInt.Delete(recursive: true);
			ModLister.RebuildModList();
		}

		public void PrepareForWorkshopUpload()
		{
		}

		public bool CanToUploadToWorkshop()
		{
			if (Official)
			{
				return false;
			}
			if (Source != ContentSource.ModsFolder)
			{
				return false;
			}
			if (GetWorkshopItemHook().MayHaveAuthorNotCurrentUser)
			{
				return false;
			}
			return true;
		}

		public PublishedFileId_t GetPublishedFileId()
		{
			return publishedFileIdInt;
		}

		public void SetPublishedFileId(PublishedFileId_t newPfid)
		{
			if (!(publishedFileIdInt == newPfid))
			{
				publishedFileIdInt = newPfid;
				File.WriteAllText(PublishedFileIdPath, newPfid.ToString());
			}
		}

		public string GetWorkshopName()
		{
			return Name;
		}

		public string GetWorkshopDescription()
		{
			return Description;
		}

		public string GetWorkshopPreviewImagePath()
		{
			return PreviewImagePath;
		}

		public IList<string> GetWorkshopTags()
		{
			if (!translationMod)
			{
				return new List<string>
				{
					"Mod"
				};
			}
			return new List<string>
			{
				"Translation"
			};
		}

		public DirectoryInfo GetWorkshopUploadDirectory()
		{
			return RootDir;
		}

		public WorkshopItemHook GetWorkshopItemHook()
		{
			if (workshopHookInt == null)
			{
				workshopHookInt = new WorkshopItemHook(this);
			}
			return workshopHookInt;
		}

		public IEnumerable<ModRequirement> GetRequirements()
		{
			for (int j = 0; j < Dependencies.Count; j++)
			{
				yield return Dependencies[j];
			}
			for (int j = 0; j < meta.incompatibleWith.Count; j++)
			{
				ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(meta.incompatibleWith[j]);
				if (modWithIdentifier != null)
				{
					yield return new ModIncompatibility
					{
						packageId = modWithIdentifier.PackageIdPlayerFacing,
						displayName = modWithIdentifier.Name
					};
				}
			}
		}

		public override int GetHashCode()
		{
			return PackageId.GetHashCode();
		}

		public override string ToString()
		{
			return "[" + PackageIdPlayerFacing + "|" + Name + "]";
		}

		public string ToStringLong()
		{
			return PackageIdPlayerFacing + "(" + RootDir.ToString() + ")";
		}
	}
}
