using System;
using RimWorld;
using Unity.Collections;
using Verse.AI;
using Verse.AI.Group;

namespace Verse;

public class PathRequest : IDisposable
{
	public interface IPathGridCustomizer
	{
		NativeArray<ushort> GetOffsetGrid();
	}

	public Map map;

	private IntVec3 start;

	private LocalTargetInfo dest;

	private IntVec3? exactDestination;

	private TraverseParms traverseParms;

	private PathFinderCostTuning tuning;

	private PathEndMode endMode;

	private int tickCreated;

	private int tickStart;

	private int tickDeadline;

	public Pawn pawn;

	public IPathGridCustomizer customizer;

	public Area area;

	private bool cancelled;

	private PawnPath path;

	private bool? found;

	public IntVec3 Start => start;

	public LocalTargetInfo Target => dest;

	public IntVec3 ExactDestination => exactDestination ?? dest.Cell;

	public TraverseParms TraverseParms => traverseParms;

	public PathFinderCostTuning Tuning => tuning;

	public PathEndMode EndMode => endMode;

	public int TickStart => tickStart;

	public bool ResultIsReady => found.HasValue;

	public bool Cancelled => cancelled;

	public bool? Found => found;

	public Faction RequesterFaction
	{
		get
		{
			if (pawn == null)
			{
				return null;
			}
			Pawn_GuestTracker guest = pawn.guest;
			if (guest == null)
			{
				return pawn.Faction;
			}
			return guest.HostFaction ?? pawn.Faction;
		}
	}

	public Lord RequesterLord
	{
		get
		{
			if (pawn == null)
			{
				return null;
			}
			return pawn.lord;
		}
	}

	public PathRequest(Map map, IntVec3 start, LocalTargetInfo dest, IntVec3? exactDestination, TraverseParms traverseParms, PathFinderCostTuning tuning, PathEndMode peMode, Pawn pawn, int tickCreated, int tickStart, int tickDeadline, IPathGridCustomizer customizer = null)
	{
		this.map = map;
		this.start = start;
		this.dest = dest;
		this.exactDestination = exactDestination;
		this.traverseParms = traverseParms;
		this.tuning = tuning;
		endMode = peMode;
		this.pawn = pawn;
		this.tickCreated = tickCreated;
		this.tickStart = tickStart;
		this.tickDeadline = tickDeadline;
		this.customizer = customizer;
		area = PathUtility.GetAllowedArea(pawn);
	}

	public bool Validate()
	{
		bool num = ValidateInt();
		if (!num)
		{
			found = false;
		}
		return num;
	}

	private bool ValidateInt()
	{
		Pawn pawn = traverseParms.pawn;
		if (pawn != null && pawn.Map != map)
		{
			Log.Error("Tried to FindPath for pawn which is spawned in another map. Its map PathFinder should have been used, not this one. pawn=" + pawn.ToStringSafe() + " pawn.Map=" + pawn.Map.ToStringSafe() + " map=" + map.ToStringSafe());
			return false;
		}
		if (!start.IsValid)
		{
			Log.Error("Tried to FindPath with invalid start " + start.ToStringSafe() + ", pawn= " + pawn.ToStringSafe());
			return false;
		}
		if (!dest.IsValid)
		{
			return false;
		}
		using (ProfilerBlock.Scope("Reachability"))
		{
			if (traverseParms.mode == TraverseMode.ByPawn)
			{
				if (!pawn.CanReach(dest, endMode, Danger.Deadly, traverseParms.canBashDoors, traverseParms.canBashFences, traverseParms.mode))
				{
					return false;
				}
			}
			else if (!map.reachability.CanReach(start, dest, endMode, traverseParms))
			{
				return false;
			}
		}
		return true;
	}

	public void Resolve(PawnPath p)
	{
		if (p != null && p.Found)
		{
			path = p;
			found = true;
		}
		else
		{
			p?.Dispose();
			found = false;
		}
	}

	public bool TryGetPath(out PawnPath outPath)
	{
		if (ResultIsReady)
		{
			outPath = path;
			return true;
		}
		outPath = null;
		return false;
	}

	public void ClaimCalculatedPath()
	{
		path = null;
	}

	public override string ToString()
	{
		return string.Format("({0} -> {1}{2}, {3}, for {4})", start, dest, dest.HasThing ? $" @ {dest.Cell}" : "", endMode, pawn);
	}

	public void Dispose()
	{
		cancelled = true;
		if (path != null)
		{
			path.Dispose();
			path = null;
		}
	}
}
