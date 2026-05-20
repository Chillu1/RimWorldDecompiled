using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public abstract class GenStep_LargeRuins : GenStep_BaseRuins
{
	protected abstract IntRange RuinsMinMaxRange { get; }

	protected virtual IntVec2 MinSize { get; } = new IntVec2(40, 40);

	protected virtual IntVec2 MaxSize { get; } = new IntVec2(60, 60);

	protected override IEnumerable<CellRect> GetRectOrder(IEnumerable<CellRect> rects, Map map)
	{
		return rects.OrderByDescending((CellRect r) => (float)r.Area - (map.Center - r.CenterCell).Magnitude / 2f).Take(RuinsMinMaxRange.TrueMax);
	}

	protected override IEnumerable<CellRect> GetRects(CellRect area, Map map)
	{
		_ = MapGenerator.Elevation;
		List<CellRect> largestClearRects = MapGenUtility.GetLargestClearRects(map, MinSize, MaxSize, UseUsedRects, -1f, 0.7f, MinAffordance);
		if (largestClearRects.Count < RuinsMinMaxRange.TrueMin)
		{
			largestClearRects = MapGenUtility.GetLargestClearRects(map, MinSize, MaxSize, UseUsedRects, -1f, 1f);
			if (largestClearRects.Count < RuinsMinMaxRange.TrueMin)
			{
				largestClearRects = MapGenUtility.GetLargestClearRects(map, MinSize, MaxSize, UseUsedRects, -1f, 1f, null, checkWaterRoads: false);
			}
			while ((float)largestClearRects.Count > RuinsMinMaxRange.Average)
			{
				largestClearRects.RemoveAt(largestClearRects.Count - 1);
			}
		}
		return largestClearRects;
	}
}
