using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Tar;

namespace RimWorld.IO;

internal class TarDirectory : VirtualDirectory
{
	private static Dictionary<string, TarDirectory> cache = new Dictionary<string, TarDirectory>();

	private string lazyLoadArchive;

	private static readonly TarDirectory NotFound = new TarDirectory();

	private string fullPath;

	private string inArchiveFullPath;

	private string name;

	private bool exists;

	public List<TarDirectory> subDirectories = new List<TarDirectory>();

	public List<TarFile> files = new List<TarFile>();

	public override string Name => name;

	public override string FullPath => fullPath;

	public override bool Exists => exists;

	public static void ClearCache()
	{
		cache.Clear();
	}

	public static TarDirectory ReadFromFileOrCache(string file)
	{
		string key = file.Replace('\\', '/');
		if (!cache.TryGetValue(key, out var value))
		{
			value = new TarDirectory(file, "");
			value.lazyLoadArchive = file;
			cache.Add(key, value);
		}
		return value;
	}

	private void CheckLazyLoad()
	{
		if (lazyLoadArchive == null)
		{
			return;
		}
		using (FileStream inputStream = File.OpenRead(lazyLoadArchive))
		{
			using TarInputStream input = new TarInputStream(inputStream);
			ParseTAR(this, input, lazyLoadArchive);
		}
		lazyLoadArchive = null;
	}

