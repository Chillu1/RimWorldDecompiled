using System.IO;

namespace RimWorld.IO
{
	public abstract class VirtualFile
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

		public abstract long Length
		{
			get;
		}

		public abstract Stream CreateReadStream();

		public abstract string ReadAllText();

		public abstract string[] ReadAllLines();

		public abstract byte[] ReadAllBytes();
	}
}
