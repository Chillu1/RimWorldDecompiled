using System.IO;

namespace RimWorld.IO
{
	internal class FilesystemFile : VirtualFile
	{
		private FileInfo fileInfo;

		public override string Name => fileInfo.Name;

		public override string FullPath => fileInfo.FullName;

		public override bool Exists => fileInfo.Exists;

		public override long Length => fileInfo.Length;

		public FilesystemFile(FileInfo fileInfo)
		{
			this.fileInfo = fileInfo;
		}

		public override Stream CreateReadStream()
		{
			return fileInfo.OpenRead();
		}

		public override byte[] ReadAllBytes()
		{
			return File.ReadAllBytes(fileInfo.FullName);
		}

		public override string[] ReadAllLines()
		{
			return File.ReadAllLines(fileInfo.FullName);
		}

		public override string ReadAllText()
		{
			return File.ReadAllText(fileInfo.FullName);
		}

		public static implicit operator FilesystemFile(FileInfo fileInfo)
		{
			return new FilesystemFile(fileInfo);
		}

		public override string ToString()
		{
			return $"FilesystemFile [{FullPath}], Length {fileInfo.Length.ToString()}";
		}
	}
}
