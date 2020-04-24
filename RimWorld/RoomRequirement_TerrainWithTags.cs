using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRequirement_TerrainWithTags : RoomRequirement
	{
		public List<string> tags;

		public override bool Met(Room r, Pawn p = null)
		{
			foreach (IntVec3 cell in r.Cells)
			{
				List<string> list = cell.GetTerrain(r.Map).tags;
				if (list.NullOrEmpty())
				{
					return false;
				}
				bool flag = false;
				foreach (string item in list)
				{
					if (tags.Contains(item))
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
	}
}
