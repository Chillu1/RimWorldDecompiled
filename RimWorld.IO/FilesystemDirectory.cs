using System.Collections.Generic;
using System.IO;

namespace RimWorld.IO
{
	internal class FilesystemDirectory : VirtualDirectory
	{
		private DirectoryInfo dirInfo;

		public override string Name => dirInfo.Name;

		public override string FullPath => dirInfo.FullName;

		public override bool Exists => dirInfo.Exists;

		public FilesystemDirectory(string dir)
		{
			dirInfo = new DirectoryInfo(dir);
		}

		public FilesystemDirectory(DirectoryInfo dir)
		{
			dirInfo = dir;
		}

		public override IEnumerable<VirtualDirectory> GetDirectories(string searchPattern, SearchOption searchOption)
		{
			DirectoryInfo[] directories = dirInfo.GetDirectories(searchPattern, searchOption);
			foreach (DirectoryInfo dir in directories)
			{
				yield return new FilesystemDirectory(dir);
			}
		}

		public override VirtualDirectory GetDirectory(string directoryName)
		{
			return new FilesystemDirectory(Path.Combine(FullPath, directoryName));
		}

		public override VirtualFile GetFile(string filename)
		{
			return new FilesystemFile(new FileInfo(Path.Combine(FullPath, filename)));
		}

		public override IEnumerable<VirtualFile> GetFiles(string searchPattern, SearchOption searchOption)
		{
			FileInfo[] files = dirInfo.GetFiles(searchPattern, searchOption);
			foreach (FileInfo fileInfo in files)
			{
				yield return new FilesystemFile(fileInfo);
			}
		}
	}
}
