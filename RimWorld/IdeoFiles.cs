using System.Collections.Generic;
using System.IO;
using Verse;

namespace RimWorld
{
	public static class IdeoFiles
	{
		private static List<Ideo> ideosLocal = new List<Ideo>();

		public static IEnumerable<Ideo> AllIdeosLocal => ideosLocal;

		public static void RecacheData()
		{
			ideosLocal.Clear();
			foreach (FileInfo allCustomIdeoFile in GenFilePaths.AllCustomIdeoFiles)
			{
				if (GameDataSaveLoader.TryLoadIdeo(allCustomIdeoFile.FullName, out var ideo))
				{
					ideosLocal.Add(ideo);
				}
			}
		}
	}
}
