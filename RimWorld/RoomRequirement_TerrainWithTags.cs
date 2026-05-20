using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRequirement_TerrainWithTags : RoomRequirement
{
	public List<string> tags;

	private HashSet<string> tagsSet;

	private HashSet<string> TagsSet => tagsSet ?? (tagsSet = new HashSet<string>(tags));

	public override bool Met(Room r, Pawn p = null)
	{
		Map map = r.Map;
		foreach (IntVec3 cell in r.Cells)
		{
			List<string> list = cell.GetTerrain(map).tags;
			if (list.NullOrEmpty())
			{
				return false;
			}
			bool flag = false;
			foreach (string item in list)
			{
				if (TagsSet.Contains(item))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public override bool SameOrSubsetOf(RoomRequirement other)
	{
		if (!base.SameOrSubsetOf(other))
		{
			return false;
		}
		RoomRequirement_TerrainWithTags roomRequirement_TerrainWithTags = (RoomRequirement_TerrainWithTags)other;
		foreach (string tag in tags)
		{
			if (!roomRequirement_TerrainWithTags.tags.Contains(tag))
			{
				return false;
			}
		}
		return true;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (string.IsNullOrEmpty(labelKey))
		{
			yield return "does not define a label key";
		}
		if (tags.NullOrEmpty())
		{
			yield return "tags are null or empty";
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref tags, "tags", LookMode.Undefined);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			tagsSet = new HashSet<string>(tags);
		}
	}
}
