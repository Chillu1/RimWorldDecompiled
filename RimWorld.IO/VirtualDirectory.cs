using System.Collections.Generic;
using System.IO;

namespace RimWorld.IO
{
	public abstract class VirtualDirectory
	{
		public abstract string Name
		{
			get;
		}

		public abstract string FullPath
		{
			get;
		}

		public abstract bool Exists
		{
			get;
		}

		public abstract VirtualDirectory GetDirectory(string directoryName);

		public abstract VirtualFile GetFile(string filename);

		public abstract IEnumerable<VirtualFile> GetFiles(string searchPattern, SearchOption searchOption);

		public abstract IEnumerable<VirtualDirectory> GetDirectories(string searchPattern, SearchOption searchOption);

		public string ReadAllText(string filename)
		{
			return GetFile(filename).ReadAllText();
		}

		public bool FileExists(string filename)
		{
			return GetFile(filename).Exists;
		}

		public override string ToString()
		{
			return FullPath;
		}
	}
}
