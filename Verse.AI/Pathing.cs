using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class Pathing : IDisposable
{
	public class DisableDirtyingScope : IDisposable
	{
		private readonly Pathing pathing;

		public DisableDirtyingScope(Pathing pathing)
		{
			this.pathing = pathing;
			pathing.DisableIncrementalDirtying();
		}

		public void Dispose()
		{
			pathing.ReEnableIncrementalDirtying();
		}
	}

	private readonly Map map;

	private readonly Dictionary<PathGridDef, PathingContext> contexts = new Dictionary<PathGridDef, PathingContext>();

	private bool incrementalDirtyingDisabled;

	private readonly PathingContext normal;

	private readonly PathingContext fenceBlocked;

	private readonly PathingContext flying;

	public PathingContext Normal => normal;

	public PathingContext FenceBlocked => fenceBlocked;

	public PathingContext Flying => flying;

	public bool IncrementalDirtyingDisabled => incrementalDirtyingDisabled;

	public Pathing(Map map)
	{
		this.map = map;
		foreach (PathGridDef item in DefDatabase<PathGridDef>.AllDefsListForReading)
		{
			if (!(item.workerType == null))
			{
				PathGrid pathGrid = (PathGrid)Activator.CreateInstance(item.workerType, map, item);
				PathingContext pathingContext = (contexts[item] = new PathingContext(map, pathGrid));
				PathingContext pathingContext3 = pathingContext;
				if (item == PathGridDefOf.Normal)
				{
					normal = pathingContext3;
				}
				else if (item == PathGridDefOf.FenceBlocked)
				{
					fenceBlocked = pathingContext3;
				}
				else if (item == PathGridDefOf.Flying)
				{
					flying = pathingContext3;
				}
			}
		}
	}

	public PathingContext Get(PathGridDef def)
	{
		return contexts[def];
	}

	public PathingContext For(TraverseParms parms)
	{
		if (parms.fenceBlocked && !parms.canBashFences)
		{
			return fenceBlocked;
		}
		return normal;
	}

	public PathingContext For(Pawn pawn)
	{
		if (pawn == null)
		{
			return normal;
		}
		return pawn.GetPathContext(this) ?? normal;
	}

	public void RecalculateAllPerceivedPathCosts()
	{
		foreach (KeyValuePair<PathGridDef, PathingContext> context in contexts)
		{
			context.Deconstruct(out var _, out var value);
			value.pathGrid.RecalculateAllPerceivedPathCosts();
		}
	}

	public void RecalculatePerceivedPathCostUnderThing(Thing thing)
	{
		map.pathFinder.MapData.Notify_CellDelta(thing.OccupiedRect());
		ThingDef def = thing.def;
		if (def.pathCost == 0 && def.passability != Traversability.Impassable)
		{
			return;
		}
		if (thing.def.size == IntVec2.One)
		{
			RecalculatePerceivedPathCostAt(thing.Position);
			return;
		}
		CellRect cellRect = thing.OccupiedRect();
		for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
		{
			for (int j = cellRect.minX; j <= cellRect.maxX; j++)
			{
				IntVec3 c = new IntVec3(j, 0, i);
				RecalculatePerceivedPathCostAt(c);
			}
		}
	}

	public void RecalculatePerceivedPathCostAt(IntVec3 c)
	{
		bool haveNotified = false;
		foreach (KeyValuePair<PathGridDef, PathingContext> context in contexts)
		{
			context.Deconstruct(out var _, out var value);
			value.pathGrid.RecalculatePerceivedPathCostAt(c, ref haveNotified);
		}
		map.events.Notify_PathCostRecalculated(c);
	}

	public DisableDirtyingScope DisableIncrementalScope()
	{
		return new DisableDirtyingScope(this);
	}

	public void DisableIncrementalDirtying()
	{
		foreach (KeyValuePair<PathGridDef, PathingContext> context in contexts)
		{
			context.Deconstruct(out var _, out var value);
			value.pathGrid.DisableIncrementalDirtying();
		}
		incrementalDirtyingDisabled = true;
	}

	public void ReEnableIncrementalDirtying()
	{
		foreach (KeyValuePair<PathGridDef, PathingContext> context in contexts)
		{
			context.Deconstruct(out var _, out var value);
			value.pathGrid.ReEnableIncrementalDirtying();
		}
		incrementalDirtyingDisabled = false;
	}

	public void Dispose()
	{
		foreach (KeyValuePair<PathGridDef, PathingContext> context in contexts)
		{
			context.Deconstruct(out var _, out var value);
			value.Dispose();
		}
	}
}
