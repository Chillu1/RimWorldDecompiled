using System.Collections.Generic;
using System.IO;

namespace RimWorld.IO
{
	public static class AbstractFilesystem
	{
		public static void ClearAllCache()
		{
			TarDirectory.ClearCache();
		}

		public static List<VirtualDirectory> GetDirectories(string filesystemPath, string searchPattern, SearchOption searchOption, bool allowArchiveAndRealFolderDuplicates = false)
		{
			List<VirtualDirectory> list = new List<VirtualDirectory>();
			string[] directories = Directory.GetDirectories(filesystemPath, searchPattern, searchOption);
			foreach (string text in directories)
			{
				string text2 = text + ".tar";
				if (!allowArchiveAndRealFolderDuplicates && File.Exists(text2))
				{
					list.Add(TarDirectory.ReadFromFileOrCache(text2));
				}
				else
				{
					list.Add(new FilesystemDirectory(text));
				}
			}
			directories = Directory.GetFiles(filesystemPath, searchPattern, searchOption);
			foreach (string text3 in directories)
			{
				if (Path.GetExtension(text3) != ".tar")
				{
					continue;
				}
				if (!allowArchiveAndRealFolderDuplicates)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text3);
					bool flag = false;
					foreach (VirtualDirectory item in list)
					{
						if (item.Name == fileNameWithoutExtension)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				list.Add(TarDirectory.ReadFromFileOrCache(text3));
			}
			return list;
		}

		public static VirtualDirectory GetDirectory(string filesystemPath)
		{
			if (Path.GetExtension(filesystemPath) == ".tar")
			{
				return TarDirectory.ReadFromFileOrCache(filesystemPath);
			}
			string text = filesystemPath + ".tar";
			if (File.Exists(text))
			{
				return TarDirectory.ReadFromFileOrCache(text);
			}
			return new FilesystemDirectory(filesystemPath);
		}
	}
}
