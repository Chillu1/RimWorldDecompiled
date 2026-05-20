using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class BackstoryCategoryFilter
{
	public List<string> categories;

	public List<string> exclude;

	public List<string> categoriesChildhood;

	public List<string> excludeChildhood;

	public List<string> categoriesAdulthood;

	public List<string> excludeAdulthood;

	public float commonality = 1f;

	public bool Matches(PawnBio bio)
	{
		if (exclude != null && exclude.Any((string e) => bio.adulthood.spawnCategories.Contains(e) || bio.childhood.spawnCategories.Contains(e)))
		{
			return false;
		}
		if (excludeChildhood != null && bio.childhood.spawnCategories.Any((string e) => excludeChildhood.Contains(e)))
		{
			return false;
		}
		if (excludeAdulthood != null && bio.adulthood.spawnCategories.Any((string e) => excludeAdulthood.Contains(e)))
		{
			return false;
		}
		bool flag = true;
		if (categoriesChildhood != null)
		{
			flag &= bio.childhood.spawnCategories.Any((string e) => categoriesChildhood.Contains(e));
		}
		if (categoriesAdulthood != null)
		{
			flag &= bio.adulthood.spawnCategories.Any((string e) => categoriesAdulthood.Contains(e));
		}
		if (categoriesChildhood != null || categoriesAdulthood != null)
		{
			return flag;
		}
		if (categories != null)
		{
			return categories.Any((string c) => bio.adulthood.spawnCategories.Contains(c) || bio.childhood.spawnCategories.Contains(c));
		}
		return true;
	}

	public bool Matches(BackstoryDef backstory)
	{
		if (exclude != null && backstory.spawnCategories.Any((string e) => exclude.Contains(e)))
		{
			return false;
		}
		if (!excludeChildhood.NullOrEmpty() && backstory.slot == BackstorySlot.Childhood && backstory.spawnCategories.Any((string e) => excludeChildhood.Contains(e)))
		{
			return false;
		}
		if (!excludeAdulthood.NullOrEmpty() && backstory.slot == BackstorySlot.Adulthood && backstory.spawnCategories.Any((string e) => excludeAdulthood.Contains(e)))
		{
			return false;
		}
		if (!categoriesChildhood.NullOrEmpty() && backstory.slot == BackstorySlot.Childhood && !backstory.spawnCategories.Any((string c) => categoriesChildhood.Contains(c)))
		{
			return false;
		}
		if (!categoriesAdulthood.NullOrEmpty() && backstory.slot == BackstorySlot.Adulthood && !backstory.spawnCategories.Any((string c) => categoriesAdulthood.Contains(c)))
		{
			return false;
		}
		if (backstory.requiresSpawnCategory)
		{
			if (categories.NullOrEmpty() && categoriesChildhood.NullOrEmpty() && backstory.slot == BackstorySlot.Childhood)
			{
				return false;
			}
			if (categories.NullOrEmpty() && categoriesAdulthood.NullOrEmpty() && backstory.slot == BackstorySlot.Adulthood)
			{
				return false;
			}
		}
		if (categories != null)
		{
			return backstory.spawnCategories.Any((string c) => categories.Contains(c));
		}
		return true;
	}
}
