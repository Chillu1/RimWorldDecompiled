using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PrefabDef : Def
{
	internal List<PrefabThingData> things = new List<PrefabThingData>();

	internal List<SubPrefabData> prefabs = new List<SubPrefabData>();

	internal List<PrefabTerrainData> terrain = new List<PrefabTerrainData>();

	public IntVec2 size;

	public RotEnum rotations = RotEnum.All;

	public bool edgeOnly;

	public IEnumerable<(PrefabThingData data, IntVec3 cell)> GetThings()
	{
		foreach (PrefabThingData data in things)
		{
			bool flag = false;
			if (!data.rects.NullOrEmpty())
			{
				foreach (CellRect rect in data.rects)
				{
					foreach (IntVec3 cell in rect.Cells)
					{
						yield return (data: data, cell: cell);
						flag = true;
					}
				}
			}
			if (!data.positions.NullOrEmpty())
			{
				foreach (IntVec3 position in data.positions)
				{
					yield return (data: data, cell: position);
					flag = true;
				}
			}
			if (!flag)
			{
				yield return (data: data, cell: data.position);
			}
		}
	}

	public IEnumerable<(SubPrefabData data, IntVec3 cell)> GetPrefabs()
	{
		foreach (SubPrefabData data in prefabs)
		{
			if (!data.positions.NullOrEmpty())
			{
				foreach (IntVec3 position in data.positions)
				{
					yield return (data: data, cell: position);
				}
			}
			else
			{
				yield return (data: data, cell: data.position);
			}
		}
	}

	public IEnumerable<(PrefabTerrainData data, IntVec3 cell)> GetTerrain()
	{
		foreach (PrefabTerrainData data in terrain)
		{
			if (data.rects.NullOrEmpty())
			{
				continue;
			}
			foreach (CellRect rect in data.rects)
			{
				foreach (IntVec3 cell in rect.Cells)
				{
					yield return (data: data, cell: cell);
				}
			}
		}
	}
}
