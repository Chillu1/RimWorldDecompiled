using System;
using System.Collections.Generic;
using System.Text;
using Gilzoide.ManagedJobs;
using LudeonTK;
using RimWorld;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Verse.AI;

namespace Verse;

public class PathFinder : IDisposable
{
	private struct ScheduledPathJob
	{
		public PathRequest request;

		public JobHandle handle;

		public PathUniqueState state;

		public PathJobOutput output;
	}

	private struct ScheduledGridJob
	{
		public int computed;

		public int lastAccessed;

		public JobHandle handle;

		public GridJobOutput output;
	}

	public readonly struct UnmanagedGridTraverseParams : IEquatable<UnmanagedGridTraverseParams>
	{
		public readonly TraverseMode mode;

		public readonly bool canBashFences;

		public readonly bool fenceBlocked;

		public readonly bool avoidPersistentDanger;

		public readonly bool avoidDarknessDanger;

		public readonly bool avoidFog;

		public readonly CellRect targetBuildable;

		public UnmanagedGridTraverseParams(TraverseMode mode, bool canBashFences, bool fenceBlocked, bool avoidPersistentDanger, bool avoidDarknessDanger, bool avoidFog, CellRect targetBuildable)
		{
			this.mode = mode;
			this.canBashFences = canBashFences;
			this.fenceBlocked = fenceBlocked;
			this.avoidPersistentDanger = avoidPersistentDanger;
			this.avoidDarknessDanger = avoidDarknessDanger;
			this.avoidFog = avoidFog;
			this.targetBuildable = targetBuildable;
		}

		public static UnmanagedGridTraverseParams For(TraverseParms parms)
		{
			return new UnmanagedGridTraverseParams(parms.mode, parms.canBashFences, parms.fenceBlocked, parms.avoidPersistentDanger, parms.avoidDarknessDanger, parms.avoidFog, parms.targetBuildable);
		}

		public bool Equals(UnmanagedGridTraverseParams other)
		{
			if (mode == other.mode && canBashFences == other.canBashFences && fenceBlocked == other.fenceBlocked && avoidPersistentDanger == other.avoidPersistentDanger && avoidDarknessDanger == other.avoidDarknessDanger && avoidFog == other.avoidFog)
			{
				return targetBuildable == other.targetBuildable;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is UnmanagedGridTraverseParams other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int)mode, canBashFences, fenceBlocked, avoidPersistentDanger, avoidDarknessDanger, avoidFog, targetBuildable);
		}
	}

	public struct MapGridRequest : IEquatable<MapGridRequest>
	{
		public UnmanagedGridTraverseParams traverseParams;

		public PathFinderCostTuning tuning;

		public PathRequest.IPathGridCustomizer customizer;

		public Area allowedArea;

		public AvoidGrid avoidGrid;

		public static MapGridRequest For(PathRequest request)
		{
			AvoidGrid grid = null;
			if (request.pawn != null)
			{
				request.pawn.TryGetAvoidGrid(out grid);
			}
			return new MapGridRequest
			{
				traverseParams = UnmanagedGridTraverseParams.For(request.TraverseParms),
				tuning = request.Tuning,
				customizer = request.customizer,
				allowedArea = request.area,
				avoidGrid = grid
			};
		}

		public bool Equals(MapGridRequest other)
		{
			if (traverseParams.Equals(other.traverseParams) && tuning.Equals(other.tuning) && object.Equals(customizer, other.customizer) && object.Equals(allowedArea, other.allowedArea))
			{
				return object.Equals(avoidGrid, other.avoidGrid);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is MapGridRequest other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(traverseParams, tuning, customizer, allowedArea, avoidGrid);
		}

		public override string ToString()
		{
			return string.Format("{0}{1}{2}{3}{4}", traverseParams.mode, traverseParams.fenceBlocked ? ", fence blocked" : "", traverseParams.avoidFog ? ", avoid fog" : "", (allowedArea != null) ? (", area: " + allowedArea.RenamableLabel) : "", (avoidGrid != null) ? ", avoid grid: true" : "");
		}
	}

	private struct JobBatchState : IDisposable
	{
		public NativeArray<PathFinderJob.CalcNode> calcGrid;

		public NativePriorityQueue<int, float, FloatMinComparer> frontier;

		public static JobBatchState AllocateNew(int cells)
		{
			JobBatchState result = default(JobBatchState);
			result.calcGrid = new NativeArray<PathFinderJob.CalcNode>(cells, Allocator.Persistent);
			result.frontier = new NativePriorityQueue<int, float, FloatMinComparer>(cells, default(FloatMinComparer), Allocator.Persistent);
			return result;
		}

		public void Dispose()
		{
			calcGrid.Dispose();
			frontier.Dispose();
		}
	}

	public struct GridJobOutput : IDisposable
	{
		public NativeArray<int> grid;

		public void Initalize(int cells)
		{
			grid = new NativeArray<int>(cells, Allocator.Persistent);
		}

		public void Dispose()
		{
			grid.Dispose();
		}
	}

	private struct PathUniqueState : IDisposable
	{
		public NativeList<int> excludedDestIndices;

		public NativeArray<ushort> providers;

		public NativeArray<bool> blocked;

		public void Initialize(int cells)
		{
			excludedDestIndices = new NativeList<int>(4, Allocator.Persistent);
			providers = new NativeArray<ushort>(cells, Allocator.Persistent);
			blocked = new NativeArray<bool>(cells, Allocator.Persistent);
		}

		public void Dispose()
		{
			excludedDestIndices.Dispose();
			providers.Dispose();
			blocked.Dispose();
		}
	}

	private struct PathJobOutput : IDisposable
	{
		public NativeList<IntVec3> path;

		public NativeReference<PathFinderJob.ResultData> data;

		public void Initialize(int cells)
		{
			path = new NativeList<IntVec3>(cells, Allocator.Persistent);
			data = new NativeReference<PathFinderJob.ResultData>(default(PathFinderJob.ResultData), Allocator.Persistent);
		}

		public void Dispose()
		{
			path.Dispose();
			data.Dispose();
		}
	}

	private readonly Map map;

	private readonly PathFinderMapData mapData;

	private readonly List<JobBatchState> batches = new List<JobBatchState>();

	private JobBatchState immediateBatch;

	private readonly List<PathRequest> workQueue = new List<PathRequest>();

	private readonly List<ScheduledPathJob> scheduledPathJobs = new List<ScheduledPathJob>();

	private readonly List<ScheduledGridJob> scheduledGridJobs = new List<ScheduledGridJob>();

	private readonly Queue<PathJobOutput> pathJobOutputs = new Queue<PathJobOutput>();

	private readonly Queue<PathUniqueState> pathUniqueStates = new Queue<PathUniqueState>();

	private readonly Queue<GridJobOutput> gridJobOutputs = new Queue<GridJobOutput>();

	private readonly Dictionary<MapGridRequest, ScheduledGridJob> gridJobLookup = new Dictionary<MapGridRequest, ScheduledGridJob>();

	private readonly List<PathRequest> tmpCurrentWork = new List<PathRequest>();

	private bool disposed;

	public const int DefaultMoveTicksCardinal = 13;

	public const int DefaultMoveTicksDiagonal = 18;

	public const float DiagonalMoveFactor = 1.41421f;

	private const float HeuristicScaleAnimal = 1.75f;

	private static readonly SimpleCurve HeuristicScaleAI = new SimpleCurve
	{
		new CurvePoint(40f, 1f),
		new CurvePoint(120f, 2f)
	};

	private readonly List<IPathFindCostProvider> cachedProviders = new List<IPathFindCostProvider>();

	private readonly List<Thing> cachedDoors = new List<Thing>();

	private readonly List<Pawn> cachedPawns = new List<Pawn>();

	private int cachedTick;

	public PathFinderMapData MapData => mapData;

	public PathFinder(Map map)
	{
		this.map = map;
		mapData = new PathFinderMapData(map);
		int numGridCells = map.cellIndices.NumGridCells;
		int maxJobWorkerCount = UnityData.MaxJobWorkerCount;
		for (int i = 0; i < maxJobWorkerCount; i++)
		{
			batches.Add(JobBatchState.AllocateNew(numGridCells));
		}
		immediateBatch = JobBatchState.AllocateNew(numGridCells);
	}

	public void PathFinderTick()
	{
		if (disposed)
		{
			return;
		}
		ForceCompleteScheduledJobs();
		FinalizeRecyclePathJobData();
		ComputeWorkThisTick();
		if (tmpCurrentWork.Count != 0)
		{
			if (mapData.GatherData(tmpCurrentWork))
			{
				RecycleGridJobData();
			}
			JobHandle lastGridHandle = ScheduleGridJobs();
			ScheduleBatchedPathJobs(lastGridHandle);
		}
	}

	private void ComputeWorkThisTick()
	{
		if (tmpCurrentWork.Count != 0)
		{
			Log.Error($"Pathfinder current work queue is not empty, count: {tmpCurrentWork.Count}");
		}
		tmpCurrentWork.Clear();
		int ticksGame = GenTicks.TicksGame;
		for (int num = workQueue.Count - 1; num >= 0; num--)
		{
			PathRequest pathRequest = workQueue[num];
			if (pathRequest.Cancelled)
			{
				workQueue.RemoveAt(num);
			}
			else if (!pathRequest.Validate())
			{
				workQueue.RemoveAt(num);
			}
			else if (pathRequest.TickStart <= ticksGame)
			{
				tmpCurrentWork.Add(pathRequest);
				workQueue.RemoveAt(num);
			}
		}
	}

	private void FinalizeRecyclePathJobData()
	{
		foreach (ScheduledPathJob scheduledPathJob in scheduledPathJobs)
		{
			ScheduledPathJob current = scheduledPathJob;
			if (!current.request.Cancelled && current.output.path.Length == 0)
			{
				Log.WarningOnce($"Resolved path returned no nodes, request: {current.request}", HashCode.Combine(current.request, 163511));
				current.request.Resolve(null);
			}
			else if (!current.request.Cancelled && (current.request.pawn == null || current.request.pawn.Spawned))
			{
				PawnPath p = EmitPath(in current.output);
				current.request.Resolve(p);
			}
			pathUniqueStates.Enqueue(current.state);
			pathJobOutputs.Enqueue(current.output);
		}
		scheduledPathJobs.Clear();
	}

	private void RecycleGridJobData()
	{
		foreach (ScheduledGridJob scheduledGridJob in scheduledGridJobs)
		{
			gridJobOutputs.Enqueue(scheduledGridJob.output);
		}
		scheduledGridJobs.Clear();
		gridJobLookup.Clear();
	}

	private void ScheduleBatchedPathJobs(JobHandle lastGridHandle = default(JobHandle))
	{
		int count = batches.Count;
		int count2 = tmpCurrentWork.Count;
		int result;
		int num = Math.DivRem(count2, count, out result);
		if (result > 0)
		{
			num++;
		}
		int i = 0;
		int num2 = 0;
		for (; i < count; i++)
		{
			JobHandle jobHandle = lastGridHandle;
			JobBatchState batchState = batches[i];
			int num3 = 0;
			while (num3 < num && num2 < count2)
			{
				PathRequest request = tmpCurrentWork.Pop();
				MapGridRequest gridRequest = MapGridRequest.For(request);
				ScheduledGridJob scheduledGridJob = gridJobLookup[gridRequest];
				PathFinderJob job = default(PathFinderJob);
				GetOrCreatePathJobUniqueState(out var state);
				GetOrCreatePathJobOutput(out var output);
				jobHandle = GetDoorBlockedJob(request, ref state).Schedule(jobHandle);
				ParameterizePathJob(ref job, ref gridRequest, ref output, ref state, ref batchState, ref scheduledGridJob, request);
				mapData.ParameterizePathJob(ref job);
				jobHandle = job.Schedule(jobHandle);
				scheduledPathJobs.Add(new ScheduledPathJob
				{
					request = request,
					handle = jobHandle,
					state = state,
					output = output
				});
				num3++;
				num2++;
			}
		}
		JobHandle.ScheduleBatchedJobs();
	}

	private ManagedJob GetDoorBlockedJob(PathRequest request, ref PathUniqueState jobState)
	{
		PathGridDoorsBlockedJob pathGridDoorsBlockedJob = new PathGridDoorsBlockedJob();
		ParameterizeDoorBlockedJob(pathGridDoorsBlockedJob, request, ref jobState);
		return new ManagedJob(pathGridDoorsBlockedJob);
	}

	private JobHandle ScheduleGridJobs()
	{
		using (ProfilerBlock.Scope("PathFinder.ScheduleGridJobs()"))
		{
			JobHandle jobHandle = default(JobHandle);
			foreach (PathRequest item in tmpCurrentWork)
			{
				MapGridRequest key = MapGridRequest.For(item);
				if (!gridJobLookup.ContainsKey(key))
				{
					jobHandle = ScheduleGridJob(item, jobHandle).handle;
				}
			}
			return jobHandle;
		}
	}

	private ScheduledGridJob ScheduleGridJob(PathRequest request, JobHandle handle = default(JobHandle))
	{
		MapGridRequest query = MapGridRequest.For(request);
		ScheduledGridJob scheduledGridJob = default(ScheduledGridJob);
		scheduledGridJob.lastAccessed = (scheduledGridJob.computed = GenTicks.TicksGame);
		GetOrCreateGridJobOutput(out scheduledGridJob.output);
		PathGridJob job = default(PathGridJob);
		mapData.ParameterizeGridJob(request, ref query, ref job, ref scheduledGridJob.output);
		int numGridCells = map.cellIndices.NumGridCells;
		int idealBatchCount = UnityData.GetIdealBatchCount(numGridCells);
		scheduledGridJob.handle = (handle.Equals(default(JobHandle)) ? IJobParallelForExtensions.Schedule(job, numGridCells, idealBatchCount) : IJobParallelForExtensions.Schedule(job, numGridCells, idealBatchCount, handle));
		scheduledGridJobs.Add(scheduledGridJob);
		return gridJobLookup[query] = scheduledGridJob;
	}

	private void ForceCompleteScheduledJobs()
	{
		int num = scheduledPathJobs.Count + scheduledGridJobs.Count;
		if (num > 0)
		{
			NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(num, Allocator.Persistent);
			for (int i = 0; i < scheduledPathJobs.Count; i++)
			{
				jobs[i] = scheduledPathJobs[i].handle;
			}
			for (int j = 0; j < scheduledGridJobs.Count; j++)
			{
				jobs[scheduledPathJobs.Count + j] = scheduledGridJobs[j].handle;
			}
			JobHandle.CompleteAll(jobs);
			jobs.Dispose();
		}
	}

	private PawnPath EmitPath(in PathJobOutput pathJobOutput)
	{
		NativeList<IntVec3> path = pathJobOutput.path;
		PathFinderJob.ResultData value = pathJobOutput.data.Value;
		PawnPath path2 = map.pawnPathPool.GetPath();
		path2.Initialize(path, value.pathCost);
		return path2;
	}

	private void GetOrCreateGridJobOutput(out GridJobOutput output)
	{
		if (!gridJobOutputs.TryDequeue(out output))
		{
			output.Initalize(map.cellIndices.NumGridCells);
		}
	}

	private void GetOrCreatePathJobUniqueState(out PathUniqueState state)
	{
		if (!pathUniqueStates.TryDequeue(out state))
		{
			state.Initialize(map.cellIndices.NumGridCells);
		}
	}

	private void GetOrCreatePathJobOutput(out PathJobOutput output)
	{
		if (!pathJobOutputs.TryDequeue(out output))
		{
			output.Initialize(map.cellIndices.NumGridCells);
		}
	}

	public PathRequest CreateRequest(IntVec3 start, LocalTargetInfo target, IntVec3? dest, Pawn pawn, PathFinderCostTuning? tuning, PathEndMode peMode = PathEndMode.OnCell, PathRequest.IPathGridCustomizer customizer = null)
	{
		ResolveCanBash(pawn, out var canBashDoors, out var canBashFences);
		bool canBashFences2 = canBashFences;
		return CreateRequest(start, target, dest, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, canBashDoors, alwaysUseAvoidGrid: false, canBashFences2), tuning, peMode, pawn, customizer);
	}

	public PathRequest CreateRequest(IntVec3 start, LocalTargetInfo target, IntVec3? dest, TraverseParms traverseParms, PathFinderCostTuning? mtuning, PathEndMode peMode = PathEndMode.OnCell, Pawn pawn = null, PathRequest.IPathGridCustomizer customizer = null)
	{
		int ticksGame = GenTicks.TicksGame;
		PathFinderCostTuning tuning = mtuning ?? PathFinderCostTuning.DefaultTuning;
		return new PathRequest(map, start, target, dest, traverseParms, tuning, peMode, pawn, ticksGame, ticksGame, ticksGame, customizer);
	}

	public void PushRequest(PathRequest request)
	{
		if (workQueue.Contains(request))
		{
			Log.Error("Tried to add the same PathRequest twice to PathFinder.");
		}
		else if (request.ResultIsReady)
		{
			Log.Error("Tried to add PathRequest to PathFinder but it already has its result calculated.");
		}
		else
		{
			workQueue.Add(request);
		}
	}

	private static float DetermineHeuristicStrength(Pawn pawn, IntVec3 start, LocalTargetInfo dest)
	{
		if (pawn == null)
		{
			return 1f;
		}
		if (pawn.RaceProps.Animal)
		{
			return 1.75f;
		}
		if (pawn.IsColonist)
		{
			if (GameCondition_UnnaturalDarkness.UnnaturalDarknessOnMap(pawn.Map) && GameCondition_UnnaturalDarkness.AffectedByDarkness(pawn))
			{
				return 1.5f;
			}
			return 1f;
		}
		float magnitude = (start - dest.Cell).Magnitude;
		return Mathf.RoundToInt(HeuristicScaleAI.Evaluate(magnitude));
	}

	public PawnPath FindPathNow(IntVec3 start, LocalTargetInfo target, Pawn pawn, PathFinderCostTuning? tuning = null, PathEndMode peMode = PathEndMode.OnCell)
	{
		ResolveCanBash(pawn, out var canBashDoors, out var canBashFences);
		bool canBashFences2 = canBashFences;
		return FindPathNow(start, target, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, canBashDoors, alwaysUseAvoidGrid: false, canBashFences2), tuning, peMode);
	}

	public PawnPath FindPathNow(IntVec3 start, LocalTargetInfo target, TraverseParms traverseParms, PathFinderCostTuning? tuning = null, PathEndMode peMode = PathEndMode.OnCell, PathRequest.IPathGridCustomizer customizer = null)
	{
		if (disposed)
		{
			return PawnPath.NotFound;
		}
		PathRequest pathRequest = CreateRequest(start, target, null, traverseParms, tuning, peMode, traverseParms.pawn, customizer);
		if (!pathRequest.Validate())
		{
			return PawnPath.NotFound;
		}
		ForceCompleteScheduledJobs();
		if (mapData.GatherData(new PathRequest[1] { pathRequest }))
		{
			RecycleGridJobData();
		}
		MapGridRequest gridRequest = MapGridRequest.For(pathRequest);
		if (!gridJobLookup.TryGetValue(gridRequest, out var value))
		{
			value = ScheduleGridJob(pathRequest);
		}
		PathFinderJob job = default(PathFinderJob);
		GetOrCreatePathJobUniqueState(out var state);
		GetOrCreatePathJobOutput(out var output);
		value.handle.Complete();
		GetDoorBlockedJob(pathRequest, ref state).Run();
		ParameterizePathJob(ref job, ref gridRequest, ref output, ref state, ref immediateBatch, ref value, pathRequest);
		mapData.ParameterizePathJob(ref job);
		job.Run();
		if (output.path.Length == 0)
		{
			pathUniqueStates.Enqueue(state);
			pathJobOutputs.Enqueue(output);
			pathRequest.Resolve(null);
			return PawnPath.NotFound;
		}
		PawnPath result = EmitPath(in output);
		pathUniqueStates.Enqueue(state);
		pathJobOutputs.Enqueue(output);
		return result;
	}

	private void ParameterizeDoorBlockedJob(PathGridDoorsBlockedJob job, PathRequest request, ref PathUniqueState state)
	{
		job.start = request.Start;
		job.dest = request.Target.Cell;
		job.map = map;
		job.pawn = request.pawn;
		job.traverseParams = request.TraverseParms;
		job.tuning = request.Tuning;
		EnsureDoorsPawnsCached();
		job.doors = cachedDoors;
		job.providers = cachedProviders;
		job.pawns = cachedPawns;
		job.providerCost = state.providers;
		job.blocked = state.blocked;
	}

	private void EnsureDoorsPawnsCached()
	{
		if (cachedTick == GenTicks.TicksGame)
		{
			return;
		}
		cachedTick = GenTicks.TicksGame;
		cachedProviders.Clear();
		cachedDoors.Clear();
		cachedPawns.Clear();
		foreach (Thing item in map.listerThings.ThingsInGroup(ThingRequestGroup.CostProvider))
		{
			cachedProviders.Add((IPathFindCostProvider)item);
		}
		cachedDoors.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.Door));
		cachedPawns.AddRange(map.mapPawns.AllPawnsSpawned);
	}

	private void ParameterizePathJob(ref PathFinderJob job, ref MapGridRequest gridRequest, ref PathJobOutput output, ref PathUniqueState uniqueState, ref JobBatchState batchState, ref ScheduledGridJob scheduledGridJob, PathRequest request)
	{
		using (ProfilerBlock.Scope("PathFinder.Parameterize"))
		{
			job.indices = map.cellIndices;
			MakeDestination(request.Target, request.EndMode, map.pathing.For(request.TraverseParms), out var rect, ref uniqueState.excludedDestIndices);
			job.grid = scheduledGridJob.output.grid.AsReadOnly();
			scheduledGridJob.lastAccessed = GenTicks.TicksGame;
			job.start = request.Start;
			job.destCell = request.Target.Cell;
			job.destRect = rect;
			job.heuristicStrength = DetermineHeuristicStrength(request.TraverseParms.pawn, request.Start, request.Target);
			job.traverseParams = gridRequest.traverseParams;
			if (request.TraverseParms.pawn != null)
			{
				job.moveTicksCardinal = Mathf.RoundToInt(request.TraverseParms.pawn.TicksPerMoveCardinal);
				job.moveTicksDiagonal = Mathf.RoundToInt(request.TraverseParms.pawn.TicksPerMoveDiagonal);
			}
			else
			{
				job.moveTicksCardinal = 18;
				job.moveTicksDiagonal = 18;
			}
			job.calcGrid = batchState.calcGrid;
			job.frontier = batchState.frontier;
			job.path = output.path;
			job.result = output.data;
			job.providerCost = uniqueState.providers.AsReadOnly();
			job.blocked = uniqueState.blocked.AsReadOnly();
			job.excludedRectIndices = uniqueState.excludedDestIndices;
		}
	}

	private static void MakeDestination(LocalTargetInfo target, PathEndMode peMode, PathingContext context, out CellRect rect, ref NativeList<int> excluded)
	{
		excluded.Clear();
		Map map = context.map;
		CellIndices cellIndices = map.cellIndices;
		rect = CalculateDestinationRect(target, peMode, context);
		if (peMode == PathEndMode.Touch && rect.Area != 1)
		{
			IntVec3 intVec = new IntVec3(1, 0, 0);
			IntVec3 intVec2 = new IntVec3(0, 0, 1);
			rect.GetInternalCorners(out var BL, out var TL, out var TR, out var BR);
			if (BL.InBounds(map) && !TouchPathEndModeUtility.IsCornerTouchAllowed_NewTemp(BL + intVec2, BL + intVec, context, target.Thing))
			{
				excluded.Add(cellIndices.CellToIndex(BL));
			}
			if (TR.InBounds(map) && !TouchPathEndModeUtility.IsCornerTouchAllowed_NewTemp(TR - intVec2, TR - intVec, context, target.Thing))
			{
				excluded.Add(cellIndices.CellToIndex(TR));
			}
			if (TL.InBounds(map) && !TouchPathEndModeUtility.IsCornerTouchAllowed_NewTemp(TL - intVec2, TL + intVec, context, target.Thing))
			{
				excluded.Add(cellIndices.CellToIndex(TL));
			}
			if (BR.InBounds(map) && !TouchPathEndModeUtility.IsCornerTouchAllowed_NewTemp(BR + intVec2, BR - intVec, context, target.Thing))
			{
				excluded.Add(cellIndices.CellToIndex(BR));
			}
		}
		rect = rect.ClipInsideMap(map);
	}

	private static CellRect CalculateDestinationRect(LocalTargetInfo dest, PathEndMode peMode, PathingContext context)
	{
		CellRect result = ((dest.HasThing && peMode != PathEndMode.OnCell) ? dest.Thing.OccupiedRect() : CellRect.SingleCell(dest.Cell));
		if (peMode == PathEndMode.Touch)
		{
			result = result.ExpandedBy(1);
		}
		return result;
	}

	private void ResolveCanBash(Pawn pawn, out bool canBashDoors, out bool canBashFences)
	{
		canBashDoors = pawn != null && pawn.CurJob != null && pawn.CurJob.canBashDoors;
		canBashFences = pawn != null && pawn.CurJob != null && pawn.CurJob.canBashFences;
	}

	public void LogPathfinderState()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Pathfinder State");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Avaliable resources:");
		stringBuilder.AppendLine($"Grid outputs: {gridJobOutputs.Count}");
		stringBuilder.AppendLine($"Path states: {pathUniqueStates.Count}");
		stringBuilder.AppendLine($"Path outputs: {pathJobOutputs.Count}");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine($"Current grid results: {gridJobLookup.Count}");
		foreach (var (mapGridRequest2, scheduledGridJob2) in gridJobLookup)
		{
			stringBuilder.AppendLine($"   - {mapGridRequest2} ({(GenTicks.TicksGame - scheduledGridJob2.computed).TicksToSeconds():F2}s old, last accessed: {(GenTicks.TicksGame - scheduledGridJob2.lastAccessed).TicksToSeconds():F2}s ago)");
		}
		Log.Message(stringBuilder.ToString());
	}

	public void LogGridCellResult(IntVec3 cell)
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (KeyValuePair<MapGridRequest, ScheduledGridJob> item in gridJobLookup)
		{
			item.Deconstruct(out var key, out var value);
			MapGridRequest mapGridRequest = key;
			ScheduledGridJob scheduledGridJob = value;
			ScheduledGridJob local = scheduledGridJob;
			list.Add(new FloatMenuOption($"{mapGridRequest} ({(GenTicks.TicksGame - scheduledGridJob.computed).TicksToSeconds():F2}s old)", delegate
			{
				Log.Message($"{cell}: {local.output.grid[map.cellIndices[cell]]}");
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	public void OnDraw()
	{
	}

	public void OnGUI()
	{
	}

	public void Dispose()
	{
		if (disposed)
		{
			return;
		}
		disposed = true;
		foreach (ScheduledGridJob scheduledGridJob in scheduledGridJobs)
		{
			scheduledGridJob.handle.Complete();
		}
		foreach (ScheduledPathJob scheduledPathJob in scheduledPathJobs)
		{
			scheduledPathJob.handle.Complete();
		}
		foreach (ScheduledGridJob scheduledGridJob2 in scheduledGridJobs)
		{
			scheduledGridJob2.output.Dispose();
		}
		foreach (ScheduledPathJob scheduledPathJob2 in scheduledPathJobs)
		{
			PathUniqueState state = scheduledPathJob2.state;
			state.Dispose();
			PathJobOutput output = scheduledPathJob2.output;
			output.Dispose();
		}
		mapData.Dispose();
		foreach (JobBatchState batch in batches)
		{
			batch.Dispose();
		}
		immediateBatch.Dispose();
		foreach (GridJobOutput gridJobOutput in gridJobOutputs)
		{
			gridJobOutput.Dispose();
		}
		foreach (PathUniqueState pathUniqueState in pathUniqueStates)
		{
			pathUniqueState.Dispose();
		}
		foreach (PathJobOutput pathJobOutput in pathJobOutputs)
		{
			pathJobOutput.Dispose();
		}
	}
}
