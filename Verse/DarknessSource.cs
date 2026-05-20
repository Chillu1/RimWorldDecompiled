using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class DarknessSource : SimpleBoolPathFinderDataSource
{
	private bool wasActive;

	private readonly HashSet<IntVec3> dirtyCells = new HashSet<IntVec3>();

	public DarknessSource(Map map)
		: base(map)
	{
		map.events.GlowChanged += NotifyGlowChanged;
	}

	private void NotifyGlowChanged(IntVec3 cell)
	{
		if (map.GameConditionManager.GetActiveCondition<GameCondition_UnnaturalDarkness>() != null)
		{
			dirtyCells.Add(cell);
		}
	}

	public override void ComputeAll(IEnumerable<PathRequest> _)
	{
		data.Clear();
		if (ModsConfig.AnomalyActive && GameCondition_UnnaturalDarkness.UnnaturalDarknessOnMap(map))
		{
			wasActive = true;
			for (int i = 0; i < cellCount; i++)
			{
				data.Set(i, GameCondition_UnnaturalDarkness.UnnaturalDarknessAt(map.cellIndices[i], map));
			}
		}
	}

	public override bool UpdateIncrementally(IEnumerable<PathRequest> requests, List<IntVec3> cellDeltas)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		bool flag = GameCondition_UnnaturalDarkness.UnnaturalDarknessOnMap(map);
		bool result = false;
		if (flag && dirtyCells.Any())
		{
			foreach (IntVec3 dirtyCell in dirtyCells)
			{
				data.Set(map.cellIndices[dirtyCell], GameCondition_UnnaturalDarkness.UnnaturalDarknessAt(dirtyCell, map));
			}
			result = true;
			dirtyCells.Clear();
		}
		if (!flag && !wasActive)
		{
			return false;
		}
		if (flag && wasActive)
		{
			return result;
		}
		if (!flag)
		{
			data.Clear();
		}
		else
		{
			for (int i = 0; i < cellCount; i++)
			{
				data.Set(i, GameCondition_UnnaturalDarkness.UnnaturalDarknessAt(map.cellIndices[i], map));
			}
		}
		wasActive = flag;
		return true;
	}
}