	private static void ParseTAR(TarDirectory root, TarInputStream input, string fullPath)
	{
		Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>();
		List<TarEntry> list = new List<TarEntry>();
		List<TarDirectory> list2 = new List<TarDirectory>();
		byte[] buffer = new byte[16384];
		try
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				TarEntry nextEntry;
				while ((nextEntry = input.GetNextEntry()) != null)
				{
					ReadTarEntryData(input, memoryStream, buffer);
					dictionary.Add(nextEntry.Name, memoryStream.ToArray());
					list.Add(nextEntry);
					memoryStream.Position = 0L;
					memoryStream.SetLength(0L);
				}
			}
			list2.Add(root);
			foreach (TarEntry item in list.Where((TarEntry e) => e.IsDirectory && !string.IsNullOrEmpty(e.Name)))
			{
				string text = FormatFolderPath(item.Name);
				list2.Add(new TarDirectory(fullPath + "/" + text, text));
			}
			foreach (TarEntry item2 in list.Where((TarEntry e) => !e.IsDirectory))
			{
				string text2 = FormatFolderPath(Path.GetDirectoryName(item2.Name));
				TarDirectory tarDirectory = null;
				foreach (TarDirectory item3 in list2)
				{
					if (item3.inArchiveFullPath == text2)
					{
						tarDirectory = item3;
						break;
					}
				}
				tarDirectory.files.Add(new TarFile(dictionary[item2.Name], fullPath + "/" + item2.Name, Path.GetFileName(item2.Name)));
			}
			foreach (TarDirectory item4 in list2)
			{
				if (string.IsNullOrEmpty(item4.inArchiveFullPath))
				{
					continue;
				}
				string text3 = FormatFolderPath(Path.GetDirectoryName(item4.inArchiveFullPath));
				TarDirectory tarDirectory2 = null;
				foreach (TarDirectory item5 in list2)
				{
					if (item5.inArchiveFullPath == text3)
					{
						tarDirectory2 = item5;
						break;
					}
				}
				tarDirectory2.subDirectories.Add(item4);
			}
		}
		finally
		{
			input.Close();
		}
	}

	private static string FormatFolderPath(string str)
	{
		if (str.Length == 0)
		{
			return str;
		}
		if (str.IndexOf('\\') != -1)
		{
			str = str.Replace('\\', '/');
		}
		if (str[str.Length - 1] == '/')
		{
			str = str.Substring(0, str.Length - 1);
		}
		return str;
	}

	private static void ReadTarEntryData(TarInputStream tarIn, Stream outStream, byte[] buffer = null)
	{
		if (buffer == null)
		{
			buffer = new byte[4096];
		}
		for (int num = tarIn.Read(buffer, 0, buffer.Length); num > 0; num = tarIn.Read(buffer, 0, buffer.Length))
		{
			outStream.Write(buffer, 0, num);
		}
	}

	private static IEnumerable<TarDirectory> EnumerateAllChildrenRecursive(TarDirectory of)
	{
		foreach (TarDirectory dir in of.subDirectories)
		{
			yield return dir;
			foreach (TarDirectory item in EnumerateAllChildrenRecursive(dir))
			{
				yield return item;
			}
		}
	}

	private static IEnumerable<TarFile> EnumerateAllFilesRecursive(TarDirectory of)
	{
		foreach (TarFile file in of.files)
		{
			yield return file;
		}
		foreach (TarDirectory subDirectory in of.subDirectories)
		{
			foreach (TarFile item in EnumerateAllFilesRecursive(subDirectory))
			{
				yield return item;
			}
		}
	}

	private static Func<string, bool> GetPatternMatcher(string searchPattern)
	{
		Func<string, bool> func = null;
		if (searchPattern.Length == 1 && searchPattern[0] == '*')
		{
			func = (string str) => true;
		}
		else if (searchPattern.Length > 2 && searchPattern[0] == '*' && searchPattern[1] == '.')
		{
			string extension = searchPattern.Substring(2);
			func = (string str) => str.Substring(str.Length - extension.Length) == extension;
		}
		if (func == null)
		{
			func = (string str) => false;
		}
		return func;
	}

	private TarDirectory(string fullPath, string inArchiveFullPath)
	{
		name = Path.GetFileName(fullPath);
		if (name.IndexOf(".tar") == name.Length - 4)
		{
			name = name.Substring(0, name.Length - 4);
		}
		this.fullPath = fullPath;
		this.inArchiveFullPath = inArchiveFullPath;
		exists = true;
	}

	private TarDirectory()
	{
		exists = false;
	}

	public override VirtualDirectory GetDirectory(string directoryName)
	{
		CheckLazyLoad();
		string text = directoryName;
		if (!string.IsNullOrEmpty(fullPath))
		{
			text = fullPath + "/" + text;
		}
		foreach (TarDirectory subDirectory in subDirectories)
		{
			if (subDirectory.fullPath == text)
			{
				return subDirectory;
			}
		}
		return NotFound;
	}

	public override VirtualFile GetFile(string filename)
	{
		CheckLazyLoad();
		VirtualDirectory virtualDirectory = this;
		string[] array = filename.Split('/', '\\');
		for (int i = 0; i < array.Length - 1; i++)
		{
			virtualDirectory = virtualDirectory.GetDirectory(array[i]);
		}
		filename = array[^1];
		if (virtualDirectory == this)
		{
			foreach (TarFile file in files)
			{
				if (file.Name == filename)
				{
					return file;
				}
			}
			return TarFile.NotFound;
		}
		return virtualDirectory.GetFile(filename);
	}

	public override IEnumerable<VirtualFile> GetFiles(string searchPattern, SearchOption searchOption)
	{
		CheckLazyLoad();
		IEnumerable<TarFile> enumerable = files;
		if (searchOption == SearchOption.AllDirectories)
		{
			enumerable = EnumerateAllFilesRecursive(this);
		}
		Func<string, bool> matcher = GetPatternMatcher(searchPattern);
		foreach (TarFile item in enumerable)
		{
			if (matcher(item.Name))
			{
				yield return item;
			}
		}
	}

	public override IEnumerable<VirtualDirectory> GetDirectories(string searchPattern, SearchOption searchOption)
	{
		CheckLazyLoad();
		IEnumerable<TarDirectory> enumerable = subDirectories;
		if (searchOption == SearchOption.AllDirectories)
		{
			enumerable = EnumerateAllChildrenRecursive(this);
		}
		Func<string, bool> matcher = GetPatternMatcher(searchPattern);
		foreach (TarDirectory item in enumerable)
		{
			if (matcher(item.Name))
			{
				yield return item;
			}
		}
	}

	public override string ToString()
	{
		return $"TarDirectory [{fullPath}], {files.Count.ToString()} files";
	}
}
