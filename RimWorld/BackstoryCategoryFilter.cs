using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class BackstoryCategoryFilter
	{
		public List<string> categories;

		public List<string> exclude;

		public float commonality = 1f;

		public bool Matches(PawnBio bio)
		{
			if (exclude != null && exclude.Where((string e) => bio.adulthood.spawnCategories.Contains(e) || bio.childhood.spawnCategories.Contains(e)).Any())
			{
				return false;
			}
			if (categories != null)
			{
				return categories.Where((string c) => bio.adulthood.spawnCategories.Contains(c) || bio.childhood.spawnCategories.Contains(c)).Any();
			}
			return true;
		}

		public bool Matches(Backstory backstory)
		{
			if (exclude != null && backstory.spawnCategories.Any((string e) => exclude.Contains(e)))
			{
				return false;
			}
			if (categories != null)
			{
				return backstory.spawnCategories.Any((string c) => categories.Contains(c));
			}
			return true;
		}
	}
}
