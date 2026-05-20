using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ModList : IExposable
	{
		public string fileName;

		public List<string> ids;

		public List<string> names;

		public void ExposeData()
		{
			Scribe_Collections.Look(ref ids, "ids", LookMode.Undefined);
			Scribe_Collections.Look(ref names, "names", LookMode.Undefined);
		}
	}
}
