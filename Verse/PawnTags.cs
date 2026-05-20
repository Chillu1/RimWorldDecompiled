using System.Collections.Generic;

namespace Verse
{
	public class PawnTags : IExposable
	{
		public List<string> tags;

		public bool Contains(string tag)
		{
			return tags.Contains(tag);
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref tags, "tags", LookMode.Value);
		}
	}
}
